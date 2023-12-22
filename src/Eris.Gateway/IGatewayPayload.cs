namespace Eris.Gateway;

public interface IGatewayPayload
{
    GatewayOpcode Opcode { get; }
}

public interface IGatewayPayload<out TData> : IGatewayPayload where TData : IPayloadData
{
    TData Data { get; }
}

public interface IEventPayload : IGatewayPayload
{
    string? EventName { get; }
    int SequenceNumber { get; }
}

public interface IPayloadData;

public record Payload<TData>(GatewayOpcode Opcode, TData Data) : IGatewayPayload<TData>
    where TData : IPayloadData;

public record Dispatch<TData>(string? EventName, int SequenceNumber, TData Data)
    : Payload<TData>(GatewayOpcode.Dispatch, Data), IEventPayload
    where TData : IPayloadData;