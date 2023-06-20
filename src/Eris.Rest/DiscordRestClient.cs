using System.Net.Http.Headers;

namespace Eris.Rest;

public sealed class DiscordRestClient : IDisposable
{
    private readonly HttpClient _client;

    public DiscordRestClient(HttpClient client) {
        _client = client;
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Eris", ThisAssembly.Info.Version));
    }

    public DiscordRestClient(IHttpClientFactory clientFactory) {
        _client = clientFactory.CreateClient("Eris");
    }

    public void Dispose() {
        _client.Dispose();
    }
}