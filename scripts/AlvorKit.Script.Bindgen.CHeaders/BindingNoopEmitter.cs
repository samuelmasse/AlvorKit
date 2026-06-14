namespace AlvorKit.Script.Bindgen;

/// <summary>Emits a generated null-object API implementation.</summary>
internal sealed class BindingNoopEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the noop class source file.</summary>
    public string Noop(BindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>A <see cref=\"{context.Config.ApiClass}\"/> that ignores calls and returns default values.</summary>");
        output.AppendLine($"public class {context.Config.ApiClass}Noop : {context.Config.ApiClass}");
        output.AppendLine("{");
        foreach (var function in model.Functions)
            NoopMethod(output, function);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits one noop override.</summary>
    private static void NoopMethod(StringBuilder output, BindingFunction function)
    {
        var header = $"    public override {function.ReturnType} {function.ManagedName}({BindingSignature.ForFunction(function)})";
        var outParameters = function.Parameters.Where(parameter => parameter.Modifier == "out").ToList();
        output.AppendLine("    /// <inheritdoc/>");
        if (outParameters.Count == 0)
        {
            output.AppendLine(function.ReturnType == "void" ? header + " { }" : header + " => default;");
            return;
        }
        output.AppendLine(header);
        output.AppendLine("    {");
        foreach (var parameter in outParameters)
            output.AppendLine($"        {parameter.ManagedName} = default;");
        if (function.ReturnType != "void")
            output.AppendLine("        return default;");
        output.AppendLine("    }");
    }
}
