namespace AlvorKit.Script.Bindgen;

/// <summary>Emits singular Gen/Create/Delete-style OpenGL helpers.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlSingularOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Emits singular helpers for one trailing counted pointer.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (parameters.Count == 0 || parameters[^1] is not { PointeeType: not null, PointeeIsChar: false } pointer)
            return;
        if (parameters.Count(parameter => parameter.PointerDepth > 0) != 1)
            return;
        if (state.ParseLen(command, pointer) is not { Kind: GlExtensionLenKind.ParamRef, Divisor: 1 } len || len.ParamIndex != parameters.Count - 2)
            return;

        var name = GlExtensionNames.Depluralize(command.ManagedName);
        var singular = GlExtensionNames.Depluralize(pointer.ManagedName.TrimStart('@'));
        if (name == command.ManagedName || state.CommandNames.Contains(name))
            return;

        var passthrough = parameters.Take(parameters.Count - 2).ToList();
        var signature = passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").ToList();
        if (pointer.PointeeIsConst)
            signature.Add($"{pointer.PointeeType} {singular}");
        if (!state.AddSignature($"{name}({string.Join(", ", signature)})"))
            return;
        EmitBody(output, command, pointer, name, singular, passthrough, signature);
    }

    /// <summary>Emits the singular helper body for const input or writable output pointers.</summary>
    private void EmitBody(
        StringBuilder output,
        GlCommand command,
        GlParameter pointer,
        string name,
        string singular,
        IReadOnlyList<GlParameter> passthrough,
        IReadOnlyList<string> signature)
    {
        var count = GlExtensionNames.CountExpression(command.Parameters[^2], "1");
        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append(count);
        GlExtensionDocEmitter.Emit(output, state.Config, command, pointer.PointeeIsConst
            ? $"Passes the single <paramref name=\"{singular}\"/> with a count of 1, taking its address for the call."
            : "Returns the single value written, calling with a count of 1 and a stack address for the out pointer.");
        if (pointer.PointeeIsConst)
            EmitConstBody(output, command, name, singular, signature, arguments);
        else
            EmitWritableBody(output, command, pointer, name, singular, signature, arguments);
    }

    /// <summary>Emits a singular helper for a const input pointer.</summary>
    private static void EmitConstBody(
        StringBuilder output,
        GlCommand command,
        string name,
        string singular,
        IReadOnlyList<string> signature,
        IEnumerable<string> arguments)
    {
        output.Append(TemplateResource.RenderFragment(
            typeof(GlSingularOverloadEmitter),
            "res/templates/bindgen/opengl-registry/csharp/singular-const.csfrag.tmpl",
            ("Name", name),
            ("Signature", string.Join(", ", signature)),
            ("ManagedName", command.ManagedName),
            ("Arguments", string.Join(", ", arguments.Append($"(nint)(&{singular})")))));
    }

    /// <summary>Emits a singular helper for a writable output pointer.</summary>
    private static void EmitWritableBody(
        StringBuilder output,
        GlCommand command,
        GlParameter pointer,
        string name,
        string singular,
        IReadOnlyList<string> signature,
        IEnumerable<string> arguments)
    {
        var returnType = pointer.PointeeType ?? throw new InvalidOperationException($"Parameter '{pointer.ManagedName}' has no pointee type.");
        output.Append(TemplateResource.RenderFragment(
            typeof(GlSingularOverloadEmitter),
            "res/templates/bindgen/opengl-registry/csharp/singular-writable.csfrag.tmpl",
            ("ReturnType", returnType),
            ("Name", name),
            ("Signature", string.Join(", ", signature)),
            ("LocalName", singular),
            ("ManagedName", command.ManagedName),
            ("Arguments", string.Join(", ", arguments.Append($"(nint)(&{singular})")))));
    }
}
