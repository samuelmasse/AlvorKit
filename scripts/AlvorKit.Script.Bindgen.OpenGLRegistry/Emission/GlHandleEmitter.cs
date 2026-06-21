namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated strongly typed OpenGL handle wrappers.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlHandleEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits typed wrappers around GL object names plus a generic handle.</summary>
    public string Emit(GlBindingModel model)
    {
        var handles = string.Join("", model.HandleTypes.Select(Handle)).TrimEnd('\r', '\n');
        return TemplateResource.Render(
            typeof(GlHandleEmitter),
            "res/templates/bindgen/opengl-registry/csharp/handles.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("Handles", handles));
    }

    /// <summary>Renders one generated handle record struct.</summary>
    private static string Handle(string handle)
    {
        var widening = handle == "GlHandle" ? "" : TemplateResource.Render(
            typeof(GlHandleEmitter),
            "res/templates/bindgen/opengl-registry/csharp/handle-widening.csfrag.tmpl",
            ("Handle", handle));
        return TemplateResource.Render(
            typeof(GlHandleEmitter),
            "res/templates/bindgen/opengl-registry/csharp/handle.csfrag.tmpl",
            ("Summary", handle == "GlHandle"
                ? "Strongly typed wrapper for any <c>GLuint</c> OpenGL object name from <c>gl.xml</c>; every specific handle widens to it."
                : "Strongly typed wrapper for a <c>GLuint</c> OpenGL object name from <c>gl.xml</c>."),
            ("Handle", handle),
            ("Widening", widening));
    }
}
