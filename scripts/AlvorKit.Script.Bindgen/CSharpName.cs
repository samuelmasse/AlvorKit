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

    /// <summary>
    /// Converts RGFW_window_setName to WindowSetName. Digit-leading names get
    /// <paramref name="digitNamePrefix"/> (XXH32 to Xxh32 with prefix Xxh) and digit-digit
    /// segment boundaries keep the underscore (XXH3_64bits to Xxh3_64bits), since merging
    /// the digit runs would garble the name. With <paramref name="dimensionSegments"/>,
    /// digits-then-capital segments stay verbatim (GL_TEXTURE_2D to Texture2D, not Texture2d).
    /// </summary>
    public static string FromNativeIdentifier(string nativeName, string nativePrefix, string digitNamePrefix = "Num", bool dimensionSegments = false)
    {
        nativeName = nativeName.TrimStart('_');
        var unprefixed = nativeName.StartsWith(nativePrefix, StringComparison.OrdinalIgnoreCase)
            ? nativeName[nativePrefix.Length..]
            : nativeName;

        var managedName = new StringBuilder();
        foreach (var segment in unprefixed.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (managedName.Length > 0 && char.IsAsciiDigit(managedName[^1]) && char.IsAsciiDigit(segment[0]))
                managedName.Append('_');
            if (dimensionSegments && segment.Length >= 2
                && char.IsAsciiLetterUpper(segment[^1]) && segment[..^1].All(char.IsAsciiDigit))
            {
                managedName.Append(segment);
                continue;
            }
            var tail = segment.Skip(1).All(c => !char.IsAsciiLetterLower(c))
                ? segment[1..].ToLowerInvariant()
                : segment[1..];
            managedName.Append(char.ToUpperInvariant(segment[0])).Append(tail);
        }

        if (char.IsAsciiDigit(managedName[0]))
            managedName.Insert(0, digitNamePrefix);
        return managedName.ToString();
    }

    /// <summary>Converts RGFW_eventType to RgfwEventType.</summary>
    public static string FromNativeTypeName(string nativeName, string nativePrefix, string managedTypePrefix, string digitNamePrefix = "Num") =>
        managedTypePrefix + FromNativeIdentifier(nativeName, nativePrefix, digitNamePrefix);

    public static string Parameter(string name) => Keywords.Contains(name) ? "@" + name : name;
}
