namespace AlvorKit.Script.Bindgen;

/// <summary>Loads and renders text templates from the repository root <c>res/</c> directory.</summary>
public static class TemplateResource
{
    /// <summary>Matches simple double-brace placeholders inside templates.</summary>
    private static readonly Regex PlaceholderPattern = new(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

    /// <summary>Reads a template file using a repository-root <c>res/</c> path.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.cs.tmpl</c>.</param>
    public static string Read(Type anchor, string resourcePath) =>
        File.ReadAllText(ResolveResPath(anchor, resourcePath), Encoding.UTF8).ReplaceLineEndings();

    /// <summary>Renders a template by replacing every <c>{{Name}}</c> placeholder.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.cs.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string Render(Type anchor, string resourcePath, params (string Name, string Value)[] values)
    {
        var byName = values.ToDictionary(value => value.Name, value => value.Value, StringComparer.Ordinal);
        return PlaceholderPattern.Replace(
            Read(anchor, resourcePath),
            match => byName.TryGetValue(match.Groups["name"].Value, out var value)
                ? value
                : throw new InvalidOperationException($"Template '{resourcePath}' has no value for placeholder '{match.Value}'."));
    }

    /// <summary>Renders a declaration fragment with exactly one trailing blank line.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.csfrag.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string RenderFragment(Type anchor, string resourcePath, params (string Name, string Value)[] values) =>
        Render(anchor, resourcePath, values).TrimEnd('\r', '\n') + Environment.NewLine + Environment.NewLine;

    /// <summary>Resolves a repository-root <c>res/</c> path to a concrete template file path.</summary>
    private static string ResolveResPath(Type anchor, string resourcePath)
    {
        var root = ProjectRoot.FindFromCurrentProcess(anchor, requireResDirectory: true);
        var normalizedPath = resourcePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var templatePath = Path.GetFullPath(Path.Combine(root, normalizedPath));
        var resRoot = Path.GetFullPath(Path.Combine(root, "res")) + Path.DirectorySeparatorChar;

        if (!templatePath.StartsWith(resRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Template path '{resourcePath}' is outside the repository root res directory.");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template file '{resourcePath}' was not found.", templatePath);

        return templatePath;
    }
}
