namespace AlvorKit.Script.Bindgen;

/// <summary>Emits raw native P/Invoke imports for the generated backend.</summary>
internal sealed class BindingNativeImportEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the native imports source file.</summary>
    public string NativeImports(BindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine("[assembly: DisableRuntimeMarshalling]");
        output.AppendLine();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>Raw imports from the {context.Config.NativeLibrary} shared library ({context.Config.Namespace}.Native).</summary>");
        output.AppendLine($"internal static partial class {context.Config.NativeClass}");
        output.AppendLine("{");
        output.AppendLine($"    /// <summary>Name of the native shared library.</summary>");
        output.AppendLine($"    private const string Lib = \"{context.Config.NativeLibrary}\";");
        foreach (var function in model.Functions)
        {
            output.AppendLine();
            output.AppendLine($"    /// <summary>Imports native <c>{function.NativeName}</c>.</summary>");
            output.AppendLine($"    [LibraryImport(Lib, EntryPoint = \"{function.NativeName}\")]");
            output.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
            output.AppendLine($"    public static partial {function.ReturnInteropType} {function.ManagedName}({BindingSignature.ForFunction(function, native: true)});");
        }
        output.AppendLine("}");
        return output.ToString();
    }
}
