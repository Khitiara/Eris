using System.Text.Json.Serialization;

namespace Eris.Rest.Models.Channels;

public interface IVoiceChannel
{
    [JsonRequired]
    public string RtcRegion { get; init; }
}