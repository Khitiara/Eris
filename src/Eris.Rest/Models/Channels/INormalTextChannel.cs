using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Channels;

public interface INormalTextChannel : ITextChannel
{
    public int RateLimitPerUser { get; init; }

    [JsonRequired]
    public int DefaultAutoArchiveDuration { get; init; }
}