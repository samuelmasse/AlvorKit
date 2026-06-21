namespace AlvorKit.Script.Workspace;

/// <summary>Loads and renders text templates from the repository root <c>res/templates</c> directory.</summary>
public static class RepositoryTemplates
{
    /// <summary>Creates a template set rooted at one path under <c>res/templates</c>.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="templateArea">Path under <c>res/templates</c>, such as <c>maths</c>.</param>
    public static RepositoryTemplateSet ForArea(Type anchor, string templateArea) =>
        new(anchor, templateArea);

    /// <summary>Reads a template file using a repository-root <c>res/</c> path.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/maths/file.cs.tmpl</c>.</param>
    public static string Read(Type anchor, string resourcePath)
    {
        var text = File.ReadAllText(ResolveResPath(anchor, resourcePath), Encoding.UTF8);
        return TemplateRenderer.NormalizeLineEndings(text, Environment.NewLine);
    }

    /// <summary>Renders a template by replacing every <c>{{Name}}</c> placeholder.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/maths/file.cs.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string Render(Type anchor, string resourcePath, params (string Name, string Value)[] values) =>
        TemplateRenderer.Render(Read(anchor, resourcePath), resourcePath, Environment.NewLine, values);

    /// <summary>Renders a declaration fragment with exactly one trailing blank line.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/maths/file.csfrag.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string RenderFragment(Type anchor, string resourcePath, params (string Name, string Value)[] values) =>
        TemplateRenderer.RenderFragment(Read(anchor, resourcePath), resourcePath, Environment.NewLine, values);

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
