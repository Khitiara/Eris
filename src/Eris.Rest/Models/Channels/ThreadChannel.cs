using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Channels;

public class ThreadChannel : GuildChannel, INormalTextChannel, IOwnedChannel
{
    public Snowflake? LastMessageId { get; init; }
    public int RateLimitPerUser { get; init; }
    public required int DefaultAutoArchiveDuration { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required Snowflake OwnerId { get; init; }
}