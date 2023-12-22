using Eris.Rest.Models.Channels;

namespace Eris.Rest.Models.Messages;

public class ChannelMention
{
    public required Snowflake Id { get; init; }
    public required Snowflake GuildId { get; init; }
    public required ChannelType Type { get; init; }
    public required string Name { get; init; }
}