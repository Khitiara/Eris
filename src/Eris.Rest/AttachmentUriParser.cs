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
            // include the second slash so Uri.ToString will do so and thus comply with discord's API docs
            // technically this means we arent compliant with RFC 3986 but thats discord's problem at this point
            return $"attachment://{AttachmentPathRegex().Match(uri.OriginalString).Groups["path"].Value}";
        
        // if keeping the delimiter is kept then append :// to the scheme and/or prepend / to the path which our regex
        // does not include in the capture group
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
        // check for validity against our regex. very simple validation only for now but whatever
        if (!AttachmentPathRegex().IsMatch(uri.OriginalString))
            parsingError = new UriFormatException("Uri does not match discord attachment:// scheme");
    }

    // we permit a single-slash version for compliance with the URI specification as specified in RFC 3986, which is
    // adhered to by UriBuilder but not by Discord for gods know what reasons.
    [GeneratedRegex("attachment://?(?<path>.*)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AttachmentPathRegex();
}