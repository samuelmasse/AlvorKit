namespace AlvorKit.Script.Bindgen;

/// <summary>Emits out-parameter overloads for scalar OpenGL getters.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlOutScalarOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Emits out-parameter overloads for Get* commands whose trailing pointer holds one value.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (!command.ManagedName.StartsWith("Get") || parameters.Count == 0)
            return;
        if (parameters[^1] is not { PointeeType: not null, PointeeIsChar: false, PointeeIsConst: false } pointer)
            return;
        var len = state.ParseLen(command, pointer);
        var singleValued = len is { Kind: GlExtensionLenKind.Literal, Divisor: 1 }
            || (len.Kind == GlExtensionLenKind.CompSize && len.CompSizeArgs is ["pname"]);
        if (!singleValued)
            return;

        var passthrough = parameters.Take(parameters.Count - 1).ToList();
        var signature = passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").ToList();
        signature.Add($"out {pointer.PointeeType} {pointer.ManagedName}");
        if (!state.AddSignature($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append("(nint)(&value)");
        GlExtensionDocEmitter.Emit(
            output,
            state.Config,
            command,
            $"Reads a single value, returned through the <paramref name=\"{GlExtensionNames.Local(pointer)}\"/> " +
            "out parameter via a stack address.");
        output.AppendLine($"    public virtual void {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine($"        {pointer.PointeeType} value;");
        output.AppendLine($"        this.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine($"        {pointer.ManagedName} = value;");
        output.AppendLine("    }");
        output.AppendLine();
    }
}
