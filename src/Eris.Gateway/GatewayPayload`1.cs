using System.Text.Json.Serialization;

namespace Eris.Gateway;

public readonly struct GatewayPayload<T> where T : struct, IGatewayPayload
{
    [JsonPropertyName("o")]
    public GatewayOpcode Opcode { get; init; }

    [JsonPropertyName("d")]
    public T? Data { get; init; }

    [JsonPropertyName("s")]
    public int? SequenceNumber { get; init; }

    [JsonPropertyName("t")]
    public string? EventName { get; init; }

    public static Type PayloadType { get; } = typeof(T);
}