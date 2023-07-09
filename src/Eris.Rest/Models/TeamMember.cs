namespace Eris.Rest.Models;

public class TeamMember
{
    public required Snowflake TeamId { get; init; }
    public required User User { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
    public required TeamMembershipState MembershipState { get; init; }
}