using Eris.Rest.Models;

namespace Eris.Rest.RateLimiting;

public readonly record struct BucketId(
    string Hash,
    bool Unlimited,
    Snowflake GuildId,
    Snowflake ChannelId,
    Snowflake WebhookId);