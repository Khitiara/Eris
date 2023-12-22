namespace Eris.Gateway.Internal;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class GatewayGeneralPayloadAttribute(GatewayOpcode opcode) : Attribute
{
    public GatewayOpcode Opcode { get; } = opcode;
    public bool Incoming { get; init; } = true;
    public bool Outgoing { get; init; } = false;
}