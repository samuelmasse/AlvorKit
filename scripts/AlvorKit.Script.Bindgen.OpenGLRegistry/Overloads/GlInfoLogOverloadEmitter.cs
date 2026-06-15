namespace AlvorKit.Script.Bindgen;

/// <summary>Emits OpenGL info-log and generated-name string helper overloads.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlInfoLogOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Emits helpers for the trailing bufSize, length, buffer pattern.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        if (!TryMatch(command, out var leading, out var coreArgs))
            return;
        EmitStringReturn(output, command, leading, coreArgs);
        EmitSpanReturn(output, command, leading, coreArgs);
    }

    /// <summary>Matches the info-log tail shape and returns reusable call fragments.</summary>
    private bool TryMatch(GlCommand command, out List<string> leading, out string coreArgs)
    {
        leading = [];
        coreArgs = "";
        var parameters = command.Parameters;
        if (parameters.Count < 3)
            return false;
        var (bufSize, length, buffer) = (parameters[^3], parameters[^2], parameters[^1]);
        if (bufSize is not { PointerDepth: 0, ManagedType: "int" })
            return false;
        if (length is not { PointeeType: "int" } || state.ParseLen(command, length) is not { Kind: GlExtensionLenKind.Literal, Divisor: 1 })
            return false;
        if (buffer is not { PointerDepth: 1, PointeeIsChar: true, PointeeIsConst: false })
            return false;
        if (state.ParseLen(command, buffer) is not { Kind: GlExtensionLenKind.ParamRef } bufferLen || bufferLen.ParamIndex != parameters.Count - 3)
            return false;

        leading = [.. parameters.Take(parameters.Count - 3).Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}")];
        coreArgs = string.Join(", ", parameters.Take(parameters.Count - 3).Select(parameter => parameter.ManagedName)
            .Append("buffer.Length")
            .Append("(nint)(&written)")
            .Append("(nint)bufferPtr"));
        return true;
    }

    /// <summary>Emits an allocating string-return overload.</summary>
    private void EmitStringReturn(StringBuilder output, GlCommand command, IReadOnlyList<string> leading, string coreArgs)
    {
        if (!state.AddSignature($"{command.ManagedName}({string.Join(", ", leading)})"))
            return;
        GlExtensionDocEmitter.Emit(
            output,
            state.Config,
            command,
            "Returns the full text, growing the work buffer as needed; the only allocation is the returned string.");
        output.AppendLine($"    public virtual string {command.ManagedName}({string.Join(", ", leading)})");
        GlInfoLogProbeEmitter.Emit(output, command, coreArgs, "                    return Encoding.UTF8.GetString(buffer[..written]);");
    }

    /// <summary>Emits a caller-buffer span decode overload.</summary>
    private void EmitSpanReturn(StringBuilder output, GlCommand command, IReadOnlyList<string> leading, string coreArgs)
    {
        var spanSignature = string.Join(", ", leading.Append("Span<char> destination"));
        if (!state.AddSignature($"{command.ManagedName}({spanSignature})"))
            return;
        GlExtensionDocEmitter.Emit(output, state.Config, command, "Decodes the UTF-8 text into <paramref name=\"destination\"/> and returns the characters written.");
        output.Append(TemplateResource.RenderFragment(
            typeof(GlInfoLogOverloadEmitter),
            "res/templates/bindgen/opengl-registry/csharp/info-log-span.csfrag.tmpl",
            ("ManagedName", command.ManagedName),
            ("Signature", spanSignature),
            ("CoreArgs", coreArgs)));
    }
}
