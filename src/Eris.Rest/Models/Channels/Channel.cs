using System.Text.Json.Serialization;
using Eris.Rest.Models.Channels.Internal;

namespace Eris.Rest.Models.Channels;

[JsonConverter(typeof(ChannelConverter))]
public abstract class Channel
{
    public required Snowflake Id { get; init; }
    public required ChannelType Type { get; init; }
}