namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated backend implementation.</summary>
internal sealed class BindingBackendEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the backend class source file.</summary>
    public string Backend(BindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>Implements <see cref=\"{context.Config.ApiClass}\"/> against the {context.Config.NativeLibrary} shared library.</summary>");
        output.AppendLine($"public class {context.Config.BackendClass} : {context.Config.ApiClass}");
        output.AppendLine("{");
        var first = true;
        foreach (var function in model.Functions)
        {
            if (!first)
                output.AppendLine();
            first = false;
            output.AppendLine("    /// <inheritdoc/>");
            output.AppendLine($"    public override {function.ReturnType} {function.ManagedName}({BindingSignature.ForFunction(function)}) => {Body(function)};");
        }
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Formats a backend method body for a native import call.</summary>
    private string Body(BindingFunction function)
    {
        var arguments = string.Join(", ", function.Parameters.Select(BindingSignature.NativeArgument));
        var call = $"{context.Config.NativeClass}.{function.ManagedName}({arguments})";
        return function.ReturnType == function.ReturnInteropType ? call
            : function.ReturnType == "bool" ? $"{call} != 0"
            : $"({function.ReturnType}){call}";
    }
}
