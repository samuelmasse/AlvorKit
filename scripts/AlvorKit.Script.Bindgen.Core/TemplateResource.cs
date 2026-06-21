namespace AlvorKit.Script.Bindgen;

/// <summary>Loads and renders text templates from the repository root <c>res/</c> directory.</summary>
public static class TemplateResource
{
    /// <summary>Reads a template file using a repository-root <c>res/</c> path.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.cs.tmpl</c>.</param>
    public static string Read(Type anchor, string resourcePath) =>
        RepositoryTemplates.Read(anchor, resourcePath);

    /// <summary>Renders a template by replacing every <c>{{Name}}</c> placeholder.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.cs.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string Render(Type anchor, string resourcePath, params (string Name, string Value)[] values) =>
        RepositoryTemplates.Render(anchor, resourcePath, values);

    /// <summary>Renders a declaration fragment with exactly one trailing blank line.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="resourcePath">The repository-style resource path, such as <c>res/templates/bindgen/file.csfrag.tmpl</c>.</param>
    /// <param name="values">Placeholder names and replacement values.</param>
    public static string RenderFragment(Type anchor, string resourcePath, params (string Name, string Value)[] values) =>
        RepositoryTemplates.RenderFragment(anchor, resourcePath, values);

    /// <summary>Creates a template set rooted at one path under <c>res/templates/bindgen</c>.</summary>
    /// <param name="anchor">A type from the calling assembly, used to help locate the repository root.</param>
    /// <param name="templateArea">Path under <c>res/templates/bindgen</c>, such as <c>c-headers/csharp</c>.</param>
    public static RepositoryTemplateSet ForArea(Type anchor, string templateArea) =>
        RepositoryTemplates.ForArea(anchor, "bindgen/" + templateArea);
}
