using Microsoft.CodeAnalysis;

namespace GatewayPayloadSerializationGenerator;

public class Diagnostics
{
    public static DiagnosticDescriptor UnexpectedGatewayConnectionTypeSymbol =
        new("ERIS0001", "Missing type symbol information for GatewayConnection type", "", "Eris",
            DiagnosticSeverity.Error, true);
}