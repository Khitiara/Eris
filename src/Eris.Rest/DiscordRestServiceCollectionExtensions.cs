using Microsoft.Extensions.DependencyInjection;

namespace Eris.Rest;

public static class DiscordRestServiceCollectionExtensions
{
    /// <summary>
    /// Configures a named <see cref="HttpClient"/> with name <value>"Eris.Rest"</value> and adds
    /// <see cref="DiscordRestClient"/> as a typed http client to the service collection.
    /// </summary>
    /// <returns>An <see cref="IHttpClientBuilder"/> to allow further configuration of the client behavior</returns>
    public static IHttpClientBuilder AddDiscordRestTypedHttpClient(this IServiceCollection collection) =>
        collection.AddHttpClient("Eris.Rest").AddTypedClient<DiscordRestClient>();
}