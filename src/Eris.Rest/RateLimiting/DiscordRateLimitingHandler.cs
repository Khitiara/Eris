namespace Eris.Rest.RateLimiting;

public sealed class DiscordRateLimitingHandler(DiscordRateLimiter rateLimiter) : DelegatingHandler
{
    private readonly DiscordRateLimiter _rateLimiter = rateLimiter;

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) {
        return _rateLimiter.SendAsync(request, Inner, cancellationToken).GetAwaiter().GetResult();

        Task<HttpResponseMessage> Inner(HttpRequestMessage m, CancellationToken c) => Task.FromResult(base.Send(m, c));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) => _rateLimiter.SendAsync(request, base.SendAsync, cancellationToken);
}