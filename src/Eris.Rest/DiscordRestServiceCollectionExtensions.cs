using Eris.Rest.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Eris.Rest;

public class DiscordRestClientOptions
{
    public int ApiVersion { get; set; } = 10;
}

public static class DiscordRestServiceCollectionExtensions
{
    private const string BotUserAgent =
        $"DiscordBot ({ThisAssembly.ProjectUrl}, {ThisAssembly.VersionThreeComponent})";

    /// <summary>
    /// Configures a named <see cref="HttpClient"/> with name <value>"Eris.Rest"</value> and adds
    /// <see cref="DiscordRestClient"/> as a typed http client to the service collection.
    /// </summary>
    /// <returns>An <see cref="IHttpClientBuilder"/> to allow further configuration of the client behavior</returns>
    public static IHttpClientBuilder AddDiscordRestTypedHttpClient(this IServiceCollection collection) {
        collection.AddOptions<DiscordRestClientOptions>();
        collection.TryAddSingleton<DiscordRateLimiter>();
        collection.TryAddScoped<DiscordRateLimitingHandler>();
        return collection.AddHttpClient("Eris.Rest")
            .AddTypedClient<DiscordRestClient>()
            .AddHttpMessageHandler<DiscordRateLimitingHandler>()
            .ConfigureHttpClient((sp, c) => {
                c.DefaultRequestHeaders.UserAgent.ParseAdd(BotUserAgent);
                c.BaseAddress = new Uri(
                    $"https://discord.com/api/v{sp.GetRequiredService<IOptions<DiscordRestClientOptions>>().Value.ApiVersion}/");
            });
    }
}