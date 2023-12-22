using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;
using NodaTime;

namespace Eris.Rest.Models.Gateway;

public class GatewayConnectionInfo
{
    public required Uri Url { get; set; }
}

public class BotGatewayConnectionInfo
{
    public required Uri Url { get; set; }
    public required int Shards { get; set; }
    public required SessionStartLimit SessionStartLimit { get; set; }
}

public class SessionStartLimit
{
    public required int Total { get; set; }
    public required int Remaining { get; set; }

    [JsonConverter(typeof(DurationMillisConverter))]
    public required Duration ResetAfter { get; set; }

    public required int MaxConcurrency { get; set; }
}