namespace AlvorKit.Script.Bindgen;

/// <summary>Emits third-party notice files for generated OpenGL documentation content.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlThirdPartyNoticeEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits attribution for XML documentation derived from Khronos refpages.</summary>
    public string Emit(GlBindingModel model)
    {
        var documented = model.Commands.Count(command => command.Documentation is not null);
        if (documented == 0)
            return $"{context.Config.Namespace} contains no third-party content." + Environment.NewLine;

        var shortDocTag = context.DocTag.Length >= 12 ? context.DocTag[..12] : context.DocTag;
        return TemplateResource.Render(
            typeof(GlThirdPartyNoticeEmitter),
            "res/templates/bindgen/opengl-registry/third-party-notices.txt.tmpl",
            ("Namespace", context.Config.Namespace),
            ("ApiClass", context.Config.ApiClass),
            ("DocumentedCommands", documented.ToString()),
            ("CommandCount", model.Commands.Count.ToString()),
            ("ShortDocTag", shortDocTag));
    }
}
