namespace Eris.Rest.Models.Channels;

public enum ChannelType
{
    GuildText          = 0,
    Dm                 = 1,
    GuildVoice         = 2,
    GroupDm            = 3,
    GuildCategory      = 4,
    GuildAnnouncement  = 5,
    AnnouncementThread = 10,
    PublicThread       = 11,
    PrivateThread      = 12,
    GuildStageVoice    = 13,
    GuildDirectory     = 14,
    GuildForum         = 15,
}