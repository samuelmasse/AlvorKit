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
        return $"""
            {context.Config.Namespace} third-party notices
            ===================================

            The XML documentation comments on the {context.Config.ApiClass} commands ({documented} of
            {model.Commands.Count}) are derived from the Khronos OpenGL reference pages (the gl4
            directory of the OpenGL-Refpages repository, commit {shortDocTag}).

            The reference pages are copyright their respective authors and are made available by
            The Khronos Group under the SGI Free Software License B, version 2.0. Portions are
            copyright (c) 1991-2006 Silicon Graphics, Inc. and (c) 2010-2014 The Khronos Group Inc.

            Source and license text:
              https://github.com/KhronosGroup/OpenGL-Refpages
              https://www.khronos.org/registry/OpenGL-Refpages/LICENSES/LicenseRef-FreeB.txt

            """;
    }
}
