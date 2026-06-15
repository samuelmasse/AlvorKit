namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated public API base class.</summary>
internal sealed class BindingApiEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the generated API contract source.</summary>
    public string ApiContract(BindingModel model)
    {
        var hasStringConvenience = model.Functions.Any(function => function.Parameters.Any(parameter => parameter.HasStringConvenience));
        var output = context.SourceHeader();
        if (hasStringConvenience)
            output.AppendLine("using System.Text;").AppendLine();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// {context.Config.ApiSummary}");
        output.AppendLine($"/// Use through a backend implementation, such as {context.Config.BackendClass} from {context.Config.Namespace}.Backend.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public class {context.Config.ApiClass}");
        output.AppendLine("{");
        EmitConstants(output, model);
        EmitVirtualMethods(output, model);
        foreach (var function in model.Functions)
            new BindingTypedOverloadEmitter(context).TypedOverloads(output, function);
        BindingCallbackSetterEmitter.CallbackSetters(output, model);
        foreach (var function in model.Functions)
            BindingStringReturnEmitter.StringReturn(output, function);
        new BindingSpanReturnEmitter(context).SpanReturns(output, model);
        if (hasStringConvenience)
            BindingUtf8HelperEmitter.Utf8Helper(output);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits public constants on the API class.</summary>
    private static void EmitConstants(StringBuilder output, BindingModel model)
    {
        foreach (var constant in model.Constants)
        {
            output.AppendLine($"    /// <summary>Native constant value for <c>{constant.ManagedName}</c>.</summary>");
            output.AppendLine(constant.Value is >= int.MinValue and <= int.MaxValue
                ? $"    public const int {constant.ManagedName} = {constant.Value};"
                : $"    public const long {constant.ManagedName} = {constant.Value};");
        }
    }

    /// <summary>Emits virtual native function methods on the API class.</summary>
    private static void EmitVirtualMethods(StringBuilder output, BindingModel model)
    {
        var first = model.Constants.Count == 0;
        foreach (var function in model.Functions)
        {
            if (!first)
                output.AppendLine();
            first = false;
            BindingDocs.Function(output, function);
            var signature = BindingSignature.ForFunction(function);
            output.AppendLine(
                $"    public virtual {function.ReturnType} {function.ManagedName}({signature}) => throw new NotImplementedException();");
        }
    }
}
