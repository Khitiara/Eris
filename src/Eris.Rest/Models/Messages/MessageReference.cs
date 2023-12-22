using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Messages;

public readonly struct MessageReference
{
    public Snowflake MessageId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Snowflake ChannelId { get; init; }

    public Snowflake GuildId { get; init; }
    public bool FailIfNotExists { get; init; }
}