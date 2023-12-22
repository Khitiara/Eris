using Eris.Rest.Models.Users;

namespace Eris.Rest.Models.Channels;

public class DmChannel : Channel, ITextChannel
{
    public Snowflake? LastMessageId { get; init; }
    public required string RtcRegion { get; init; }
    public required IReadOnlyList<User> Recipients { get; init; }
}