namespace AlvorKit.Script.Bindgen;

/// <summary>Emits shared helper members used by generated OpenGL extension overloads.</summary>
internal static class GlExtensionHelperEmitter
{
    /// <summary>Appends shared helper members to the generated partial API class.</summary>
    public static void Append(StringBuilder output)
    {
        output.Append(TemplateResource.Read(typeof(GlExtensionHelperEmitter), "res/templates/bindgen/opengl-registry/csharp/extension-helpers.csfrag.tmpl"));
    }
}
