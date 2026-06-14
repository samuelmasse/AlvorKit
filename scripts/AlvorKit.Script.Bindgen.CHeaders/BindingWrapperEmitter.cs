namespace AlvorKit.Script.Bindgen;

/// <summary>Emits a generated forwarding wrapper base class.</summary>
internal sealed class BindingWrapperEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the wrapper class source file.</summary>
    public string Wrapper(BindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// A <see cref=\"{context.Config.ApiClass}\"/> that forwards every call to an inner instance.");
        output.AppendLine("/// Override only the calls you want to intercept; the rest pass straight through.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public class {context.Config.ApiClass}Wrapper({context.Config.ApiClass} inner) : {context.Config.ApiClass}");
        output.AppendLine("{");
        output.AppendLine("    /// <summary>The instance each call is forwarded to.</summary>");
        output.AppendLine($"    protected {context.Config.ApiClass} Inner {{ get; }} = inner ?? throw new ArgumentNullException(nameof(inner));");
        foreach (var function in model.Functions)
        {
            output.AppendLine();
            var arguments = string.Join(", ", function.Parameters.Select(BindingSignature.Argument));
            output.AppendLine("    /// <inheritdoc/>");
            output.AppendLine($"    public override {function.ReturnType} {function.ManagedName}({BindingSignature.ForFunction(function)}) => Inner.{function.ManagedName}({arguments});");
        }
        output.AppendLine("}");
        return output.ToString();
    }
}
