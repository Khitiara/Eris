using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

namespace Eris.Rest.RateLimiting;

public class RateLimitHttpHandler(PartitionedRateLimiter<HttpRequestMessage> limiter) : DelegatingHandler
{
    public RateLimitHttpHandler(RateLimiter rateLimiter) : this(
        new SingletonPartitionedRateLimiter<HttpRequestMessage>(rateLimiter)) { }

    private readonly PartitionedRateLimiter<HttpRequestMessage> _rateLimiter = limiter;

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _rateLimiter.Dispose();
        }

        base.Dispose(disposing);
    }

    ~RateLimitHttpHandler() {
        Dispose(false);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) {
        using RateLimitLease lease = await _rateLimiter.AcquireAsync(request, cancellationToken: cancellationToken);
        if (lease.IsAcquired) {
            return await base.SendAsync(request, cancellationToken);
        }

        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter)) {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
        }

        return response;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Note that this non-async send option will return <see cref="HttpStatusCode.TooManyRequests"/> rather than wait
    /// for a lease to be available, and if waiting is desired behavior it should be done manually or
    /// <see cref="SendAsync"/> used instead
    /// </remarks>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) {
        using RateLimitLease lease = _rateLimiter.AttemptAcquire(request);
        if (lease.IsAcquired) {
            return base.Send(request, cancellationToken);
        }

        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter)) {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
        }

        return response;
    }
}