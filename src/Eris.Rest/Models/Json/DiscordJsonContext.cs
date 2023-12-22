using System.Text.Json.Serialization;
using Eris.Rest.Models.Activities;
using Eris.Rest.Models.Applications;
using Eris.Rest.Models.Channels;
using Eris.Rest.Models.Gateway;
using Eris.Rest.Models.Messages;
using Eris.Rest.RateLimiting;
using NodaTime.Serialization.SystemTextJson;

namespace Eris.Rest.Models.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [typeof(InstantUnixMillisecondsConverter), typeof(NodaTimeDefaultJsonConverterFactory),])]
[JsonSerializable(typeof(Snowflake))]
[JsonSerializable(typeof(RateLimitingHeaders.RateLimitResponsePayload))]
[JsonSerializable(typeof(Application))]
[JsonSerializable(typeof(Activity))]
[JsonSerializable(typeof(BasicActivity))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Channel))]
[JsonSerializable(typeof(GuildChannel))]
[JsonSerializable(typeof(ThreadChannel))]
[JsonSerializable(typeof(CategoryChannel))]
[JsonSerializable(typeof(GuildTextChannel))]
[JsonSerializable(typeof(GuildVoiceChannel))]
[JsonSerializable(typeof(DmChannel))]
[JsonSerializable(typeof(GroupDmChannel))]
[JsonSerializable(typeof(GatewayConnectionInfo))]
[JsonSerializable(typeof(BotGatewayConnectionInfo))]
public partial class DiscordJsonContext : JsonSerializerContext;