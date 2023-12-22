using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using NodaTime;

namespace Eris.Rest.RateLimiting;

public delegate bool TryParseDelegate<T>(string str, [NotNullWhen(true)] out T? value);

public readonly struct HeaderKey<T>(string key, TryParseDelegate<T> tryParseDelegate)
{
    public string Key { get; } = key;
    private readonly TryParseDelegate<T> _tryParseDelegate = tryParseDelegate;

    public bool TryParse(string headerValue, [NotNullWhen(true)] out T? value) =>
        _tryParseDelegate(headerValue, out value);

    public HeaderKey<TResult> Select<TResult>(Func<T, TResult> selectFunction) {
        TryParseDelegate<T>? inner = _tryParseDelegate;
        return new HeaderKey<TResult>(Key, (string str, out TResult? value) => {
            if (inner(str, out T? t1)) {
                value = selectFunction(t1);
                return true;
            }

            value = default;
            return false;
        });
    }
}

public static class HeaderKey
{
    public static HeaderKey<T> Create<T>(string key)
        where T : IParsable<T> => new(key,
        static (string str, out T? value) => T.TryParse(str, null, out value));

    public static HeaderKey<T> Create<T>(string key, TryParseDelegate<T> tryParseDelegate) =>
        new(key, tryParseDelegate);
}

public static class RateLimitingHeaders
{
    public static readonly HeaderKey<int> XRateLimitLimit     = HeaderKey.Create<int>("X-RateLimit-Limit");
    public static readonly HeaderKey<int> XRateLimitRemaining = HeaderKey.Create<int>("X-RateLimit-Remaining");

    public static readonly HeaderKey<Instant> XRateLimitReset = HeaderKey.Create<double>("X-RateLimit-Reset")
        .Select(Duration.FromSeconds).Select(FromDuration);

    public static readonly HeaderKey<Duration> XRateLimitResetAfter =
        HeaderKey.Create<double>("X-RateLimit-Reset-After").Select(Duration.FromSeconds);

    public static readonly HeaderKey<string> XRateLimitBucket = HeaderKey.Create<string>("X-RateLimit-Bucket");
    public static readonly HeaderKey<bool>   XRateLimitGlobal = HeaderKey.Create<bool>("X-RateLimit-Global");

    public static readonly HeaderKey<RateLimitScope> XRateLimitScope = HeaderKey.Create<string>("X-RateLimit-Scope")
        .Select(s => Enum.TryParse(s, out RateLimitScope scope) ? scope : RateLimitScope.User);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern Instant FromDuration(Duration duration);


    public readonly struct RateLimitResponsePayload
    {
        public required string Message { get; init; }

        public required float RetryAfter { get; init; }

        public required bool Global { get; init; }

        public int? Code { get; init; }
    }

    public readonly struct RateLimitTriggerInfo
    {
        public bool IsLimited { get; init; }
        public string Message { get; init; }
        public TimeSpan RetryAfter { get; init; }
    }

    public enum RateLimitScope
    {
        User,
        Shared,
        Global,
    }

    internal static Duration? RetryAfterDelta(HttpResponseMessage response, IClock clock) {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        return retryAfter switch {
            { Delta: { } ts } => Duration.FromTimeSpan(ts),
            { Date: { } d } => Instant.FromDateTimeOffset(d) - clock.GetCurrentInstant(),
            _ => null,
        };
    }

    internal static void ReadRetryAfter(HttpResponseMessage response, IClock clock, out Instant instant,
        out Duration delta) {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        Instant now = clock.GetCurrentInstant();
        switch (retryAfter) {
            case { Delta: { } ts }:
                instant = now + (delta = Duration.FromTimeSpan(ts));
                break;
            case { Date: { } d }:
                delta = (instant = Instant.FromDateTimeOffset(d)) - now;
                break;
            default:
                instant = default;
                delta = default;
                break;
        }
    }

    internal static bool TryGetSingleHeader(this HttpResponseHeaders headers, string header, out string s) {
        if (headers.TryGetValues(header, out IEnumerable<string>? values)
            && (values as string[] ?? values.ToArray()) is [var s2, ..,]) {
            s = s2;
            return true;
        }

        s = "";
        return false;
    }

    internal static bool TryGetSingleHeader<T>(this HttpResponseHeaders headers, HeaderKey<T> key,
        [NotNullWhen(true)] out T? value) {
        if (TryGetSingleHeader(headers, key.Key, out string s) && key.TryParse(s, out value)) {
            return true;
        }

        value = default;
        return false;
    }

    internal static T GetSingleHeaderOrDefault<T>(this HttpResponseHeaders headers, HeaderKey<T> key,
        T defaultValue = default!) => TryGetSingleHeader(headers, key, out T? value) ? value : defaultValue;

    // internal static async ValueTask<RateLimitTriggerInfo> UpdateRateLimitsFromRestResponse(
    //     DiscordApiRateLimiter limiter,
    //     DiscordApiRateLimiter.Bucket bucket, HttpResponseMessage responseMessage,
    //     CancellationToken cancellationToken = default) {
    //     bool global = responseMessage.Headers.TryGetSingleHeader(XRateLimitGlobal, out string s)
    //                   && s.Equals("true", StringComparison.InvariantCultureIgnoreCase);
    //
    //     TimeSpan retryAfter = RetryAfterDelta(responseMessage) ?? TimeSpan.Zero;
    //
    //     RateLimitScope scope = responseMessage.Headers.TryGetSingleHeader(XRateLimitScope, out s)
    //         ? s switch {
    //             "user" => RateLimitScope.User,
    //             "shared" => RateLimitScope.Shared,
    //             "global" => RateLimitScope.Global,
    //             _ => RateLimitScope.User,
    //         }
    //         : RateLimitScope.User;
    //
    //     if (responseMessage.Headers.TryParseSingleHeader(XRateLimitResetAfter, out double seconds)) {
    //         retryAfter = TimeSpan.FromSeconds(seconds);
    //     }
    //
    //
    //     // we separate per-resource buckets so no need to check user vs shared here
    //     if (!global) {
    //         UpdateBucket(bucket, responseMessage, retryAfter);
    //     }
    //
    //
    //     if (responseMessage.StatusCode is not HttpStatusCode.TooManyRequests) {
    //         return new RateLimitTriggerInfo {
    //             IsLimited = false,
    //         };
    //     }
    //
    //
    //     RateLimitResponsePayload payload = await responseMessage.Content.ReadFromJsonAsync(
    //         DiscordJsonContext.Default.RateLimitResponsePayload, cancellationToken);
    //
    //     if (payload.Global)
    //         scope = RateLimitScope.Global;
    //
    //     RateLimitTriggerInfo info = new() {
    //         IsLimited = true,
    //         Message = payload.Message,
    //         RetryAfter = retryAfter = TimeSpan.FromSeconds(payload.RetryAfter),
    //     };
    //
    //     if (!global) {
    //         bucket.ResetAfter = retryAfter;
    //     } else {
    //         limiter.NotifyGlobalRateLimit(info, scope);
    //     }
    //
    //     return info;
    // }

    // private static void UpdateBucket(DiscordApiRateLimiter.Bucket bucket, HttpResponseMessage responseMessage,
    //     TimeSpan retryAfter) {
    //     bucket.ResetAfterOffset = DateTimeOffset.UtcNow + (bucket.ResetAfter = retryAfter).Value;
    //
    //     if (responseMessage.Headers.TryGetSingleHeader(XRateLimitBucket, out string s))
    //         bucket.Hash = s;
    //
    //     if (responseMessage.Headers.TryParseSingleHeader(XRateLimitReset, out double epoch))
    //         bucket.Reset = DateTimeOffset.UnixEpoch.AddSeconds(epoch);
    //
    //     if (responseMessage.Headers.TryParseSingleHeader(XRateLimitLimit, out int limit))
    //         bucket.Maximum = limit;
    //
    //     if (responseMessage.Headers.TryParseSingleHeader(XRateLimitRemaining, out int remaining))
    //         bucket.InternalRemaining = remaining;
    // }
}