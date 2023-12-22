using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Users;

public class User
{
    public required Snowflake Id { get; init; }
    public required string Username { get; init; }
    public required string Discriminator { get; init; }
    public required string? GlobalName { get; init; }
    public string? Avatar { get; init; }
    public bool Bot { get; init; }
    public bool System { get; init; }
    public bool MfaEnabled { get; init; }
    public string? Banner { get; init; }
    public int? AccentColor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; init; }

    public bool Verified { get; init; }
    public string? Email { get; init; }
    public UserFlags Flags { get; init; }
    public PremiumType PremiumType { get; init; }
    public UserFlags PublicFlags { get; init; }
}