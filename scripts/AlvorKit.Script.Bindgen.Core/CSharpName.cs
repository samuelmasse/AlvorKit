namespace AlvorKit.Script.Bindgen;

/// <summary>Centralizes the native-to-managed naming rules shared by every generator pipeline.</summary>
public static class CSharpName
{
    /// <summary>C# reserved identifiers that require parameter escaping.</summary>
    private static readonly HashSet<string> Keywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "record", "ref", "return", "sbyte",
        "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
        "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
        "using", "virtual", "void", "volatile", "while",
        "add", "alias", "and", "args", "async", "await", "by", "descending", "dynamic",
        "equals", "file", "from", "get", "global", "group", "init", "into", "join", "let",
        "managed", "nameof", "nint", "not", "notnull", "nuint", "on", "or", "orderby",
        "partial", "remove", "required", "scoped", "select", "set", "unmanaged", "value",
        "var", "when", "where", "with", "yield"
    ];

    /// <summary>
    /// Turns a native identifier into PascalCase after stripping the native prefix. Numeric edge cases
    /// stay readable: digit-leading identifiers receive <paramref name="digitNamePrefix"/>, adjacent
    /// digit runs keep a separator, and OpenGL dimension tokens can preserve forms like 2D.
    /// </summary>
    public static string FromNativeIdentifier(string nativeName, string nativePrefix, string digitNamePrefix = "Num", bool dimensionSegments = false)
    {
        if (string.IsNullOrWhiteSpace(nativeName))
            throw new ArgumentException("Native names must contain at least one identifier character.", nameof(nativeName));

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
                // Khronos dimension suffixes are identifiers in practice: Texture2D reads better
                // than Texture2d and matches the rest of the generated OpenGL API surface.
                managedName.Append(segment);
                continue;
            }
            var tail = segment.Skip(1).All(c => !char.IsAsciiLetterLower(c))
                ? segment[1..].ToLowerInvariant()
                : segment[1..];
            managedName.Append(char.ToUpperInvariant(segment[0])).Append(tail);
        }

        if (managedName.Length == 0)
            throw new ArgumentException($"Native name '{nativeName}' did not produce a managed identifier.", nameof(nativeName));
        if (char.IsAsciiDigit(managedName[0]))
            managedName.Insert(0, digitNamePrefix);
        return managedName.ToString();
    }

    /// <summary>Adds the library type prefix after applying the normal identifier conversion.</summary>
    public static string FromNativeTypeName(string nativeName, string nativePrefix, string managedTypePrefix, string digitNamePrefix = "Num") =>
        managedTypePrefix + FromNativeIdentifier(nativeName, nativePrefix, digitNamePrefix);

    /// <summary>Escapes C# keywords that are valid C parameter names.</summary>
    public static string Parameter(string name) => Keywords.Contains(name) ? "@" + name : name;
}
