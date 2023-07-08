using System.Text.Json.Serialization;
using Eris.Rest.RateLimiting;

namespace Eris.Rest.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Snowflake))]
[JsonSerializable(typeof(RateLimitingHeaders.RateLimitResponsePayload))]
public partial class DiscordJsonContext : JsonSerializerContext;