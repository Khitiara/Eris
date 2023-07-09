namespace Eris.Rest.Models;

public class BasicActivity
{
    public required string Name { get; init; }
    public required ActivityType Type { get; init; }
    public Uri? Url { get; init; }
}