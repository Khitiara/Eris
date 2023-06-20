using System.Net;
using System.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;

namespace Eris.Rest;

public static class DiscordPolicies
{
    /// <summary>
    /// Returns a Polly policy that complies with the <value>RetryAfter</value> header of responses with the
    /// HTTP 429 Too Many Requests response code
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> ApiRateLimitRetrier() => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(m => m.StatusCode is HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(6, (i, result, _) => {
            RetryConditionHeaderValue? retryAfter = result.Result.Headers.RetryAfter;
            return retryAfter?.Delta ?? retryAfter?.Date - DateTimeOffset.UtcNow ?? TimeSpan.FromSeconds(1 << i);
        }, (_, _, _, _) => Task.CompletedTask);
}