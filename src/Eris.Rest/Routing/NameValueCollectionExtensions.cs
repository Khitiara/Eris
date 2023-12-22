using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Eris.Rest.Routing;

public static class NameValueCollectionExtensions
{
    public static T? GetValueOrDefault<T>(this IReadOnlyDictionary<string, object?> collection, string name) =>
        TryGetValue(collection, name, out T? v) ? v : default;

    public static bool TryGetValue<T>(this IReadOnlyDictionary<string, object?> collection, string name,
        [NotNullWhen(true)] out T? value) {
        if (!collection.TryGetValue(name, out object? v)) {
            value = default;
            return false;
        }

        if (v is T t) {
            value = t;
            return true;
        }

        value = (T?)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
        return value is not null;
    }
}