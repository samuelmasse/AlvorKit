namespace AlvorKit.Script.Bindgen;

/// <summary>Parses conventional Doxygen comments into XML documentation fields.</summary>
internal static class DoxygenDocParser
{
    /// <summary>Parses a raw Doxygen comment into summary, parameter, return, and remarks text.</summary>
    internal static XmlDocComment Parse(string raw)
    {
        var summary = new StringBuilder();
        var remarks = new StringBuilder();
        var returns = new StringBuilder();
        var discard = new StringBuilder();
        var parameters = new Dictionary<string, string>();
        var currentText = summary;
        string? currentParameter = null;

        foreach (var rawLine in raw.Split('\n'))
        {
            var line = XmlDocText.CleanLine(rawLine);
            if (line.Length == 0)
            {
                if (currentParameter is null && currentText == summary && summary.Length > 0)
                    currentText = remarks;
                continue;
            }
            if (IsGroupMarker(line))
                continue;

            if (line.StartsWith("@brief "))
            {
                currentText = summary;
                currentParameter = null;
                line = line["@brief ".Length..];
            }
            else if (line.StartsWith("@param"))
            {
                currentParameter = TryStartParameter(line, parameters);
                continue;
            }
            else if (line.StartsWith("@return"))
            {
                currentText = returns;
                currentParameter = null;
                line = Regex.Replace(line, @"^@returns?\b", "").TrimStart();
            }
            else if (XmlDocText.IsNoiseTag(line))
            {
                // Metadata-only sections are noisy in IntelliSense, and they must not be appended to
                // the previous return or description section.
                currentText = discard;
                currentParameter = null;
                continue;
            }

            if (currentParameter is not null)
            {
                parameters[currentParameter] = (parameters[currentParameter] + " " + line).Trim();
                continue;
            }

            XmlDocText.AppendSentence(currentText, line);
        }

        return new(
            XmlDocText.CleanText(summary),
            parameters.ToDictionary(pair => pair.Key, pair => XmlDocText.Escape(XmlDocText.StripInlineMarkup(pair.Value))),
            XmlDocText.CleanText(returns),
            XmlDocText.CleanText(remarks));
    }

    /// <summary>Recognizes grouping directives that do not describe the API member itself.</summary>
    private static bool IsGroupMarker(string line) =>
        Regex.IsMatch(line, @"^@(defgroup|addtogroup|ingroup|name)\b") || line is "@{" or "@}";

    /// <summary>Starts collecting text for a Doxygen parameter directive when one is present.</summary>
    private static string? TryStartParameter(string line, Dictionary<string, string> parameters)
    {
        // Doxygen direction markers describe ownership, not the managed parameter name.
        var match = Regex.Match(line, @"^@param(?:\s*\[[^\]]*\])?\s+(\S+)\s*(.*)$");
        if (!match.Success)
            return null;

        var parameter = match.Groups[1].Value;
        parameters[parameter] = match.Groups[2].Value;
        return parameter;
    }
}
