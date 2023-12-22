using System.Text.Json.Serialization;
using Eris.Rest.Models.Applications;
using Eris.Rest.Models.Channels;
using Eris.Rest.Models.Users;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Eris.Rest.Models.Messages;

public class Message
{
    public required Snowflake Id { get; init; }
    public required Snowflake ChannelId { get; init; }
    public required User Author { get; init; }
    public required string Content { get; init; }

    [JsonConverter(typeof(NodaTimeDefaultJsonConverterFactory))]
    public required OffsetDateTime Timestamp { get; init; }

    [JsonConverter(typeof(NodaTimeDefaultJsonConverterFactory))]
    public OffsetDateTime? EditedTimestamp { get; init; }

    public required bool Tts { get; init; }
    public required bool MentionEveryone { get; init; }
    public required IReadOnlyList<User> Mentions { get; init; }
    public required IReadOnlyList<Snowflake> MentionRoles { get; init; }

    public IReadOnlyList<ChannelMention> MentionChannels { get; init; } = Array.Empty<ChannelMention>();

    // public required IReadOnlyList<Attachment> Attachments { get; init; }
    // public required IReadOnlyList<Embed> Embeds { get; init; }
    // public required IReadOnlyList<Reaction> Reactions { get; init; }
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Nonce { get; init; }

    public required bool Pinned { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Snowflake WebhookId { get; init; }

    public required MessageType Type { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessageActivity Activity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Application? Application { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Snowflake ApplicationId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessageReference MessageReference { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessageFlags Flags { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Message? ReferencedMessage { get; init; }

    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // public Interaction? Interaction { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Channel? Thread { get; init; }

    // public IReadOnlyList<MessageComponent> Components { get; init; } = Array.Empty<MessageComponent>();
    // public IReadOnlyList<MessageStickerItem> StickerItems { get; init; } = Array.Empty<MessageStickerItem>();
    // public IReadOnlyList<Sticker> Stickers { get; init; } = Array.Empty<Sticker>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Position { get; init; }

    // public RoleSubscriptionData RoleSubscriptionData { get; init; }
}