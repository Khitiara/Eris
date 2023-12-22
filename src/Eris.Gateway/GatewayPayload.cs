using System.Text.Json;
using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;

namespace Eris.Gateway;

public readonly struct GatewayPayload
{
    [JsonPropertyName("o")]
    public required GatewayOpcode Opcode { get; init; }

    [JsonPropertyName("d")]
    public required object? Data { get; init; }

    [JsonPropertyName("s")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? SequenceNumber { get; init; }

    [JsonPropertyName("t")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EventName { get; init; }
}