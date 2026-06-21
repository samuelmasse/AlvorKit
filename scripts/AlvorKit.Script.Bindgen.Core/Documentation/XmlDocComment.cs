namespace AlvorKit.Script.Bindgen;

/// <summary>Native documentation after it has been reduced to XML-doc-friendly prose.</summary>
/// <param name="Summary">Summary prose for the documented member.</param>
/// <param name="Parameters">Parameter prose keyed by native parameter name.</param>
/// <param name="Returns">Return value prose for the documented member.</param>
/// <param name="Remarks">Additional remarks for the documented member.</param>
public sealed record XmlDocComment(
    string? Summary,
    Dictionary<string, string> Parameters,
    string? Returns,
    string? Remarks)
{
    /// <summary>Parses common Doxygen comments plus FreeType's sectioned comment format.</summary>
    public static XmlDocComment? Parse(string? raw) => XmlDocCommentParser.Parse(raw);

    /// <summary>Strips native comment delimiters and leading decoration from one raw line.</summary>
    public static string CleanLine(string line) => XmlDocText.CleanLine(line);

    /// <summary>Escapes text so it can be embedded in XML documentation.</summary>
    public static string Escape(string text) => XmlDocText.Escape(text);

    /// <summary>Parses trailing enum/field comments such as <c>/*!&lt; left mouse button */</c>.</summary>
    public static string? Member(string? raw) => XmlDocCommentParser.Member(raw);

    /// <summary>Returns a documentation sentence anchored to an exact native symbol.</summary>
    public static string NativeSummary(string nativeName, string? original, string fallback) =>
        original is null
            ? fallback
            : NormalizeAlreadyAnchoredSummary(nativeName, original) is { } anchored
                ? SummarySentence(anchored)
                : $"<c>{nativeName}</c> - {SummarySentence(original)}";

    /// <summary>Returns summary prose that already starts with the requested native symbol.</summary>
    private static string? NormalizeAlreadyAnchoredSummary(string nativeName, string original)
    {
        var codePrefix = $"<c>{nativeName}</c>";
        if (original.StartsWith(codePrefix, StringComparison.Ordinal))
        {
            return original;
        }

        return original.StartsWith(nativeName, StringComparison.Ordinal) &&
            (original.Length == nativeName.Length || IsNativeNameBoundary(original[nativeName.Length]))
            ? $"<c>{nativeName}</c>{original[nativeName.Length..]}"
            : null;
    }

    /// <summary>Returns whether a character can follow a complete native symbol in prose.</summary>
    private static bool IsNativeNameBoundary(char value)
    {
        return char.IsWhiteSpace(value) || value is '.' or ':' or '-' or ',' or ';' or ')';
    }

    /// <summary>Capitalizes and terminates a documentation fragment as a sentence.</summary>
    private static string SummarySentence(string text)
    {
        var capitalized = text.Length > 0 && char.IsAsciiLetterLower(text[0])
            ? char.ToUpperInvariant(text[0]) + text[1..]
            : text;
        return capitalized.EndsWith('.') || capitalized.EndsWith('!') || capitalized.EndsWith('?')
            ? capitalized
            : capitalized + ".";
    }
}
