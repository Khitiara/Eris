namespace Eris.Rest.Models.Channels;

public class BaseNormalGuildChannel : GuildChannel
{
    public required int Position { get; init; }
}