namespace AlvorKit.Script.Bindgen;

/// <summary>Emits null-object OpenGL implementations for tests and headless paths.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlNoopEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits a null-object GL implementation for tests and headless paths.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine($"/// <summary>A <see cref=\"{context.Config.ApiClass}\"/> that ignores every call and returns default values.</summary>");
        output.AppendLine($"public class {context.Config.ApiClass}Noop : {context.Config.ApiClass}");
        output.AppendLine("{");
        foreach (var command in model.Commands)
            EmitCommand(output, command);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits one no-op override.</summary>
    private static void EmitCommand(StringBuilder output, GlCommand command)
    {
        output.AppendLine("    /// <inheritdoc/>");
        output.AppendLine(command.ReturnType == "void"
            ? $"    public override void {command.ManagedName}({GlSignatureFormatter.Signature(command)}) {{ }}"
            : $"    public override {command.ReturnType} {command.ManagedName}({GlSignatureFormatter.Signature(command)}) => default;");
    }
}
