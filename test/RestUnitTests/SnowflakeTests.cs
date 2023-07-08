using System.Text.Json;
using Eris.Rest.Models;
using FluentAssertions;

namespace RestUnitTests;

[UsesVerify]
public class SnowflakeTests
{
    [Fact]
    public void TestBasicSnowflake() {
        Snowflake snowflake = new(175928847299117063UL);
        DateTimeOffset timestamp = snowflake.Timestamp;
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
    public Task TestSnowflakeSerialization() =>
        Verify(JsonSerializer.Serialize(new Snowflake(175928847299117063UL),
                new JsonSerializerOptions(JsonSerializerOptions.Default) {
                    WriteIndented = true,
                    TypeInfoResolver = DiscordJsonContext.Default,
                }))
            .UseDirectory(".verification")
            .ToTask();

    [Fact]
    public void TestSnowflakeDeserialization() {
        JsonSerializer.Deserialize("\"175928847299117063\"", DiscordJsonContext.Default.Snowflake)
            .Should().Be(new Snowflake(175928847299117063UL));
    }
}