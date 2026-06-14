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
        var discard = new StringBuilder();
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
            else if (line.StartsWith("@param"))
            {
                // Doxygen @param, optionally carrying a direction such as @param[in] or @param[out].
                var match = Regex.Match(line, @"^@param(?:\s*\[[^\]]*\])?\s+(\S+)\s*(.*)$");
                if (match.Success)
                {
                    currentParameter = match.Groups[1].Value;
                    parameters[currentParameter] = match.Groups[2].Value;
                }
                continue;
            }
            else if (line.StartsWith("@return"))
            {
                currentText = returns;
                currentParameter = null;
                line = Regex.Replace(line, @"^@returns?\b", "").TrimStart();
            }
            else if (IsNoiseTag(line))
            {
                // Trailing metadata (@thread_safety, @errors, @sa, @since, platform @remark ...):
                // drop it, and stop the open @return or description from swallowing what follows.
                currentText = discard;
                currentParameter = null;
                continue;
            }

            if (currentParameter is not null)
            {
                parameters[currentParameter] = (parameters[currentParameter] + " " + line).Trim();
                continue;
            }

            AppendSentence(currentText, line);
        }

        return new(
            CleanText(summary),
            parameters.ToDictionary(p => p.Key, p => Escape(StripInlineMarkup(p.Value))),
            CleanText(returns),
            CleanText(remarks));
    }

    /// <summary>
    /// Trailing Doxygen metadata that reads as noise in a C# tooltip. Such a tag is dropped, and it
    /// also ends the current section so an open &lt;returns&gt; or description stops capturing.
    /// </summary>
    private static bool IsNoiseTag(string line) => Regex.IsMatch(line,
        @"^@(sa|see|since|thread_safety|errors?|remarks?|note|warning|attention|deprecated|pointer_lifetime|reentrancy|analysis|par|glfw3|code|endcode|verbatim|endverbatim|internal|pre|post|invariant|win32|macos|linux|x11|wayland|egl|wgl|glx|osmesa|nsgl|posix|unix)\b");

    /// <summary>
    /// Strips inline Doxygen and Markdown residue — cross-references, links, platform markers and
    /// emphasis — so the surviving prose reads as plain text.
    /// </summary>
    private static string StripInlineMarkup(string text)
    {
        text = Regex.Replace(text, @"\[([^\]]+)\]\([^)]*\)", "$1");   // [label](@ref x) or [label](url) -> label
        text = Regex.Replace(text, @"\[([^\]]+)\]\[[^\]]*\]", "$1");  // [label][anchor] -> label
        text = Regex.Replace(text, @"@(?:ref|p|c|a)\s+(\S+)", "$1");  // @ref Name / @p Name -> Name
        text = Regex.Replace(text,                                    // stray inline tags -> removed
            @"@(?:win32|macos|linux|x11|wayland|egl|wgl|glx|osmesa|nsgl|posix|unix|note|remarks?|warning|attention|thread_safety|errors?|sa|see|since|pointer_lifetime|reentrancy|analysis|par|glfw3|link|endlink)\b\s*", "");
        text = text.Replace("__", "").Replace("`", "");              // markdown emphasis and code spans
        return Regex.Replace(text, @"\s{2,}", " ").Trim();           // collapse whitespace left behind
    }

    private static string? CleanText(StringBuilder value)
    {
        if (value.Length == 0)
            return null;
        var cleaned = StripInlineMarkup(value.ToString());
        return cleaned.Length == 0 ? null : Escape(cleaned);
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
        text = StripInlineMarkup(text);   // strip Doxygen/Markdown residue, as the section parser does for functions
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
