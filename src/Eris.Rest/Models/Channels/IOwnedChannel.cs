namespace Eris.Rest.Models.Channels;

public interface IOwnedChannel
{
    Snowflake OwnerId { get; init; }
}