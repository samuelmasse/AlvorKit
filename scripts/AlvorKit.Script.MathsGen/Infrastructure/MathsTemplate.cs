namespace AlvorKit.Script.MathsGen;

/// <summary>Renders Maths generator templates from the repository <c>res/templates/maths</c> directory.</summary>
internal static class MathsTemplate
{
    /// <summary>Template files under <c>res/templates/maths</c>.</summary>
    private static readonly RepositoryTemplateSet Templates = RepositoryTemplates.ForArea(typeof(MathsTemplate), "maths");

    /// <summary>Renders a full template file.</summary>
    public static string Render(string name, params (string Name, string Value)[] values) =>
        Templates.Render(name, values);

    /// <summary>Renders a declaration fragment followed by a blank line.</summary>
    public static string Fragment(string name, params (string Name, string Value)[] values) =>
        Templates.RenderFragment(name, values);
}
