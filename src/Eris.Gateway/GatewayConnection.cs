using System.Buffers;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text.Json;
using Nerdbank.Streams;

namespace Eris.Gateway;

public partial class GatewayConnection
{
    private readonly IAsyncSubject<IGatewayPayload> _payloadSubject = new ConcurrentAsyncAsyncSubject<IGatewayPayload>();

    public IAsyncObservable<IGatewayPayload> Incoming => _payloadSubject;

    private async Task ListenAsync(WebSocket sock, CancellationToken cancellationToken) {
        using Sequence<byte> buffer = new();
        do {
            try {
                ValueWebSocketReceiveResult result = default;
                while (!result.EndOfMessage) {
                    result = await sock.ReceiveAsync(buffer.GetMemory(1024), cancellationToken);
                    buffer.Advance(result.Count);
                }

                switch (result.MessageType) {
                    case WebSocketMessageType.Text:
                    case WebSocketMessageType.Binary:
                        await DoParseDispatchAsync(buffer.AsReadOnlySequence, _payloadSubject);
                        break;
                    case WebSocketMessageType.Close:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally {
                buffer.Reset();
            }
        } while (sock.State is WebSocketState.Open && !cancellationToken.IsCancellationRequested);
    }

    private ValueTask DoParseDispatchAsync(ReadOnlySequence<byte> sequence, IAsyncObserver<IGatewayPayload> dispatch) {
        void AssertRead(ref Utf8JsonReader reader, JsonTokenType tokenType, bool nullable = false) {
            if (!reader.Read() || (tokenType == JsonTokenType.None &&
                                   (reader.TokenType != tokenType ||
                                    nullable && reader.TokenType == JsonTokenType.Null)))
            throw new JsonException("Invalid gateway payload");
        }

        void AssertNullableInt(ref Utf8JsonReader reader, out int? number) {
            if (!reader.Read())
                throw new JsonException("Invalid gateway payload");
            number = reader.TokenType switch {
                JsonTokenType.Null => null,
                JsonTokenType.Number => reader.GetInt32(),
                _ => throw new JsonException("Invalid gateway payload"),
            };
        }

        void VerifyOpcode(GatewayOpcode opcode) {
            if (opcode is GatewayOpcode.None || !Enum.IsDefined(opcode))
                throw new JsonException("Invalid gateway opcode");
        }

        Utf8JsonReader reader = new(sequence);
        GatewayOpcode opcode = GatewayOpcode.None;
        Utf8JsonReader dataReader = default;
        bool hasData = false;
        IPayloadData? data = null;
        int? sequenceNum = null;
        string? eventName = null;
        AssertRead(ref reader, JsonTokenType.StartObject);
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            if (reader.TokenType is not JsonTokenType.PropertyName)
                throw new JsonException("Invalid gateway payload");
            if (reader.ValueTextEquals("o"u8)) {
                AssertRead(ref reader, JsonTokenType.Number);
                opcode = (GatewayOpcode)reader.GetInt32();
                VerifyOpcode(opcode);
            } else if (reader.ValueTextEquals("d"u8)) {
                AssertRead(ref reader, JsonTokenType.None);
                if (opcode is GatewayOpcode.None || (opcode is GatewayOpcode.Dispatch && eventName is null)) {
                    // if we get data before opcode, tag the location by copying the reader and then skip it.
                    dataReader = reader;
                    reader.Skip();
                } else {
                    // read data fast path if opcode before data. no need to copy the reader or rewind
                    ReadGatewayPayload(ref reader, opcode, eventName, out data);
                    hasData = true;
                }
            } else if (reader.ValueTextEquals("s"u8)) {
                AssertNullableInt(ref reader, out sequenceNum);
            } else if (reader.ValueTextEquals("t")) {
                AssertRead(ref reader, JsonTokenType.String, true);
                eventName = reader.GetString();
            }
        }

        if (!hasData)
            ReadGatewayPayload(ref dataReader, opcode, eventName, out data);

        return dispatch.OnNextAsync(ConstructPayload(opcode, sequenceNum, eventName, data));
    }

    private IGatewayPayload ConstructPayload(GatewayOpcode opcode, int? sequenceNumber, string? eventName,
        IPayloadData? data) {
        throw new NotImplementedException();
    }

    private void ReadGatewayPayload(ref Utf8JsonReader reader, GatewayOpcode opcode, string? eventNameNullable,
        out IPayloadData? data) {
        if (opcode is GatewayOpcode.None)
            throw new JsonException("Invalid gateway opcode");
        if (opcode is GatewayOpcode.Dispatch) {
            if (eventNameNullable is not { } eventName) throw new JsonException("Invalid gateway dispatch");
            // TODO parse dispatch
            throw new NotImplementedException();
        } else {
            // TODO parse the other stuff
            throw new NotImplementedException();
        }
    }
}