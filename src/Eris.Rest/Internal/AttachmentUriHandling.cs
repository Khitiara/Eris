using System.Runtime.CompilerServices;

namespace Eris.Rest.Internal;

/// <summary>
/// Special handling for discord's <value>attachment://</value> URI scheme within <see cref="Uri"/>.
/// Note that <see cref="UriBuilder"/> will by default produce formatting not compatible with the discord API,
/// The parser this type registers will accept such an invalid attachment URI and the resulting URI will produce
/// correct output on a call to <see cref="Uri.ToString"/>
/// </summary>
public static class AttachmentUriHandling
{
    static AttachmentUriHandling() {
        AttachmentUriParser.Register();
    }

    /// <summary>
    /// Registers the attachment uri parser if it is not already; actual registration is done by this type's
    /// static constructor which can only run at most once and calling this method forces that constructor to run.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Init() {}
}