namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated function-pointer backends for OpenGL APIs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlBackendEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the function-pointer backend implementation.</summary>
    public string Emit(GlBindingModel model)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// Implements <see cref=\"{context.Config.ApiClass}\"/> over function pointers resolved from the current OpenGL context.");
        output.AppendLine("/// Construct it with a proc loader once the context is current.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public unsafe class {context.Config.BackendClass} : {context.Config.ApiClass}");
        output.AppendLine("{");
        EmitFields(output, model.Commands);
        EmitConstructor(output, model.Commands);
        foreach (var command in model.Commands)
            EmitCommand(output, command);
        EmitNotLoaded(output);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits resolved native function-pointer fields.</summary>
    private static void EmitFields(StringBuilder output, IReadOnlyList<GlCommand> commands)
    {
        foreach (var command in commands)
        {
            output.AppendLine($"    /// <summary>Resolved function pointer for <c>{command.NativeName}</c>.</summary>");
            output.AppendLine($"    private readonly {GlSignatureFormatter.DelegateType(command)} {command.NativeName};");
        }
    }

    /// <summary>Emits the backend constructor that resolves all entry points.</summary>
    private void EmitConstructor(StringBuilder output, IReadOnlyList<GlCommand> commands)
    {
        output.AppendLine();
        output.AppendLine("    /// <summary>Resolves every entry point through <paramref name=\"getProcAddress\"/> while the context is current.</summary>");
        output.AppendLine($"    public {context.Config.BackendClass}(Func<string, nint> getProcAddress)");
        output.AppendLine("    {");
        foreach (var command in commands)
            output.AppendLine($"        {command.NativeName} = ({GlSignatureFormatter.DelegateType(command)})getProcAddress(\"{command.NativeName}\");");
        output.AppendLine("    }");
    }

    /// <summary>Emits one backend override.</summary>
    private static void EmitCommand(StringBuilder output, GlCommand command)
    {
        output.AppendLine();
        output.AppendLine("    /// <inheritdoc/>");
        output.AppendLine($"    public override {command.ReturnType} {command.ManagedName}({GlSignatureFormatter.Signature(command)})");
        output.AppendLine("    {");
        output.AppendLine($"        if ({command.NativeName} is null) throw NotLoaded(\"{command.NativeName}\");");
        output.AppendLine($"        {GlSignatureFormatter.BackendCall(command)}");
        output.AppendLine("    }");
    }

    /// <summary>Emits the helper that creates missing-entry-point exceptions.</summary>
    private static void EmitNotLoaded(StringBuilder output)
    {
        output.AppendLine();
        output.AppendLine("    /// <summary>Creates the exception thrown for an unresolved OpenGL entry point.</summary>");
        output.AppendLine("    private static EntryPointNotFoundException NotLoaded(string entryPoint) =>");
        output.AppendLine(
            "        new($\"OpenGL entry point not available in the current context: {entryPoint} " +
            "(the proc loader did not provide it - the function is not in the active GL version or driver).\");");
    }
}
