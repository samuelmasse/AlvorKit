namespace AlvorKit.Script.Bindgen;

/// <summary>Recovers documentation for macro constants when libclang does not attach it directly.</summary>
internal sealed class CHeaderMacroDocumentation
{
    private readonly Dictionary<string, string[]> sourceLinesByPath = [];

    /// <summary>Returns parsed upstream macro documentation from Clang or an adjacent doc block.</summary>
    public XmlDocComment? Documentation(Cursor cursor) =>
        XmlDocComment.Parse(cursor.Handle.RawCommentText.ToString())
        ?? XmlDocComment.Parse(LeadingDocBlock(cursor));

    /// <summary>Returns the doc block immediately above a macro definition.</summary>
    private string? LeadingDocBlock(Cursor cursor)
    {
        cursor.Handle.Location.GetExpansionLocation(out var file, out var line, out _, out _);
        var path = file.Name.ToString();
        if (path.Length == 0 || !File.Exists(path) || line <= 1)
            return null;

        var lines = Lines(path);
        var index = (int)line - 2;
        if (index < 0 || lines[index].Trim().Length == 0)
            return null;

        if (!lines[index].TrimEnd().EndsWith("*/", StringComparison.Ordinal))
            return null;

        var end = index;
        while (index >= 0)
        {
            var text = lines[index].TrimStart();
            if (text.StartsWith("/*", StringComparison.Ordinal))
                break;
            if (!text.StartsWith('*'))
                return null;
            index--;
        }

        return index >= 0 && IsDocBlockStart(lines[index].TrimStart())
            ? string.Join(Environment.NewLine, lines[index..(end + 1)])
            : null;
    }

    /// <summary>Returns whether a source line begins a Doxygen-style doc block.</summary>
    private static bool IsDocBlockStart(string line) =>
        line.StartsWith("/**", StringComparison.Ordinal) ||
        line.StartsWith("/*!", StringComparison.Ordinal);

    /// <summary>Returns cached source lines for comment fallback lookup.</summary>
    private string[] Lines(string path)
    {
        if (!sourceLinesByPath.TryGetValue(path, out var lines))
        {
            lines = File.ReadAllLines(path);
            sourceLinesByPath[path] = lines;
        }
        return lines;
    }
}
