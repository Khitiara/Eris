using Eris.Rest.Models.Applications;
using Eris.Rest.Routing;

namespace Eris.Rest;

public partial class DiscordRestClient
{
    private static partial class Routes
    {
        public static readonly Route GetCurrentApplication   = RouteTemplate.Get("oauth2/applications/@me").Expand();
        public static readonly Route GetCurrentAuthorization = RouteTemplate.Get("oauth2/@me").Expand();
    }

    public Task<Application?> GetCurrentApplicationAsync(CancellationToken cancellationToken = default) =>
        SendAsync<Application>(Routes.GetCurrentApplication, cancellationToken: cancellationToken);
}