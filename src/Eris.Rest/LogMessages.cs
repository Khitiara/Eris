using System.Net;
using System.Net.Http.Headers;
using Eris.Rest.RateLimiting;
using Eris.Rest.Routing;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Eris.Rest;

internal static partial class LogMessages
{
    [LoggerMessage(LogLevel.Debug, "Moving request for route {Route} to limited request bucket")]
    public static partial void MovingRequestToLimitedBucket(this ILogger logger, Route route);

    [LoggerMessage(LogLevel.Error, "Encountered unexpected exception when processing request for route {Route}")]
    public static partial void UnexpectedRequestException(this ILogger logger, Route route, Exception ex);

    [LoggerMessage(LogLevel.Trace, "Cached bucket hash {Route} -> {Hash}")]
    public static partial void CreatedBucketHash(this ILogger logger, Route route, string hash);


    [LoggerMessage("Hit a {Kind} rate-limit on {Route}, Expires after {Duration}")]
    public static partial void HitRateLimit(this ILogger logger, LogLevel level,
        RateLimitingHeaders.RateLimitScope kind,
        Route route, Duration duration);


    [LoggerMessage(LogLevel.Error, "Hit a {Kind} rate-limit, Expires after {Duration}")]
    public static partial void HitGlobalRateLimit(this ILogger logger, string kind, Duration duration);

    [LoggerMessage(LogLevel.Debug, "Updated bucket for route {Route} to ({Remaining}/{Limit}, {ResetsAfter})")]
    public static partial void UpdatedBucket(this ILogger logger, Route route, int remaining, int limit,
        Duration resetsAfter);

    [LoggerMessage(LogLevel.Error, "Encountered exception updating bucket for {Route}; Http Status: {StatusCode}, Headers: {ResponseHeaders}")]
    public static partial void ExceptionUpdatingBucket(this ILogger logger, Exception ex, Route route, HttpStatusCode statusCode,
        HttpResponseHeaders responseHeaders);
}