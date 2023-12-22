using Eris.Gateway.Internal;

namespace Eris.Gateway.Payloads;

[GatewayGeneralPayload(GatewayOpcode.Resume)]
public record Resume : IPayloadData;