namespace AlvorKit.Script.Bindgen;

/// <summary>Emits forwarding wrapper classes for generated OpenGL APIs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlWrapperEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the forwarding base used by validation, tracing, or resource-tracking layers.</summary>
    public string Emit(GlBindingModel model)
    {
        var methods = string.Join("", model.Commands.Select(Method));
        return TemplateResource.Render(
            typeof(GlWrapperEmitter),
            "res/templates/bindgen/opengl-registry/csharp/wrapper.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("Methods", methods));
    }

    /// <summary>Renders one forwarding override.</summary>
    private static string Method(GlCommand command)
    {
        var arguments = string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedName));
        return TemplateResource.Render(
            typeof(GlWrapperEmitter),
            "res/templates/bindgen/opengl-registry/csharp/wrapper-method.csfrag.tmpl",
            ("ReturnType", command.ReturnType),
            ("ManagedName", command.ManagedName),
            ("Signature", GlSignatureFormatter.Signature(command)),
            ("Arguments", arguments));
    }
}
