namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the generated base OpenGL API contract.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlApiContractEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the generated base API contract.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// {context.Config.ApiSummary}");
        output.AppendLine($"/// Use through a backend implementation, such as {context.Config.BackendClass} from {context.Config.Namespace}.Backend.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public partial class {context.Config.ApiClass}");
        output.AppendLine("{");
        EmitCommands(output, model.Commands);
        GlCallbackSetterEmitter.Emit(output, model);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits all raw virtual command members.</summary>
    private static void EmitCommands(StringBuilder output, IReadOnlyList<GlCommand> commands)
    {
        var first = true;
        foreach (var command in commands)
        {
            if (!first)
                output.AppendLine();
            first = false;
            GlCommandDocEmitter.Emit(output, command);
            var signature = GlSignatureFormatter.Signature(command);
            output.AppendLine(
                $"    public virtual {command.ReturnType} {command.ManagedName}({signature}) => throw new NotImplementedException();");
        }
    }
}
