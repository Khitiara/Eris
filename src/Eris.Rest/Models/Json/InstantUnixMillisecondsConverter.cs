using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;

namespace Eris.Rest.Models.Json;

public sealed class InstantUnixMillisecondsConverter : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Instant.FromUnixTimeMilliseconds(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.ToUnixTimeMilliseconds());
    }
}

public sealed class DurationMillisConverter : JsonConverter<Duration>
{
    public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Duration.FromMilliseconds(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options) {
        writer.WriteNumberValue((long)value.TotalMilliseconds);
    }
}