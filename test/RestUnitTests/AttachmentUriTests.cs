using Eris.Rest;
using Eris.Rest.Internal;
using FluentAssertions;
using AttachmentUriParser = Eris.Rest.Internal.AttachmentUriParser;

namespace RestUnitTests;

public class AttachmentUriTests
{
    public AttachmentUriTests() {
        AttachmentUriHandling.Init();
    }
    
    [Fact]
    public void UriParsesWithCorrectComponents() {
        Func<Uri> test = () => new Uri("attachment://filename.txt");
        Uri uri = test.Should().NotThrow().Which;
        uri.Scheme.Should().Be(AttachmentUriParser.AttachmentScheme);
        uri.Authority.Should().BeNullOrEmpty();
        uri.AbsolutePath.Should().Be("/filename.txt");
    }

    [Fact]
    public void UriBuilderAttachmentSchemeWorks() {
        UriBuilder builder = new() {
            Scheme = "attachment",
            Host = null,
            Path = "/filename.txt",
        };
        builder.Uri.ToString().Should().Be("attachment://filename.txt");
    }
}