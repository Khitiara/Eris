namespace Eris.Rest.Models.Channels;

public abstract class GuildChannel : Channel
{
    public required string Name { get; init; }
    public Snowflake GuildId { get; init; }
    public required Snowflake? ParentId { get; init; }
    public required bool Nsfw { get; init; }
    // public IReadOnlyList<PermissionOverwrite> PermissionOverwrites { get; init; } = Array.Empty<PermissionOverwrite>();
}