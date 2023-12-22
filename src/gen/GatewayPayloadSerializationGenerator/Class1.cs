using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GatewayPayloadSerializationGenerator;

public class GatewayPayloadSerializationGen : IIncrementalGenerator
{
    private readonly record struct OrDiagnostic<T>(T Info, params Diagnostic[] Diag);

    private readonly record struct GatewayConnTypeInfo(string ConnectionTypeName);

    private readonly record struct PayloadTypeInfo(string TypeName, string OpcodeName, bool Incoming, bool Outgoing);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValueProvider<OrDiagnostic<GatewayConnTypeInfo>> conntype = context.SyntaxProvider
            .CreateSyntaxProvider(CheckGatewayConnectionType, EstablishGatewayConnTypeInfo)
            .Collect().Select((a, _) => a.First());
        IncrementalValueProvider<(OrDiagnostic<(ImmutableArray<PayloadTypeInfo>, GatewayConnTypeInfo)> Left,
            Compilation Right)> generalPayloads = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Eris.Gateway.Internal.GatewayGeneralPayloadAttribute",
                CheckVerifyPayloadType, DeducePayloadTypeInfo)
            .Collect()
            .Select(static (array, _) => {
                List<Diagnostic> diags = new();
                ImmutableArray<PayloadTypeInfo>.Builder infos = ImmutableArray.CreateBuilder<PayloadTypeInfo>();
                foreach ((PayloadTypeInfo payloadTypeInfo, Diagnostic[] diagnostics) in array) {
                    diags.AddRange(diagnostics);
                    infos.Add(payloadTypeInfo);
                }

                return new OrDiagnostic<ImmutableArray<PayloadTypeInfo>>(infos.ToImmutable(), diags.ToArray());
            })
            .Combine(conntype)
            .Select(static (tuple, _) => {
                Diagnostic[] diags = new Diagnostic[tuple.Left.Diag.Length + tuple.Right.Diag.Length];
                tuple.Left.Diag.CopyTo(diags, 0);
                tuple.Right.Diag.CopyTo(diags, tuple.Left.Diag.Length);
                return new OrDiagnostic<(ImmutableArray<PayloadTypeInfo>, GatewayConnTypeInfo)>(
                    (tuple.Left.Info, tuple.Right.Info), diags);
            })
            .Combine(context.CompilationProvider);

        // context.RegisterSourceOutput();
    }

    private static OrDiagnostic<GatewayConnTypeInfo> EstablishGatewayConnTypeInfo(GeneratorSyntaxContext arg1,
        CancellationToken arg2) {
        ISymbol? typeSym = arg1.SemanticModel.GetDeclaredSymbol(arg1.Node, arg2);
        if (typeSym is null)
            return new OrDiagnostic<GatewayConnTypeInfo>(default,
                Diagnostic.Create(Diagnostics.UnexpectedGatewayConnectionTypeSymbol, arg1.Node.GetLocation()));
        return new OrDiagnostic<GatewayConnTypeInfo>(new GatewayConnTypeInfo(typeSym.MetadataName),
            Array.Empty<Diagnostic>());
    }

    private static bool CheckGatewayConnectionType(SyntaxNode arg1, CancellationToken arg2) =>
        arg1 is TypeDeclarationSyntax { Identifier.Text: "GatewayConnection", };


    private static OrDiagnostic<PayloadTypeInfo> DeducePayloadTypeInfo(GeneratorAttributeSyntaxContext arg1,
        CancellationToken arg2) {
        throw new NotImplementedException();
    }

    private static bool CheckVerifyPayloadType(SyntaxNode arg1, CancellationToken arg2) {
        throw new NotImplementedException();
    }
}