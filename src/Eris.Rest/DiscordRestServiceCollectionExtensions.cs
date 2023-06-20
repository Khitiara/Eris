using Microsoft.Extensions.DependencyInjection;

namespace Eris.Rest;

public static class DiscordRestServiceCollectionExtensions
{
    /// <summary>
    /// Configures a named <see cref="HttpClient"/> with name <value>"Eris.Rest"</value> and with a Polly policy to
    /// comply with the HTTP 429 Too Many Requests response code's <value>RetryAfter</value> header as returned by
    /// Discord's REST Api for rate limiting, and adds <see cref="DiscordRestClient"/> as a typed http client to the
    /// service collection.
    /// </summary>
    /// <returns>An <see cref="IHttpClientBuilder"/> to allow further configuration of the client behavior</returns>
    public static IHttpClientBuilder AddDiscordRestTypedHttpClient(this IServiceCollection collection) =>
        collection.AddDiscordApiPolicies().AddTypedClient<DiscordRestClient>();

    /// <summary>
    /// Configures a named <see cref="HttpClient"/> with name <value>"Eris.Rest"</value> and with a Polly policy to
    /// comply with the HTTP 429 Too Many Requests response code's <value>RetryAfter</value> header as returned by
    /// Discord's REST Api for rate limiting.
    /// </summary>
    /// <returns>An <see cref="IHttpClientBuilder"/> to allow further configuration of the client behavior</returns>
    public static IHttpClientBuilder AddDiscordApiPolicies(this IServiceCollection collection) =>
        collection.AddHttpClient("Eris.Rest").AddPolicyHandler(DiscordPolicies.ApiRateLimitRetrier());
}