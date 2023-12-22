namespace Eris.Rest.Models.Channels;

public interface ITextChannel
{
    public Snowflake? LastMessageId { get; init; }
}