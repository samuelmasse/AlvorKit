namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated OpenGL API and backend project files.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlProjectEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the generated public API project file.</summary>
    public string EmitApiProject(string version) =>
        TemplateResource.Render(
            typeof(GlProjectEmitter),
            "res/templates/bindgen/opengl-registry/api-project.csproj.tmpl",
            ("XmlBanner", context.XmlBanner()),
            ("Version", version));

    /// <summary>Emits the backend project file that references the generated API project.</summary>
    public string EmitBackendProject(string version, string apiProjectName) =>
        TemplateResource.Render(
            typeof(GlProjectEmitter),
            "res/templates/bindgen/opengl-registry/backend-project.csproj.tmpl",
            ("XmlBanner", context.XmlBanner()),
            ("Version", version),
            ("ApiProjectName", apiProjectName));
}
