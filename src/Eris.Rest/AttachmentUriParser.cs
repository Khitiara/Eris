using System.Text.RegularExpressions;

namespace Eris.Rest;

internal sealed partial class AttachmentUriParser : UriParser
{
    public const string AttachmentScheme = "attachment";

    internal static void Register() {
        UriParser.Register(new AttachmentUriParser(), AttachmentScheme, -1);
    }

    protected override string GetComponents(Uri uri, UriComponents uriComponents, UriFormat format) {
        if (!uri.IsAbsoluteUri)
            throw new InvalidOperationException();

        if ((uriComponents & UriComponents.SerializationInfoString) != 0)
            uriComponents |= UriComponents.AbsoluteUri;

        if ((uriComponents & (UriComponents.Scheme | UriComponents.Path)) ==
            (UriComponents.Scheme | UriComponents.Path))
            return $"attachment://{AttachmentPathRegex().Match(uri.OriginalString).Groups["path"].Value}";
        bool keepDelim = (uriComponents & UriComponents.KeepDelimiter) != 0;
        if ((uriComponents & UriComponents.Scheme) != 0)
            return keepDelim ? AttachmentScheme + "://" : AttachmentScheme;
        if ((uriComponents & UriComponents.Path) != 0) {
            string path = AttachmentPathRegex().Match(uri.OriginalString).Groups["path"].Value;
            return keepDelim ? '/' + path : path;
        }

        return string.Empty;
    }

    protected override void InitializeAndValidate(Uri uri, out UriFormatException? parsingError) {
        base.InitializeAndValidate(uri, out parsingError);
        if (!AttachmentPathRegex().IsMatch(uri.OriginalString))
            parsingError = new UriFormatException("Uri does not match discord attachment:// scheme");
    }

    [GeneratedRegex("attachment://?(?<path>.*)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AttachmentPathRegex();
}