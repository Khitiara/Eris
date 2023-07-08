using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;

namespace Eris.Rest.RateLimiting;

public class DiscordApiRateLimiter : PartitionedRateLimiter<LimitableDiscordApiRequest>
{
    public readonly record struct BucketKey(string Method, string Route);
    
    public sealed class Bucket
    {
        internal readonly string  GuildId;
        internal readonly string  ChannelId;
        internal readonly string  WebhookId;
        internal volatile string? ErisBucketId;

        public int Remaining => InternalRemaining;

        internal volatile int InternalRemaining;
        public int Maximum { get; internal set; }
        public DateTimeOffset Reset { get; internal set; }
        public TimeSpan? ResetAfter { get; internal set; }
        
        public DateTimeOffset ResetAfterOffset { get; internal set; }

        internal readonly ConcurrentBag<string> KnownHashes;
        private volatile  string                _hash;

        internal Bucket(string initialHash, string guildId, string channelId, string webhookId) {
            GuildId = guildId;
            ChannelId = channelId;
            WebhookId = webhookId;
            Hash = initialHash;
            KnownHashes = new ConcurrentBag<string>();
        }

        internal string Hash {
            get => _hash;
            [MemberNotNull(nameof(_hash))]
            set {
                if (ErisBucketId is not { } s || !s.StartsWith(value)) {
                    string id = GenerateBucketId(value, GuildId, ChannelId, WebhookId);
                    ErisBucketId = id;
                    KnownHashes.Add(id);
                }

                _hash = value;
            }
        }

        /// <summary>
        /// Generates an ID for a request bucket.
        /// </summary>
        /// <param name="hash">Hash for this bucket.</param>
        /// <param name="guildId">Guild Id for this bucket.</param>
        /// <param name="channelId">Channel Id for this bucket.</param>
        /// <param name="webhookId">Webhook Id for this bucket.</param>
        /// <returns>Bucket Id.</returns>
        public static string GenerateBucketId(string hash, string guildId, string channelId, string webhookId)
            => $"{hash}:{guildId}:{channelId}:{webhookId}";
    }

    public override RateLimiterStatistics? GetStatistics(LimitableDiscordApiRequest resource) =>
        throw new NotImplementedException();

    protected override RateLimitLease AttemptAcquireCore(LimitableDiscordApiRequest resource, int permitCount) =>
        throw new NotImplementedException();

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(LimitableDiscordApiRequest resource,
        int permitCount, CancellationToken cancellationToken) => throw new NotImplementedException();

    internal void NotifyGlobalRateLimit(RateLimitingHeaders.RateLimitTriggerInfo info,
        RateLimitingHeaders.RateLimitScope rateLimitScope) {
        throw new NotImplementedException();
    }
}