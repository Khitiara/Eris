using System.Text.Json.Serialization;
using Eris.Rest.Models.Json;
using NodaTime;

namespace Eris.Rest.Models;

[JsonConverter(typeof(SnowflakeJsonConverter))]
public readonly struct Snowflake(ulong snowflake)
    : IEquatable<Snowflake>, IComparable<Snowflake>, IComparable, ISpanParsable<Snowflake>,
        IUtf8SpanParsable<Snowflake>, ISpanFormattable, IUtf8SpanFormattable
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

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider) => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider) => Value.TryFormat(destination, out charsWritten, format, provider);

    public static bool operator ==(Snowflake left, Snowflake right) => left.Equals(right);

    public static bool operator !=(Snowflake left, Snowflake right) => !left.Equals(right);

    public int CompareTo(Snowflake other) => Value.CompareTo(other.Value);

    public int CompareTo(object? obj) {
        if (ReferenceEquals(null, obj)) return 1;
        return obj is Snowflake other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(Snowflake)}");
    }

    public static bool operator <(Snowflake left, Snowflake right) => left.CompareTo(right) < 0;

    public static bool operator >(Snowflake left, Snowflake right) => left.CompareTo(right) > 0;

    public static bool operator <=(Snowflake left, Snowflake right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Snowflake left, Snowflake right) => left.CompareTo(right) >= 0;
    public static Snowflake Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    public static bool TryParse(string? s, IFormatProvider? provider, out Snowflake result) =>
        TryParse(s.AsSpan(), provider, out result);

    public static Snowflake Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
        if (s.Length is < 15 or >= 21)
            throw new FormatException("The input was in an incorrect format.");
        try {
            return new Snowflake(ulong.Parse(s, provider));
        }
        catch (Exception e) {
            throw new FormatException("The input was in an incorrect format.", e);
        }
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Snowflake result) {
        if (s.Length is >= 15 and < 21 && ulong.TryParse(s, provider, out ulong value)) {
            result = new Snowflake(value);
            return true;
        }

        result = default;
        return false;
    }

    public static Snowflake Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider) {
        try {
            return new Snowflake(ulong.Parse(utf8Text, provider));
        }
        catch (Exception e) {
            throw new FormatException("The input was in an incorrect format.", e);
        }
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out Snowflake result) {
        if (ulong.TryParse(utf8Text, provider, out var value)) {
            result = new Snowflake(value);
            return true;
        }

        result = default;
        return false;
    }
}