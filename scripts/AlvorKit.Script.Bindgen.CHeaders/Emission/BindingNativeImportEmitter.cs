namespace AlvorKit.Script.Bindgen;

/// <summary>Emits raw native P/Invoke imports for the generated backend.</summary>
internal sealed class BindingNativeImportEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the native imports source file.</summary>
    public string NativeImports(BindingModel model)
    {
        var imports = string.Join("", model.Functions.Select(Import));
        return TemplateResource.Render(
            typeof(BindingNativeImportEmitter),
            "res/templates/bindgen/c-headers/csharp/native-imports.cs.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace),
            ("NativeLibrary", context.Config.NativeLibrary),
            ("NativePackageId", context.Config.Namespace + ".Native"),
            ("NativeClass", context.Config.NativeClass),
            ("Imports", imports));
    }

    /// <summary>Renders one native import declaration.</summary>
    private static string Import(BindingFunction function) =>
        TemplateResource.Render(
            typeof(BindingNativeImportEmitter),
            "res/templates/bindgen/c-headers/csharp/native-import.csfrag.tmpl",
            ("NativeName", function.NativeName),
            ("ReturnInteropType", function.ReturnInteropType),
            ("ManagedName", function.ManagedName),
            ("Signature", BindingSignature.ForFunction(function, native: true)));
}
