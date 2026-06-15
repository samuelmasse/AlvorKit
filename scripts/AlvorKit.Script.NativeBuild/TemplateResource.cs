using AlvorKit.Script.Workspace;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Loads and renders text templates from the repository root <c>res/</c> directory.</summary>
internal static class TemplateResource
{
    /// <summary>Matches simple double-brace placeholders inside templates.</summary>
    private static readonly Regex PlaceholderPattern = new(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}", RegexOptions.Compiled);

    /// <summary>Reads a template file using a repository-root <c>res/</c> path.</summary>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/native-build/file.ps1.tmpl</c>.</param>
    public static string Read(string resourcePath) =>
        File.ReadAllText(ResolveResPath(resourcePath), Encoding.UTF8).ReplaceLineEndings();

    /// <summary>Renders a template by replacing every <c>{{Name}}</c> placeholder.</summary>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/native-build/file.ps1.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string Render(string resourcePath, params (string Name, string Value)[] values)
    {
        var byName = values.ToDictionary(value => value.Name, value => value.Value, StringComparer.Ordinal);
        return PlaceholderPattern.Replace(
            Read(resourcePath),
            match => byName.TryGetValue(match.Groups["name"].Value, out var value)
                ? value
                : throw new InvalidOperationException($"Template '{resourcePath}' has no value for placeholder '{match.Value}'."));
    }

    /// <summary>Resolves a repository-root <c>res/</c> path to a concrete template file path.</summary>
    private static string ResolveResPath(string resourcePath)
    {
        var root = ProjectRoot.FindFromCurrentProcess(typeof(TemplateResource), requireResDirectory: true);
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
