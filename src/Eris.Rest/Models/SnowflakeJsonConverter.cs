using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eris.Rest.Models;

internal sealed class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidOperationException();
        ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        if (!Utf8Parser.TryParse(span, out ulong snowflake, out _))
            throw new InvalidOperationException();
        return new Snowflake(snowflake);
    }

    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options) {
        Span<byte> utf8 = stackalloc byte[21]; // the biggest ulong fits into 20 characters plus one more for safety
        if (!Utf8Formatter.TryFormat(value.Value, utf8, out int bytesWritten))
            throw new InvalidOperationException();
        writer.WriteStringValue(utf8[..bytesWritten]);
    }
}