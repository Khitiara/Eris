namespace Eris.Rest.Models.Activities;

public class ActivityEmoji
{
    public required string Name { get; init; }
    public Snowflake Id { get; init; }
    public bool Animated { get; init; }
}