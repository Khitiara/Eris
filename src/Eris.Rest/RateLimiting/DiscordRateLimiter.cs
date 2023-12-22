using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Eris.Rest.Models;
using Eris.Rest.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NodaTime;

namespace Eris.Rest.RateLimiting;

public sealed class DiscordRateLimiter : IDisposable
{
    public static readonly HttpRequestOptionsKey<Route> RateLimitingRouteDetailsKey = new("ERIS_RATELIMIT_DETAILS");

    private readonly ILoggerFactory                    _loggerFactory;
    private readonly Logger<DiscordRateLimiter>        _logger;
    private readonly IClock                            _clock;
    private readonly ObjectPool<Bucket.Token>          _tokenPool;
    private readonly object                            _bucketDictionariesLock = new();
    private readonly Dictionary<RouteTemplate, string> _hashes                 = new();
    private readonly Dictionary<BucketId, Bucket>      _buckets                = new();
    private readonly HashSet<RouteTemplate>            _hitRateLimits          = new();

    private event Action? Shutdown;

    public DiscordRateLimiter(ILoggerFactory loggerFactory, ObjectPoolProvider poolProvider,
        Logger<DiscordRateLimiter> logger, IClock clock) {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _clock = clock;
        _tokenPool = poolProvider.Create<Bucket.Token>();
    }

    private class Bucket
    {
        private readonly bool _isUnlimited;
        public int Limit { get; internal set; } = 1;
        public int Remaining { get; internal set; } = 1;
        public Instant ResetsAt { get; internal set; }
        private readonly ILogger<Bucket>    _logger;
        private readonly DiscordRateLimiter _limiter;
        private readonly Channel<Token>     _channel;

        public Bucket(DiscordRateLimiter limiter, bool isUnlimited) {
            _limiter = limiter;
            _isUnlimited = isUnlimited;
            _logger = _limiter._loggerFactory.CreateLogger<Bucket>();
            _channel = Channel.CreateUnbounded<Token>(new UnboundedChannelOptions() { SingleReader = true, });
            RunAsync();
            _limiter.Shutdown += () => _channel.Writer.TryComplete();
        }

        private async void RunAsync() {
            ChannelReader<Token> reader = _channel.Reader;
            await foreach (Token token in reader.ReadAllAsync().ConfigureAwait(false)) {
                if (token.CancellationToken.IsCancellationRequested) {
                    _limiter._tokenPool.Return(token);
                    continue;
                }

                HttpRequestMessage req = token.Request;
                try {
                    while (await RunSingleAttemptAsync(token, req)) { }
                }
                finally {
                    _limiter._tokenPool.Return(token);
                }
            }
        }

        private async ValueTask<bool> RunSingleAttemptAsync(Token token, HttpRequestMessage req) {
            try {
                if (_isUnlimited && _limiter.TryGetBucket(token.Route, out Bucket? bucket) && this != bucket) {
                    _logger.MovingRequestToLimitedBucket(token.Route);
                    await bucket.PostAsync(token).ConfigureAwait(false);
                    return false;
                }

                Instant globalReset = _limiter.GlobalResetsAt;
                Instant now = _limiter._clock.GetCurrentInstant();
                bool isGlobalLimit = globalReset > now;
                if (Remaining == 0 || isGlobalLimit) {
                    Duration delay = isGlobalLimit
                        ? Duration.Max(globalReset - now, ResetsAt - now)
                        : ResetsAt - now;
                    if (delay > Duration.Zero) {
                        await Task.Delay(delay.ToTimeSpan());
                    }
                }

                HttpResponseMessage response = await token.InnerHandler(req, token.CancellationToken)
                    .ConfigureAwait(false);
                if (_limiter.UpdateBucket(token.Route, response)) {
                    return true;
                }

                token.TrySetResult(response);
                return false;
            }
            catch (Exception e) {
                token.TrySetException(e);
                if (e is not OperationCanceledException or TimeoutException) {
                    _logger.UnexpectedRequestException(token.Route, e);
                }

                return false;
            }
        }

        public async ValueTask<Token> PostAsync(HttpRequestMessage message, Route route,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> innerHandler,
            CancellationToken cancellationToken) {
            Token token = _limiter._tokenPool.Get();
            token.Init(message, route, innerHandler, cancellationToken);
            await PostAsync(token);
            return token;
        }

        private ValueTask PostAsync(Token token) => _channel.Writer.WriteAsync(token, token.CancellationToken);

        public sealed class Token : IResettable, IDisposable
        {
            private TaskCompletionSource<HttpResponseMessage> _tcs = null!;
            private CancellationTokenRegistration             _registration;
            public HttpRequestMessage Request { get; private set; } = null!;

            public Route Route { get; private set; }
            public CancellationToken CancellationToken { get; private set; }

            public Task<HttpResponseMessage> Task => _tcs.Task;

            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> InnerHandler {
                get;
                private set;
            } =
                null!;

            public void Init(HttpRequestMessage request, Route route,
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> innerHandler,
                CancellationToken token) {
                Request = request;
                Route = route;
                CancellationToken = token;
                _tcs = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions
                    .RunContinuationsAsynchronously);
                InnerHandler = innerHandler;
                _registration = CancellationToken.UnsafeRegister(Callback, _tcs);

                return;

                static void Callback(object? state, CancellationToken token) {
                    Unsafe.As<TaskCompletionSource<HttpResponseMessage>>(state)!.TrySetCanceled(token);
                }
            }

            public void TrySetResult(HttpResponseMessage responseMessage) {
                _tcs.TrySetResult(responseMessage);
            }

            public void TrySetException(Exception e) {
                _tcs.TrySetException(e);
            }

            public bool TryReset() {
                _registration.Dispose();
                _tcs.TrySetCanceled();
                return true;
            }

            public void Dispose() {
                _registration.Dispose();
            }
        }
    }

    private bool UpdateBucket(Route route, HttpResponseMessage response) {
        try {
            lock (_bucketDictionariesLock) {
                bool hasBucketHashHeader =
                    response.Headers.TryGetSingleHeader(RateLimitingHeaders.XRateLimitBucket, out string? bucketHash);
                if (hasBucketHashHeader && _hashes.TryAdd(route.Template, bucketHash!)) {
                    _logger.CreatedBucketHash(route, bucketHash!);
                }

                GetOrCreateBucket(route, out Bucket bucket);
                if (response.StatusCode == HttpStatusCode.TooManyRequests) {
                    RateLimitingHeaders.ReadRetryAfter(response, _clock, out Instant retryAfterDate,
                        out Duration retryAfterDelta);
                    if (response.Headers.TryGetSingleHeader(RateLimitingHeaders.XRateLimitGlobal, out bool global) &&
                        global || response.Headers.Via.Count == 0) {
                        string type = global ? "global" : "Cloudflare";
                        _logger.HitGlobalRateLimit(type, retryAfterDelta);
                        GlobalResetsAt = retryAfterDate;
                        return true;
                    } else {
                        bucket.Remaining = 0;
                        bucket.ResetsAt = retryAfterDate;
                        RateLimitingHeaders.RateLimitScope scope =
                            response.Headers.GetSingleHeaderOrDefault(RateLimitingHeaders.XRateLimitScope);
                        _logger.HitRateLimit(
                            _hitRateLimits.Add(route.Template) && retryAfterDelta.TotalSeconds < 30 ||
                            scope == RateLimitingHeaders.RateLimitScope.Shared
                                ? LogLevel.Information
                                : LogLevel.Warning, scope, route, retryAfterDelta);

                        return true;
                    }
                }

                if (hasBucketHashHeader) {
                    bucket.Limit = response.Headers.GetSingleHeaderOrDefault(RateLimitingHeaders.XRateLimitLimit);
                    bucket.Remaining =
                        response.Headers.GetSingleHeaderOrDefault(RateLimitingHeaders.XRateLimitRemaining);
                    Duration resetAfter =
                        response.Headers.GetSingleHeaderOrDefault(RateLimitingHeaders.XRateLimitResetAfter);
                    bucket.ResetsAt = _clock.GetCurrentInstant() + resetAfter;

                    _logger.UpdatedBucket(route, bucket.Remaining, bucket.Limit, resetAfter);
                }

                return false;
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return false;
        }
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> innerHandler,
        CancellationToken cancellationToken) {
        if (!request.Options.TryGetValue(RateLimitingRouteDetailsKey, out Route route)) {
            return innerHandler(request, cancellationToken);
        }

        GetOrCreateBucket(route, out Bucket bucket);
        return Core(bucket, request, route, innerHandler, cancellationToken);

        static async Task<HttpResponseMessage> Core(Bucket bucket, HttpRequestMessage request, Route route,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> innerHandler,
            CancellationToken cancellationToken) {
            Bucket.Token tok =
                await bucket.PostAsync(request, route, innerHandler, cancellationToken).ConfigureAwait(false);
            return await tok.Task.ConfigureAwait(false);
        }
    }

    public Instant GlobalResetsAt { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BucketId GetBucketId(Route route, out bool unlimited) {
        unlimited = !_hashes.TryGetValue(route.Template, out string? hash);
        hash ??= $"unlimited+{route.Template.ToString()}";
        BucketId id = new(hash, unlimited,
            new Snowflake(route.Parameters.GetValueOrDefault<ulong>("guild")),
            new Snowflake(route.Parameters.GetValueOrDefault<ulong>("channel")),
            new Snowflake(route.Parameters.GetValueOrDefault<ulong>("webhook")));
        return id;
    }

    private bool TryGetBucket(Route route, [NotNullWhen(true)] out Bucket? bucket) {
        lock (_bucketDictionariesLock) {
            return _buckets.TryGetValue(GetBucketId(route, out _), out bucket);
        }
    }

    private void GetOrCreateBucket(Route route, out Bucket bucket) {
        lock (_bucketDictionariesLock) {
            ref Bucket? bucketRef =
                ref CollectionsMarshal.GetValueRefOrAddDefault(_buckets, GetBucketId(route, out bool unlimited), out _);
            bucket = bucketRef ??= new Bucket(this, unlimited);
        }
    }

    public void Dispose() {
        if (true) {
            if (_tokenPool is IDisposable disposable)
                disposable.Dispose(); // this should always be true but cant guarantee ig
            Shutdown?.Invoke();
        }
    }
}