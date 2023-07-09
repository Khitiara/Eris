using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;
using NodaTime;

namespace Eris.Rest.Models;

public readonly struct ActivityTimestamps
{
    [JsonConverter(typeof(InstantUnixMillisecondsConverter))]
    public Instant Start { get; init; }

    [JsonConverter(typeof(InstantUnixMillisecondsConverter))]
    public Instant End { get; init; }
}