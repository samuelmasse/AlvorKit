namespace AlvorKit.Script.Bindgen;

/// <summary>Converts native C identifiers into public C# identifiers.</summary>
public static class CSharpName
{
    private static readonly HashSet<string> Keywords =
    [
        "base", "bool", "byte", "char", "class", "decimal", "double", "event", "fixed", "float",
        "in", "int", "lock", "long", "object", "out", "params", "ref", "sbyte", "short",
        "string", "this", "uint", "ulong", "ushort"
    ];

    /// <summary>Converts RGFW_window_setName to WindowSetName.</summary>
    public static string FromNativeIdentifier(string nativeName, string nativePrefix)
    {
        nativeName = nativeName.TrimStart('_');
        var unprefixed = nativeName.StartsWith(nativePrefix, StringComparison.OrdinalIgnoreCase)
            ? nativeName[nativePrefix.Length..]
            : nativeName;

        var managedName = new StringBuilder();
        foreach (var segment in unprefixed.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            var tail = segment.Skip(1).All(c => !char.IsAsciiLetterLower(c))
                ? segment[1..].ToLowerInvariant()
                : segment[1..];
            managedName.Append(char.ToUpperInvariant(segment[0])).Append(tail);
        }

        if (char.IsAsciiDigit(managedName[0]))
            managedName.Insert(0, "Num");
        return managedName.ToString();
    }

    /// <summary>Converts RGFW_eventType to RgfwEventType.</summary>
    public static string FromNativeTypeName(string nativeName, string nativePrefix, string managedTypePrefix) =>
        managedTypePrefix + FromNativeIdentifier(nativeName, nativePrefix);

    public static string Parameter(string name) => Keywords.Contains(name) ? "@" + name : name;
}
