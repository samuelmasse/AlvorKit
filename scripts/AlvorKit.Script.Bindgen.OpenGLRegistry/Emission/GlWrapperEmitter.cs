namespace AlvorKit.Script.Bindgen;

/// <summary>Emits forwarding wrapper classes for generated OpenGL APIs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlWrapperEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the forwarding base used by validation, tracing, or resource-tracking layers.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// A <see cref=\"{context.Config.ApiClass}\"/> that forwards every call to an inner instance. Subclass it and");
        output.AppendLine("/// override only the calls you want to intercept; the rest pass straight through.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public class {context.Config.ApiClass}Wrapper({context.Config.ApiClass} inner) : {context.Config.ApiClass}");
        output.AppendLine("{");
        output.AppendLine("    /// <summary>The instance each call is forwarded to.</summary>");
        output.AppendLine($"    protected {context.Config.ApiClass} Inner {{ get; }} = inner ?? throw new ArgumentNullException(nameof(inner));");
        foreach (var command in model.Commands)
            EmitCommand(output, command);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits one forwarding override.</summary>
    private static void EmitCommand(StringBuilder output, GlCommand command)
    {
        output.AppendLine();
        var arguments = string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedName));
        var signature = GlSignatureFormatter.Signature(command);
        output.AppendLine("    /// <inheritdoc/>");
        output.AppendLine(
            $"    public override {command.ReturnType} {command.ManagedName}({signature}) => " +
            $"Inner.{command.ManagedName}({arguments});");
    }
}
