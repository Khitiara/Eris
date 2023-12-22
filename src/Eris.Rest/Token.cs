using System.Buffers;
using System.Globalization;
using System.Net.Http.Headers;
using Eris.Rest.Models;
using gfoidl.Base64;

namespace Eris.Rest;

public abstract class Token
    : IEquatable<Token>
{
    private readonly string _scheme    = null!;
    private readonly string _parameter = null!;

    protected Token() {

    }

    protected Token(string scheme, string parameter) {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameter);
        _scheme = scheme;
        _parameter = parameter;
        AuthorizationHeader = new AuthenticationHeaderValue(_scheme, _parameter);
    }

    protected internal virtual AuthenticationHeaderValue? AuthorizationHeader { get; }

    public bool Equals(Token? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _scheme == other._scheme && _parameter == other._parameter;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Token other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_scheme, _parameter);

    public static Token Bot(string token) => new BotToken(token);
    public static Token Bearer(string token) => new BearerToken(token);

    public static readonly Token None = new NoToken();

    private sealed class NoToken : Token
    {
        protected internal override AuthenticationHeaderValue? AuthorizationHeader => null;
    }
}

public sealed class BearerToken(string token) : Token("Bearer", token);

public sealed class BotToken : Token
{
    public BotToken(string token) : base("Bot", token) {
        Span<Range> splits = stackalloc Range[4];
        ReadOnlySpan<char> tokenSpan = token.AsSpan();
        if (tokenSpan.Split(splits, '.') != 3)
            throw new FormatException("Invalid bot token");
        int len = Base64.Url.GetMaxDecodedLength(splits[0].GetOffsetAndLength(tokenSpan.Length).Length);
        Span<byte> decodedBuffer = stackalloc byte[len];
        if (Base64.Url.Decode(tokenSpan[splits[0]], decodedBuffer, out _, out int written) !=
            OperationStatus.Done)
            throw new FormatException("Invalid bot token");

        if (!Snowflake.TryParse(decodedBuffer[..written], CultureInfo.InvariantCulture, out Snowflake s)) {
            throw new FormatException("The provided token contains a malformed ID segment.");
        }

        Id = s;
    }

    public Snowflake Id { get; set; }
}