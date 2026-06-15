namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the UTF-8 helper used by generated string overloads.</summary>
internal static class BindingUtf8HelperEmitter
{
    /// <summary>Emits the stack-first UTF-8 ref struct helper.</summary>
    public static void Utf8Helper(StringBuilder output)
    {
        output.AppendLine();
        output.Append(TemplateResource.Read(typeof(BindingUtf8HelperEmitter), "res/templates/bindgen/c-headers/csharp/utf8-helper.csfrag.tmpl"));
    }
}
