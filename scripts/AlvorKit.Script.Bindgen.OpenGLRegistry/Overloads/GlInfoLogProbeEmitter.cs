namespace AlvorKit.Script.Bindgen;

/// <summary>Emits shared probe-and-grow bodies for OpenGL info-log helpers.</summary>
internal static class GlInfoLogProbeEmitter
{
    /// <summary>Emits the shared probe-and-grow body used by string-returning info-log helpers.</summary>
    public static void Emit(StringBuilder output, GlCommand command, string coreArgs, params string[] onComplete)
    {
        output.Append(TemplateResource.RenderFragment(
            typeof(GlInfoLogProbeEmitter),
            "res/templates/bindgen/opengl-registry/csharp/info-log-probe.csfrag.tmpl",
            ("ManagedName", command.ManagedName),
            ("CoreArgs", coreArgs),
            ("OnComplete", string.Join(Environment.NewLine, onComplete) + Environment.NewLine)));
    }
}
