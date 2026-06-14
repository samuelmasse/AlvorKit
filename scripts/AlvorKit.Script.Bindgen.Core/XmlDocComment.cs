namespace AlvorKit.Script.Bindgen;

/// <summary>Native documentation after it has been reduced to XML-doc-friendly prose.</summary>
public record XmlDocComment(
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
}
