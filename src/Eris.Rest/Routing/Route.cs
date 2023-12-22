using System.Collections.Frozen;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Std;

namespace Eris.Rest.Routing;

public readonly record struct Route(
    RouteTemplate Template,
    IReadOnlyDictionary<string, object?> Parameters,
    IEnumerable<KeyValuePair<string, StringValues>> QueryParameters)
{
    public HttpMethod Method => Template.Method;

    public Uri ToUri() =>
        new(QueryHelpers.AddQueryString(UriTemplate.Expand(Template.Path, Parameters), QueryParameters),
            UriKind.RelativeOrAbsolute);

    public override string ToString() => Template.ToString();

    public Route(RouteTemplate Template) : this(Template, FrozenDictionary<string, object?>.Empty,
        FrozenDictionary<string, StringValues>.Empty) { }

    public Route(RouteTemplate Template,
        IReadOnlyDictionary<string, object?> Parameters) : this(Template, Parameters,
        FrozenDictionary<string, StringValues>.Empty) { }

    public Route(RouteTemplate Template,
        IEnumerable<KeyValuePair<string, StringValues>> QueryParameters) : this(Template,
        FrozenDictionary<string, object?>.Empty, QueryParameters) { }
}