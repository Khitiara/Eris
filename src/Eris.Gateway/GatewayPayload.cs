using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Eris.Gateway;

public readonly struct GatewayPayload
{
    [JsonPropertyName("o")]
    public GatewayOpcode Opcode { get; init; }

    [JsonPropertyName("d")]
    public JsonNode? Data { get; init; }

    [JsonPropertyName("s")]
    public int? SequenceNumber { get; init; }

    [JsonPropertyName("t")]
    public string? EventName { get; init; }

    public GatewayPayload<T> Resolve<T>(JsonSerializerOptions? serializerOptions = null)
        where T : struct, IGatewayPayload {
        if (T.Opcode != Opcode)
            throw new ArgumentException($"Payload data type {typeof(T).FullName} does not match opcode {Opcode}",
                nameof(T));
        return new GatewayPayload<T> {
            Opcode = Opcode,
            Data = Data?.Deserialize<T>(serializerOptions),
            SequenceNumber = SequenceNumber,
            EventName = EventName
        };
    }
}