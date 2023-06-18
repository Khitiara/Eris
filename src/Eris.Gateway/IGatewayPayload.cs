namespace Eris.Gateway;

public interface IGatewayPayload
{
    public static abstract GatewayOpcode Opcode { get; }
}