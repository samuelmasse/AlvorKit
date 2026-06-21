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
        var skippingTable = false;

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
            if (DoxygenDocTable.IsTableLine(line))
            {
                if (!skippingTable)
                    DoxygenDocTable.DropLeadIn(currentText, parameters, currentParameter);
                skippingTable = true;
                continue;
            }
            if (skippingTable && DoxygenDocTable.IsTableFootnote(line))
                continue;
            skippingTable = false;

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
                if (IsCodeBlockStart(line))
                    DropTrailingCodeExampleLeadIn(currentText, parameters, currentParameter);
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

    /// <summary>Returns whether a Doxygen noise tag begins a block whose prose lead-in should be discarded.</summary>
    private static bool IsCodeBlockStart(string line) => Regex.IsMatch(line, @"^@(code|verbatim)\b");

    /// <summary>Drops a trailing example lead-in that only introduced a skipped Doxygen code block.</summary>
    private static void DropTrailingCodeExampleLeadIn(
        StringBuilder currentText,
        Dictionary<string, string> parameters,
        string? currentParameter)
    {
        if (currentParameter is not null)
        {
            parameters[currentParameter] = DropTrailingCodeExampleLeadIn(parameters[currentParameter]);
            return;
        }

        var trimmed = DropTrailingCodeExampleLeadIn(currentText.ToString());
        currentText.Clear();
        currentText.Append(trimmed);
    }

    /// <summary>Removes a final example heading that would otherwise dangle after its code block is discarded.</summary>
    private static string DropTrailingCodeExampleLeadIn(string text) =>
        Regex.Replace(text, @"\s*Example\b[^.?!]*:\s*$", "", RegexOptions.CultureInvariant);
}
