namespace AlvorKit.Script.Bindgen;

/// <summary>Emits decoded string overloads for OpenGL commands that return C strings.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlStringGetterOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Emits decoded string overloads for commands whose raw return is a C string pointer.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        if (!command.ReturnsCString)
            return;
        var leading = command.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").ToList();
        var callArgs = string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedName));
        EmitOutString(output, command, leading, callArgs);
        EmitSpanString(output, command, leading, callArgs);
    }

    /// <summary>Emits an allocating out-string overload.</summary>
    private void EmitOutString(StringBuilder output, GlCommand command, IReadOnlyList<string> leading, string callArgs)
    {
        var outStringSignature = string.Join(", ", leading.Append("out string? value"));
        if (!state.AddSignature($"{command.ManagedName}({outStringSignature})"))
            return;
        GlExtensionDocEmitter.Emit(output, state.Config, command, "Decodes the returned C string into <paramref name=\"value\"/>, or null when GL returns no string.");
        output.AppendLine($"    public virtual void {command.ManagedName}({outStringSignature})");
        output.AppendLine("    {");
        output.AppendLine($"        value = Marshal.PtrToStringUTF8(this.{command.ManagedName}({callArgs}));");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>Emits a non-allocating span decode overload.</summary>
    private void EmitSpanString(StringBuilder output, GlCommand command, IReadOnlyList<string> leading, string callArgs)
    {
        var spanSignature = string.Join(
            ", ",
            leading.Append("Span<char> destination").Append("out ReadOnlySpan<char> result"));
        if (!state.AddSignature($"{command.ManagedName}({spanSignature})"))
            return;
        GlExtensionDocEmitter.Emit(
            output,
            state.Config,
            command,
            "Decodes the returned C string into <paramref name=\"destination\"/> and sets " +
            "<paramref name=\"result\"/> to the slice written.");
        output.AppendLine($"    public virtual void {command.ManagedName}({spanSignature})");
        output.AppendLine("    {");
        output.AppendLine($"        var pointer = this.{command.ManagedName}({callArgs});");
        output.AppendLine("        if (pointer == 0) { result = default; return; }");
        output.AppendLine("        var bytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)pointer);");
        output.AppendLine("        System.Text.Unicode.Utf8.ToUtf16(bytes, destination, out _, out var written);");
        output.AppendLine("        result = destination[..written];");
        output.AppendLine("    }");
        output.AppendLine();
    }
}
