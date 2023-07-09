using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;
using NodaTime;

namespace Eris.Rest.Models;

[JsonConverter(typeof(SnowflakeJsonConverter))]
public readonly struct Snowflake(ulong snowflake)
    : IEquatable<Snowflake>, IComparable<Snowflake>, IComparable
{
    public static readonly Instant DiscordEpoch = Instant.FromUnixTimeMilliseconds(1420070400000L);

    public readonly ulong Value = snowflake;

    public readonly Instant Timestamp = DiscordEpoch + Duration.FromMilliseconds(snowflake >> 22);

    public readonly byte InternalWorkerId = (byte)((snowflake & 0x3E0000) >> 17);

    public readonly byte InternalProcessId = (byte)((snowflake & 0x1F000) >> 12);

    public readonly ushort Increment = (ushort)(snowflake & 0xFFF);

    public bool Equals(Snowflake other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Snowflake other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Snowflake left, Snowflake right) => left.Equals(right);

    public static bool operator !=(Snowflake left, Snowflake right) => !left.Equals(right);

    public int CompareTo(Snowflake other) => Value.CompareTo(other.Value);

    public int CompareTo(object? obj) {
        if (ReferenceEquals(null, obj)) return 1;
        return obj is Snowflake other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Snowflake)}");
    }

    public static bool operator <(Snowflake left, Snowflake right) => left.CompareTo(right) < 0;

    public static bool operator >(Snowflake left, Snowflake right) => left.CompareTo(right) > 0;

    public static bool operator <=(Snowflake left, Snowflake right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Snowflake left, Snowflake right) => left.CompareTo(right) >= 0;
}