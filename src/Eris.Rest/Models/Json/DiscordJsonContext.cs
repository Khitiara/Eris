using System.Text.Json.Serialization;
using Eris.Rest.Models.Activity;
using Eris.Rest.RateLimiting;

namespace Eris.Rest.Models.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Snowflake))]
[JsonSerializable(typeof(RateLimitingHeaders.RateLimitResponsePayload))]
[JsonSerializable(typeof(Application.Application))]
[JsonSerializable(typeof(Activity.Activity))]
[JsonSerializable(typeof(BasicActivity))]
public partial class DiscordJsonContext : JsonSerializerContext;