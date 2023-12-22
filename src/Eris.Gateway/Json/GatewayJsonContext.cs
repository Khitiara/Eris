using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Eris.Rest.Models.Json;

namespace Eris.Gateway.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [typeof(DiscordJsonContext)])]
[JsonSerializable(typeof(GatewayPayload))]
public partial class GatewayJsonContext : JsonSerializerContext
{
    // private static IJsonTypeInfoResolver? _combinedResolver;
    //
    // public static IJsonTypeInfoResolver CombinedResolver => LazyInitializer.EnsureInitialized(ref _combinedResolver,
    //     () => JsonTypeInfoResolver.Combine(DiscordJsonContext.Default, Default));
}