using System.Collections.Frozen;
using Microsoft.Extensions.Primitives;

namespace Eris.Rest.Routing;

public readonly record struct RouteTemplate(HttpMethod Method, string Path)
{
    public override string ToString() => $"{Method}|{Path}";

    public static RouteTemplate Get(string path) => new(HttpMethod.Get, path);
    public static RouteTemplate Post(string path) => new(HttpMethod.Post, path);
    public static RouteTemplate Patch(string path) => new(HttpMethod.Patch, path);
    public static RouteTemplate Delete(string path) => new(HttpMethod.Delete, path);
    public static RouteTemplate Put(string path) => new(HttpMethod.Put, path);

    public Route Expand(IReadOnlyDictionary<string, object?>? parameters = null,
        IEnumerable<KeyValuePair<string, StringValues>>? queryParameters = null) => new(this,
        parameters ?? FrozenDictionary<string, object?>.Empty,
        queryParameters ?? FrozenDictionary<string, StringValues>.Empty);
}