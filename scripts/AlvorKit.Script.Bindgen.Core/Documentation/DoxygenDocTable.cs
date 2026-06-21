namespace AlvorKit.Script.Bindgen;

/// <summary>Filters markdown tables from native Doxygen prose before XML docs are flattened.</summary>
internal static class DoxygenDocTable
{
    /// <summary>Returns whether a cleaned Doxygen line is part of a markdown table.</summary>
    internal static bool IsTableLine(string line) =>
        Regex.IsMatch(line, @"\S+\s+\|\s+\S+")
        || Regex.IsMatch(line, @"^\s*-{3,}\s*\|\s*-{3,}")
        || Regex.IsMatch(line, @"^\s*\|\s*-{3,}");

    /// <summary>Returns whether a line is a footnote immediately following a skipped table.</summary>
    internal static bool IsTableFootnote(string line) => Regex.IsMatch(line, @"^\d+\)\s+");

    /// <summary>Removes dangling prose that only introduced a table that will not appear in XML docs.</summary>
    internal static void DropLeadIn(
        StringBuilder currentText,
        Dictionary<string, string> parameters,
        string? currentParameter)
    {
        if (currentParameter is not null)
        {
            parameters[currentParameter] = DropLeadIn(parameters[currentParameter]);
            return;
        }

        var trimmed = DropLeadIn(currentText.ToString());
        currentText.Clear();
        currentText.Append(trimmed);
    }

    /// <summary>Drops table lead-in phrases that would otherwise dangle after markdown rows are skipped.</summary>
    private static string DropLeadIn(string text)
    {
        text = Regex.Replace(text, @"\s*See the table below for details\.\s*$", "", RegexOptions.CultureInvariant);
        text = Regex.Replace(text, @"\s*Each [^.?!]* following values:\s*$", "", RegexOptions.CultureInvariant);
        return text.TrimEnd();
    }
}
