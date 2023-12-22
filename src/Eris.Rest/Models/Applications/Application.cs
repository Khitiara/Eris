using System.Text.Json.Serialization;
using Eris.Rest.Models.Teams;
using Eris.Rest.Models.Users;

namespace Eris.Rest.Models.Applications;

public sealed class Application
{
    public required Snowflake Id { get; init; }
    public required string Name { get; init; }

    [JsonPropertyName("icon")]
    public string? IconHash { get; init; }

    public required string Description { get; init; }

    [JsonPropertyName("bot_public")]
    public bool IsPublicBot { get; init; }

    public bool BotRequiresCodeGrant { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TermsOfServiceUrl { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PrivacyPolicyUrl { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? RpcOrigins { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public User? Owner { get; init; }

    public Team? Team { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ApplicationFlags Flags { get; init; }
}