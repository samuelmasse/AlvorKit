namespace AlvorKit.Script.MathsGen;

/// <summary>Renders Maths generator templates from the repository <c>res/templates/maths</c> directory.</summary>
internal static class MathsTemplate
{
    /// <summary>Renders a full template file.</summary>
    public static string Render(string name, params (string Name, string Value)[] values) =>
        TemplateResource.Render(typeof(MathsTemplate), $"res/templates/maths/{name}", values);

    /// <summary>Renders a declaration fragment followed by a blank line.</summary>
    public static string Fragment(string name, params (string Name, string Value)[] values) =>
        TemplateResource.RenderFragment(typeof(MathsTemplate), $"res/templates/maths/{name}", values);
}
