using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;
using NodaTime;

namespace Eris.Rest.Models;

public class Activity : BasicActivity
{
    [JsonConverter(typeof(InstantUnixMillisecondsConverter))]
    public required Instant CreatedAt { get; init; }

    public ActivityTimestamps Timestamps { get; init; }

    public Snowflake ApplicationId { get; init; }

    public string? Details { get; init; }

    public string? State { get; init; }

    public ActivityEmoji? Emoji { get; init; }

    public ActivityParty Party { get; init; }
}