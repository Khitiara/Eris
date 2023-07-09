using System.Text.Json.Serialization;

namespace Eris.Rest.Models;

public readonly struct ActivityParty
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? Size { get; init; }
}