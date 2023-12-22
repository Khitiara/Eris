using System.Net.Http.Json;
using Eris.Rest.Models.Json;
using Eris.Rest.RateLimiting;
using Eris.Rest.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eris.Rest;

[method: ActivatorUtilitiesConstructor]
public sealed partial class DiscordRestClient(
    HttpClient client,
    IOptions<DiscordRestClientOptions> options,
    ILogger<DiscordRestClient> logger) : IDisposable
{
    private          Token                      _token   = Token.None;
    private readonly DiscordRestClientOptions   _options = options.Value;
    private readonly ILogger<DiscordRestClient> _logger  = logger;

    public DiscordRestClient(IHttpClientFactory clientFactory, IOptions<DiscordRestClientOptions> options,
        ILogger<DiscordRestClient> logger) : this(clientFactory.CreateClient("Eris.Rest"), options, logger) { }

    public Task<HttpResponseMessage> SendAsync(Route route, HttpContent? content = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) {
        HttpRequestMessage request = new(route.Template.Method, route.ToUri()) {
            Content = content,
            Version = HttpClient.DefaultRequestVersion,
            VersionPolicy = HttpClient.DefaultVersionPolicy
        };
        request.Options.Set(DiscordRateLimiter.RateLimitingRouteDetailsKey, route);
        return HttpClient.SendAsync(request, completionOption, cancellationToken);
    }

    public async Task<TModel?> SendAsync<TModel>(Route route, HttpContent? content = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) {
        HttpResponseMessage resp = await SendAsync(route, content, completionOption, cancellationToken)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TModel>(DiscordJsonContext.Default.Options, cancellationToken)
            .ConfigureAwait(false);
    }

    public Token Token {
        get => _token;
        set {
            _token = value;
            HttpClient.DefaultRequestHeaders.Authorization = _token.AuthorizationHeader;
        }
    }

    public HttpClient HttpClient { get; } = client;

    public void Dispose() {
        HttpClient.Dispose();
    }
}