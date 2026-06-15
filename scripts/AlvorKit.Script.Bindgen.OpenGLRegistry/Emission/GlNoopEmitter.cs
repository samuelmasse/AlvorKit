namespace AlvorKit.Script.Bindgen;

/// <summary>Emits null-object OpenGL implementations for tests and headless paths.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlNoopEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits a null-object GL implementation for tests and headless paths.</summary>
    public string Emit(GlBindingModel model)
    {
        var methods = string.Join("", model.Commands.Select(Method));
        return TemplateResource.Render(
            typeof(GlNoopEmitter),
            "res/templates/bindgen/opengl-registry/csharp/noop.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("Methods", methods));
    }

    /// <summary>Renders one no-op override.</summary>
    private static string Method(GlCommand command) =>
        TemplateResource.Render(
            typeof(GlNoopEmitter),
            "res/templates/bindgen/opengl-registry/csharp/noop-method.csfrag.tmpl",
            ("ReturnType", command.ReturnType),
            ("ManagedName", command.ManagedName),
            ("Signature", GlSignatureFormatter.Signature(command)),
            ("Body", command.ReturnType == "void" ? "{ }" : "=> default;"));
}
