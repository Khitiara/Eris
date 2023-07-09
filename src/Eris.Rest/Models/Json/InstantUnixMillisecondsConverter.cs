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