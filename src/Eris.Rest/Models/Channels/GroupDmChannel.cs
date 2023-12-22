using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Channels;

public class GroupDmChannel : DmChannel, IOwnedChannel
{
    public required string? Name { get; init; }
    public required string? Icon { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required Snowflake OwnerId { get; init; }
}