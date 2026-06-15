namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated managed delegates for OpenGL callback typedefs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlDelegateEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits a blittable callback delegate used by a rooted typed-setter overload.</summary>
    public string Emit(GlDelegate callback)
    {
        var signature = string.Join(", ", callback.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        return TemplateResource.Render(
            typeof(GlDelegateEmitter),
            "res/templates/bindgen/opengl-registry/csharp/delegate.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("NativeName", callback.NativeName),
            ("ReturnType", callback.ReturnType),
            ("ManagedName", callback.ManagedName),
            ("Signature", signature));
    }
}
