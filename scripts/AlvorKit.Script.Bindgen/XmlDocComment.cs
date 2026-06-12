namespace AlvorKit.Script.Bindgen;

/// <summary>A Doxygen-style native comment normalized for XML documentation.</summary>
public record XmlDocComment(
    string? Summary,
    Dictionary<string, string> Parameters,
    string? Returns,
    string? Remarks)
{
    /// <summary>
    /// Parses inline Doxygen tags and FreeType-style sectioned tags such as
    /// @description:, @input:, @output:, and @return:.
    /// </summary>
    public static XmlDocComment? Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var nonEmptyLines = raw.Split('\n').Select(CleanLine).Where(l => l.Length > 0).ToList();
        if (nonEmptyLines.Any(l => Regex.IsMatch(l, @"^@\w+:")))
            return ParseSectioned(nonEmptyLines);

        var summary = new StringBuilder();
        var remarks = new StringBuilder();
        var returns = new StringBuilder();
        var parameters = new Dictionary<string, string>();
        var currentText = summary;
        string? currentParameter = null;

        foreach (var rawLine in raw.Split('\n'))
        {
            var line = CleanLine(rawLine);
            if (line.Length == 0)
            {
                if (currentParameter is null && currentText == summary && summary.Length > 0)
                    currentText = remarks;
                continue;
            }
            if (Regex.IsMatch(line, @"^@(defgroup|addtogroup|ingroup)\b") || line is "@{" or "@}")
                continue;

            if (line.StartsWith("@brief "))
            {
                currentText = summary;
                currentParameter = null;
                line = line["@brief ".Length..];
            }
            else if (line.StartsWith("@param "))
            {
                var rest = line["@param ".Length..];
                var space = rest.IndexOf(' ');
                currentParameter = space < 0 ? rest : rest[..space];
                parameters[currentParameter] = space < 0 ? "" : rest[(space + 1)..];
                continue;
            }
            else if (line.StartsWith("@return"))
            {
                currentText = returns;
                currentParameter = null;
                line = line.TrimStart("@returns".ToCharArray()).TrimStart();
            }

            if (currentParameter is not null)
            {
                parameters[currentParameter] = (parameters[currentParameter] + " " + line).Trim();
                continue;
            }

            AppendSentence(currentText, line);
        }

        return new(
            Text(summary),
            parameters.ToDictionary(p => p.Key, p => Escape(p.Value)),
            Text(returns),
            Text(remarks));
    }

    /// <summary>Cleans one comment line: strips comment markers and Doxygen noise.</summary>
    public static string CleanLine(string line)
    {
        line = line.Trim();
        foreach (var marker in new[] { "/**!", "/*!<", "/*!", "/**", "/*" })
            if (line.StartsWith(marker))
                line = line[marker.Length..];
        if (line.EndsWith("*/"))
            line = line[..^2];
        line = Regex.Replace(line, @"^[\s*!]+", "");
        if (line.StartsWith('<'))
            line = line[1..];
        line = Regex.Replace(line, @"^--+\s*", "");
        return line.Trim();
    }

    public static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    /// <summary>Parses a trailing member comment like "/*!&lt; left mouse button */".</summary>
    public static string? Member(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var text = string.Join(' ', raw.Split('\n').Select(CleanLine).Where(l => l.Length > 0)).Trim();
        return text.Length == 0 ? null : Escape(text);
    }

    private static XmlDocComment ParseSectioned(List<string> lines)
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
                    AppendToCurrentSection(tag.Groups[2].Value);
                continue;
            }

            if (section is "input" or "output" or "inout")
            {
                var entry = Regex.Match(line, @"^(\w+) ::\s*(.*)$");
                if (entry.Success)
                {
                    currentParameter = entry.Groups[1].Value;
                    parameters[currentParameter] = ConvertFreeTypeMarkup(entry.Groups[2].Value);
                    continue;
                }
                if (currentParameter is not null)
                {
                    parameters[currentParameter] = (parameters[currentParameter] + " " + ConvertFreeTypeMarkup(line)).Trim();
                    continue;
                }
            }
            AppendToCurrentSection(line);
        }

        return new(
            Text(summary),
            parameters.ToDictionary(p => p.Key, p => Escape(p.Value)),
            Text(returns),
            Text(remarks));

        void AppendToCurrentSection(string text)
        {
            var target = section switch
            {
                "description" => summary,
                "return" => returns,
                "note" => remarks,
                _ => null
            };
            if (target is not null)
                AppendSentence(target, ConvertFreeTypeMarkup(text));
        }
    }

    private static string ConvertFreeTypeMarkup(string text) =>
        Regex.Replace(text.Replace('~', ' '), @"@(?=[A-Za-z_])", "");

    private static void AppendSentence(StringBuilder builder, string text)
    {
        if (builder.Length > 0)
            builder.Append(' ');
        builder.Append(text);
    }

    private static string? Text(StringBuilder value) =>
        value.Length == 0 ? null : Escape(value.ToString());
}
