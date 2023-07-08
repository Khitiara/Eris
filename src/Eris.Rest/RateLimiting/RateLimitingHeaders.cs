using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Eris.Rest.Models;

namespace Eris.Rest.RateLimiting;

public static class RateLimitingHeaders
{
    public const string XRateLimitLimit      = "X-RateLimit-Limit";
    public const string XRateLimitRemaining  = "X-RateLimit-Remaining";
    public const string XRateLimitReset      = "X-RateLimit-Reset";
    public const string XRateLimitResetAfter = "X-RateLimit-Reset-After";
    public const string XRateLimitBucket     = "X-RateLimit-Bucket";
    public const string XRateLimitGlobal     = "X-RateLimit-Global";
    public const string XRateLimitScope      = "X-RateLimit-Scope";


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

    internal enum RateLimitScope
    {
        User,
        Shared,
        Global,
    }

    internal static TimeSpan? RetryAfterDelta(HttpResponseMessage response) {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        return retryAfter?.Delta ?? retryAfter?.Date - DateTimeOffset.UtcNow;
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

    internal static bool TryParseSingleHeader<T>(this HttpResponseHeaders headers, string header,
        [MaybeNullWhen(false)] out T t)
        where T : IParsable<T> {
        Unsafe.SkipInit(out t);
        return TryGetSingleHeader(headers, header, out string s) && T.TryParse(s, CultureInfo.InvariantCulture, out t);
    }

    internal static async ValueTask<RateLimitTriggerInfo> UpdateRateLimitsFromRestResponse(
        DiscordApiRateLimiter limiter,
        DiscordApiRateLimiter.Bucket bucket, HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default) {
        bool global = responseMessage.Headers.TryGetSingleHeader(XRateLimitGlobal, out string s)
                      && s.Equals("true", StringComparison.InvariantCultureIgnoreCase);

        TimeSpan retryAfter = RetryAfterDelta(responseMessage) ?? TimeSpan.Zero;

        RateLimitScope scope = responseMessage.Headers.TryGetSingleHeader(XRateLimitScope, out s)
            ? s switch {
                "user" => RateLimitScope.User,
                "shared" => RateLimitScope.Shared,
                "global" => RateLimitScope.Global,
                _ => RateLimitScope.User,
            }
            : RateLimitScope.User;

        if (responseMessage.Headers.TryParseSingleHeader(XRateLimitResetAfter, out double seconds)) {
            retryAfter = TimeSpan.FromSeconds(seconds);
        }


        // we separate per-resource buckets so no need to check user vs shared here 
        if (!global) {
            UpdateBucket(bucket, responseMessage, retryAfter);
        }


        if (responseMessage.StatusCode is not HttpStatusCode.TooManyRequests) {
            return new RateLimitTriggerInfo {
                IsLimited = false,
            };
        }


        RateLimitResponsePayload payload = await responseMessage.Content.ReadFromJsonAsync(
            DiscordJsonContext.Default.RateLimitResponsePayload, cancellationToken);

        if (payload.Global)
            scope = RateLimitScope.Global;

        RateLimitTriggerInfo info = new() {
            IsLimited = true,
            Message = payload.Message,
            RetryAfter = retryAfter = TimeSpan.FromSeconds(payload.RetryAfter),
        };

        if (!global) {
            bucket.ResetAfter = retryAfter;
        } else {
            limiter.NotifyGlobalRateLimit(info, scope);
        }

        return info;
    }

    private static void UpdateBucket(DiscordApiRateLimiter.Bucket bucket, HttpResponseMessage responseMessage,
        TimeSpan retryAfter) {
        bucket.ResetAfterOffset = DateTimeOffset.UtcNow + (bucket.ResetAfter = retryAfter).Value;

        if (responseMessage.Headers.TryGetSingleHeader(XRateLimitBucket, out string s))
            bucket.Hash = s;

        if (responseMessage.Headers.TryParseSingleHeader(XRateLimitReset, out double epoch))
            bucket.Reset = DateTimeOffset.UnixEpoch.AddSeconds(epoch);

        if (responseMessage.Headers.TryParseSingleHeader(XRateLimitLimit, out int limit))
            bucket.Maximum = limit;

        if (responseMessage.Headers.TryParseSingleHeader(XRateLimitRemaining, out int remaining))
            bucket.InternalRemaining = remaining;
    }
}