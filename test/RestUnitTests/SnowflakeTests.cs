using System.Text.Json;
using Eris.Rest.Models;
using FluentAssertions;
using NodaTime;
using DiscordJsonContext = Eris.Rest.Models.Json.DiscordJsonContext;

namespace RestUnitTests;

public class SnowflakeTests
{
    [Fact]
    public void TestBasicSnowflake() {
        Snowflake snowflake = new(175928847299117063UL);
        ZonedDateTime timestamp = snowflake.Timestamp.InUtc();
        timestamp.Year.Should().Be(2016);
        timestamp.Month.Should().Be(4);
        timestamp.Day.Should().Be(30);
        timestamp.Hour.Should().Be(11);
        timestamp.Minute.Should().Be(18);
        timestamp.Second.Should().Be(25);
        timestamp.Millisecond.Should().Be(796);
        snowflake.InternalWorkerId.Should().Be(1);
        snowflake.InternalProcessId.Should().Be(0);
        snowflake.Increment.Should().Be(7);
    }

    [Fact]
    public void TestSnowflakeSerialization() {
        JsonSerializer.Serialize(new Snowflake(175928847299117063UL),
            new JsonSerializerOptions(JsonSerializerOptions.Default) {
                WriteIndented = true,
                TypeInfoResolver = DiscordJsonContext.Default,
            }).Should().Be("\"175928847299117063\"");
    }

    [Fact]
    public void TestSnowflakeDeserialization() {
        JsonSerializer.Deserialize("\"175928847299117063\"", DiscordJsonContext.Default.Snowflake)
            .Should().Be(new Snowflake(175928847299117063UL));
    }
}