using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Channels.Internal;

public class ChannelConverter : JsonConverter<Channel>
{
    public override Channel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        JsonElement obj = JsonElement.ParseValue(ref reader);
        if (obj.ValueKind is JsonValueKind.Null)
            return null;
        if (obj.ValueKind is not JsonValueKind.Object)
            throw new JsonException("Cannot deserialize channel from non-object");
        if (!obj.TryGetProperty("type"u8, out JsonElement typeProp) || !typeProp.TryGetInt32(out int typeInt))
            throw new JsonException("Cannot get type from channel object");
        switch ((ChannelType)typeInt) {
            case ChannelType.GuildVoice:
            case ChannelType.GuildStageVoice:
                return obj.Deserialize<GuildVoiceChannel>(options);
            case ChannelType.Dm:
                return obj.Deserialize<DmChannel>(options);
            case ChannelType.GroupDm:
                return obj.Deserialize<GroupDmChannel>(options);
            case ChannelType.GuildCategory:
                return obj.Deserialize<CategoryChannel>(options);
            case ChannelType.GuildText:
            case ChannelType.GuildAnnouncement:
                return obj.Deserialize<GuildTextChannel>(options);
            case ChannelType.AnnouncementThread:
            case ChannelType.PublicThread:
            case ChannelType.PrivateThread:
                return obj.Deserialize<ThreadChannel>(options);
            case ChannelType.GuildDirectory:
            case ChannelType.GuildForum:
                return obj.Deserialize<GuildChannel>(options);
            default:
                throw new JsonException("Could not deserialize channel",
                    // ReSharper disable once NotResolvedInText
                    new ArgumentOutOfRangeException("type", typeInt, "Invalid channel type"));
        }
    }

    public override void Write(Utf8JsonWriter writer, Channel value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}