namespace AlvorKit.Script.Workspace;

/// <summary>Loads related repository templates from one <c>res/templates</c> subdirectory.</summary>
/// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
/// <param name="templateArea">Path under <c>res/templates</c>, such as <c>native-build/windows</c>.</param>
public sealed class RepositoryTemplateSet(Type anchor, string templateArea)
{
    /// <summary>Repository-style path prefix shared by templates in this set.</summary>
    private readonly string resourcePrefix =
        "res/templates/" + NormalizeRelativePath(templateArea, nameof(templateArea), "Template area");

    /// <summary>Reads a template from this set using UTF-8 text and normalized line endings.</summary>
    /// <param name="templateName">Template filename relative to this set.</param>
    public string Read(string templateName) =>
        RepositoryTemplates.Read(anchor, ResourcePath(templateName));

    /// <summary>Renders a full template from this set.</summary>
    /// <param name="templateName">Template filename relative to this set.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public string Render(string templateName, params (string Name, string Value)[] values) =>
        RepositoryTemplates.Render(anchor, ResourcePath(templateName), values);

    /// <summary>Renders a fragment template from this set with exactly one trailing blank line.</summary>
    /// <param name="templateName">Template filename relative to this set.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public string RenderFragment(string templateName, params (string Name, string Value)[] values) =>
        RepositoryTemplates.RenderFragment(anchor, ResourcePath(templateName), values);

    /// <summary>Returns the repository-style resource path for a template in this set.</summary>
    private string ResourcePath(string templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name must not be blank.", nameof(templateName));

        var normalizedName = NormalizeRelativePath(templateName, nameof(templateName), "Template name");
        return resourcePrefix + "/" + normalizedName;
    }

    /// <summary>Normalizes a relative path segment while rejecting escapes outside its template area.</summary>
    private static string NormalizeRelativePath(string path, string parameterName, string label)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"{label} must not be blank.", parameterName);

        if (Path.IsPathRooted(path))
            throw new ArgumentException($"{label} must be relative.", parameterName);

        var normalized = path.Replace('\\', '/').Trim('/');
        var segments = normalized.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".."))
            throw new ArgumentException($"{label} must stay inside its template directory.", parameterName);

        return normalized;
    }
}
