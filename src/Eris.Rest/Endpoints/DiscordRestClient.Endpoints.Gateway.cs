using Eris.Rest.Models.Gateway;
using Eris.Rest.Routing;

namespace Eris.Rest;

public partial class DiscordRestClient
{
    private static partial class Routes
    {
        public static readonly Route GetGateway    = RouteTemplate.Get("/gateway").Expand();
        public static readonly Route GetGatewayBot = RouteTemplate.Get("/gateway/bot").Expand();
    }

    public Task<GatewayConnectionInfo?> GetGatewayAsync(CancellationToken cancellationToken = default) =>
        SendAsync<GatewayConnectionInfo>(Routes.GetGateway, cancellationToken: cancellationToken);

    public Task<BotGatewayConnectionInfo?> GetGatewayBotAsync(CancellationToken cancellationToken = default) =>
        SendAsync<BotGatewayConnectionInfo>(Routes.GetGatewayBot, cancellationToken: cancellationToken);
}