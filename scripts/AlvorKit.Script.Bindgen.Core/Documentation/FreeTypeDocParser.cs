namespace AlvorKit.Script.Bindgen;

/// <summary>Parses FreeType's sectioned comment format into XML documentation fields.</summary>
internal static class FreeTypeDocParser
{
    /// <summary>Parses cleaned FreeType comment lines into summary, parameter, return, and remarks text.</summary>
    internal static XmlDocComment Parse(List<string> lines)
    {
        var summary = new StringBuilder();
        var returns = new StringBuilder();
        var remarks = new StringBuilder();
        var parameters = new Dictionary<string, string>();
        var section = "";
        string? currentParameter = null;

        foreach (var line in lines)
        {
            var tag = Regex.Match(line, @"^@(\w+):\s*(.*)$");
            if (tag.Success)
            {
                section = tag.Groups[1].Value.ToLowerInvariant();
                currentParameter = null;
                if (tag.Groups[2].Value.Length > 0)
                    AppendToSection(section, tag.Groups[2].Value, summary, returns, remarks);
                continue;
            }

            if (IsParameterSection(section) && TryAppendParameter(line, parameters, ref currentParameter))
                continue;

            AppendToSection(section, line, summary, returns, remarks);
        }

        return new(
            Text(summary),
            parameters.ToDictionary(pair => pair.Key, pair => XmlDocText.Escape(pair.Value)),
            Text(returns),
            Text(remarks));
    }

    /// <summary>Detects FreeType parameter sections.</summary>
    private static bool IsParameterSection(string section) => section is "input" or "output" or "inout";

    /// <summary>Reads a FreeType parameter entry or continuation line.</summary>
    private static bool TryAppendParameter(
        string line,
        Dictionary<string, string> parameters,
        ref string? currentParameter)
    {
        var entry = Regex.Match(line, @"^(\w+)\s+::\s*(.*)$");
        if (entry.Success)
        {
            currentParameter = entry.Groups[1].Value;
            parameters[currentParameter] = XmlDocText.ConvertFreeTypeMarkup(entry.Groups[2].Value);
            return true;
        }

        if (currentParameter is null)
            return false;

        parameters[currentParameter] = (parameters[currentParameter] + " " + XmlDocText.ConvertFreeTypeMarkup(line)).Trim();
        return true;
    }

    /// <summary>Appends prose to the current recognized FreeType section.</summary>
    private static void AppendToSection(
        string section,
        string text,
        StringBuilder summary,
        StringBuilder returns,
        StringBuilder remarks)
    {
        var target = section switch
        {
            "description" => summary,
            "return" => returns,
            "note" => remarks,
            _ => null
        };

        if (target is not null)
            XmlDocText.AppendSentence(target, XmlDocText.ConvertFreeTypeMarkup(text));
    }

    /// <summary>Escapes collected FreeType prose, preserving null for empty sections.</summary>
    private static string? Text(StringBuilder value) =>
        value.Length == 0 ? null : XmlDocText.Escape(value.ToString());
}
