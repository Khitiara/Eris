using System.Net.Http.Headers;

namespace Eris.Rest;

public sealed class DiscordRestClient : IDisposable
{
    public DiscordRestClient(HttpClient client) {
        HttpClient = client;
        HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Eris", "0.0.1"/*ThisAssembly.Info.Version*/));
    }

    public DiscordRestClient(IHttpClientFactory clientFactory) : this(clientFactory.CreateClient("Eris.Rest")) { }

    public HttpClient HttpClient { get; }

    public void Dispose() {
        HttpClient.Dispose();
    }
}