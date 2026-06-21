namespace AlvorKit.ECS.Generator;

/// <summary>Renders embedded source-generator templates.</summary>
internal static class ComponentTemplate
{
    /// <summary>Matches simple double-brace placeholders inside generator templates.</summary>
    private static readonly Regex PlaceholderPattern = new(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

    /// <summary>The manifest resource prefix used for embedded source-generator templates.</summary>
    private const string ResourcePrefix = "AlvorKit.ECS.Generator.Templates.";

    /// <summary>Renders an embedded template by replacing every placeholder with a supplied value.</summary>
    internal static string Render(string templateName, params (string Name, string Value)[] values)
    {
        var byName = values.ToDictionary(value => value.Name, value => value.Value, StringComparer.Ordinal);
        return PlaceholderPattern.Replace(
            Read(templateName),
            match => byName.TryGetValue(match.Groups["name"].Value, out var value)
                ? value
                : throw new InvalidOperationException($"Template '{templateName}' has no value for placeholder '{match.Value}'."));
    }

    /// <summary>Renders a fragment template with exactly one trailing blank line.</summary>
    internal static string RenderFragment(string templateName, params (string Name, string Value)[] values) =>
        Render(templateName, values).TrimEnd('\r', '\n') + "\n\n";

    /// <summary>Reads an embedded template as UTF-8 text.</summary>
    private static string Read(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = ResourcePrefix + templateName;
        using var stream = assembly.GetManifestResourceStream(resourceName) ??
            throw new FileNotFoundException($"Embedded template '{templateName}' was not found.", resourceName);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
