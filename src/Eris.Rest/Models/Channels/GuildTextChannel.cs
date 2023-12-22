namespace Eris.Rest.Models.Channels;

public class GuildTextChannel : BaseGuildTextChannel, INormalTextChannel
{
    public required string? Topic { get; init; }
    public int RateLimitPerUser { get; init; }
    public required int DefaultAutoArchiveDuration { get; init; }
}