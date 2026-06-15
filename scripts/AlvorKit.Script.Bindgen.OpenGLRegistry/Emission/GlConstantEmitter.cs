namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated constants for OpenGL tokens too wide for enum backing types.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlConstantEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits 64-bit sentinel tokens that cannot fit in uint-backed enum types.</summary>
    public string Emit(GlBindingModel model)
    {
        var constants = string.Join(Environment.NewLine, model.WideConstants.Select(Constant));
        return TemplateResource.Render(
            typeof(GlConstantEmitter),
            "res/templates/bindgen/opengl-registry/csharp/constants.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("Constants", constants));
    }

    /// <summary>Renders one wide OpenGL constant.</summary>
    private static string Constant(GlConstant constant) =>
        TemplateResource.Render(
            typeof(GlConstantEmitter),
            "res/templates/bindgen/opengl-registry/csharp/constant.csfrag.tmpl",
            ("NativeName", constant.NativeName),
            ("Availability", GlCodeEmissionContext.AvailabilityText(constant.Availability)),
            ("ManagedName", constant.ManagedName),
            ("Value", constant.Value.ToString("X")));
}
