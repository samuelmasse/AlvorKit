namespace AlvorKit.Script.Bindgen;

/// <summary>Finds native C-family symbol names in parsed documentation prose.</summary>
internal static class XmlDocNativeReference
{
    private const string NativeSymbolPattern =
        @"(?:NULL|(?:[A-Z]{2,}[A-Za-z0-9]*_[A-Za-z0-9_]+)|(?:gl|vk|glfw)[A-Z][A-Za-z0-9_]*|GLFW[a-z][A-Za-z0-9_]*|Vk[A-Z][A-Za-z0-9_]*)";

    /// <summary>Formats a Doxygen reference target as native code when it names a C symbol, or readable prose otherwise.</summary>
    internal static string FormatDoxygenReference(string value) =>
        IsNativeReference(value) ? XmlDocCode.Wrap(value) : HumanizeReference(value);

    /// <summary>Wraps bare native symbols in code placeholders without touching symbols already wrapped by placeholders.</summary>
    internal static string CodeBareReferences(string text) =>
        Regex.Replace(
            text,
            $@"(?<!{XmlDocCode.Start})\b(?<symbol>{NativeSymbolPattern})\b(?!{XmlDocCode.End})",
            match => XmlDocCode.Wrap(match.Groups["symbol"].Value));

    /// <summary>Returns whether a token looks like an exact native C symbol rather than ordinary prose.</summary>
    internal static bool IsNativeReference(string value) =>
        Regex.IsMatch(value, $"^{NativeSymbolPattern}$");

    /// <summary>Converts non-symbol Doxygen page anchors into readable prose instead of leaking page IDs.</summary>
    private static string HumanizeReference(string value)
    {
        var words = value.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 1 && words[1].StartsWith(words[0], StringComparison.OrdinalIgnoreCase))
            words = words[1..];
        return string.Join(' ', words);
    }
}
