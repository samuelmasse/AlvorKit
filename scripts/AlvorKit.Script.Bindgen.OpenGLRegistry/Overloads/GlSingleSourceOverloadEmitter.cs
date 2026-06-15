namespace AlvorKit.Script.Bindgen;

/// <summary>Emits single-string helpers for OpenGL shader-source-style APIs.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlSingleSourceOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Emits the single-string helper for the shader-source array shape.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (parameters.Count < 3)
            return;
        var (count, strings, lengths) = (parameters[^3], parameters[^2], parameters[^1]);
        if (!MatchesShape(command, count, strings, lengths))
            return;

        var passthrough = parameters.Take(parameters.Count - 3).ToList();
        var signature = passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").Append("string source").ToList();
        if (!state.AddSignature($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName)
            .Append("1")
            .Append("(nint)(&sourcePointer)")
            .Append("(nint)(&length)");
        GlExtensionDocEmitter.Emit(output, state.Config, command, "Marshals <paramref name=\"source\"/> to UTF-8 and passes it with its byte length.");
        output.Append(TemplateResource.RenderFragment(
            typeof(GlSingleSourceOverloadEmitter),
            "res/templates/bindgen/opengl-registry/csharp/single-source.csfrag.tmpl",
            ("ManagedName", command.ManagedName),
            ("Signature", string.Join(", ", signature)),
            ("Arguments", string.Join(", ", arguments))));
    }

    /// <summary>Returns whether the command has the trailing count, string array, length array shape.</summary>
    private bool MatchesShape(GlCommand command, GlParameter count, GlParameter strings, GlParameter lengths)
    {
        if (count is not { PointerDepth: 0, ManagedType: "int" })
            return false;
        if (strings is not { PointerDepth: 2, PointeeIsChar: true, PointeeIsConst: true })
            return false;
        if (lengths is not { PointerDepth: 1, PointeeType: "int", PointeeIsConst: true })
            return false;
        if (state.ParseLen(command, strings) is not { Kind: GlExtensionLenKind.ParamRef } stringsLen || stringsLen.ParamIndex != command.Parameters.Count - 3)
            return false;
        return state.ParseLen(command, lengths) is { Kind: GlExtensionLenKind.ParamRef } lengthsLen
            && lengthsLen.ParamIndex == command.Parameters.Count - 3;
    }
}
