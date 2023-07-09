namespace Eris.Rest.Models;

public class Team
{
    public required Snowflake Id { get; init; }
    public required string Name { get; init; }
    public required Snowflake OwnerUserId { get; init; }
    public string? Icon { get; init; }
    public required IReadOnlyList<TeamMember> Members { get; init; }
}