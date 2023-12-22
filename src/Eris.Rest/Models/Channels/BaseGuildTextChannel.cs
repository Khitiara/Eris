namespace Eris.Rest.Models.Channels;

public class BaseGuildTextChannel : BaseNormalGuildChannel, ITextChannel
{
    public Snowflake? LastMessageId { get; init; }
}