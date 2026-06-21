namespace AlvorKit.Script.Workspace;

/// <summary>Renders simple double-brace text templates.</summary>
internal static class TemplateRenderer
{
    /// <summary>Matches valid placeholder tokens inside templates.</summary>
    private static readonly Regex PlaceholderPattern = new(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

    /// <summary>Renders a template by replacing every placeholder with a supplied value.</summary>
    public static string Render(string templateText, string templateId, string lineEnding, params (string Name, string Value)[] values)
    {
        var normalized = NormalizeLineEndings(templateText, lineEnding);
        ValidatePlaceholders(normalized, templateId);
        var byName = ValuesByName(templateId, values);
        return PlaceholderPattern.Replace(
            normalized,
            match => byName.TryGetValue(match.Groups["name"].Value, out var value)
                ? value
                : throw new InvalidOperationException($"Template '{templateId}' has no value for placeholder '{match.Value}'."));
    }

    /// <summary>Renders a fragment with exactly one trailing blank line.</summary>
    public static string RenderFragment(string templateText, string templateId, string lineEnding, params (string Name, string Value)[] values) =>
        Render(templateText, templateId, lineEnding, values).TrimEnd('\r', '\n') + lineEnding + lineEnding;

    /// <summary>Normalizes CRLF and CR-only line endings to the requested line ending.</summary>
    public static string NormalizeLineEndings(string text, string lineEnding) =>
        text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);

    /// <summary>Returns replacement values by placeholder name, rejecting duplicates that make rendering ambiguous.</summary>
    private static Dictionary<string, string> ValuesByName(string templateId, IEnumerable<(string Name, string Value)> values)
    {
        var byName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, value) in values)
        {
            if (!byName.TryAdd(name, value))
                throw new InvalidOperationException($"Template '{templateId}' received duplicate value for placeholder '{{{{{name}}}}}'.");
        }

        return byName;
    }

    /// <summary>Rejects malformed placeholder-like text before rendering.</summary>
    private static void ValidatePlaceholders(string text, string templateId)
    {
        var start = 0;
        while ((start = text.IndexOf("{{", start, StringComparison.Ordinal)) >= 0)
        {
            var end = text.IndexOf("}}", start + 2, StringComparison.Ordinal);
            if (end < 0)
                throw new InvalidOperationException($"Template '{templateId}' has an unterminated placeholder.");

            var token = text[start..(end + 2)];
            if (!PlaceholderPattern.IsMatch(token))
                throw new InvalidOperationException($"Template '{templateId}' has malformed placeholder '{token}'.");

            start = end + 2;
        }
    }
}
