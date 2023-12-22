namespace Eris.Rest.Models.Channels;

public class GuildVoiceChannel : BaseGuildTextChannel, IVoiceChannel
{
    public required string RtcRegion { get; init; }
    public required int Bitrate { get; init; }
    public required int UserLimit { get; init; }
}