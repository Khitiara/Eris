using System.Text.Json.Serialization;
using Eris.Rest.RateLimiting;

namespace Eris.Rest.Models.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Snowflake))]
[JsonSerializable(typeof(RateLimitingHeaders.RateLimitResponsePayload))]
[JsonSerializable(typeof(Application))]
[JsonSerializable(typeof(Activity))]
[JsonSerializable(typeof(BasicActivity))]
public partial class DiscordJsonContext : JsonSerializerContext;