using System.Threading.RateLimiting;

namespace Eris.Rest.RateLimiting;

public sealed class SingletonPartitionedRateLimiter<T>(RateLimiter innerLimiter) : PartitionedRateLimiter<T>
{
    private readonly RateLimiter _innerLimiter = innerLimiter;

    public override RateLimiterStatistics? GetStatistics(T resource) => _innerLimiter.GetStatistics();

    protected override RateLimitLease AttemptAcquireCore(T resource, int permitCount) =>
        _innerLimiter.AttemptAcquire(permitCount);

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(T resource, int permitCount,
        CancellationToken cancellationToken) =>
        _innerLimiter.AcquireAsync(permitCount, cancellationToken);

    protected override async ValueTask DisposeAsyncCore() {
        await _innerLimiter.DisposeAsync();
        await base.DisposeAsyncCore();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _innerLimiter.Dispose();
        }

        base.Dispose(disposing);
    }
}