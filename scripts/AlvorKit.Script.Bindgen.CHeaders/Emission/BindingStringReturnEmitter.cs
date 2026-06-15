namespace AlvorKit.Script.Bindgen;

/// <summary>Emits string-return convenience overloads for const C-string functions.</summary>
internal static class BindingStringReturnEmitter
{
    /// <summary>Emits managed string-return overloads when a function returns a const C string.</summary>
    public static void StringReturn(StringBuilder output, BindingFunction function, string apiClass)
    {
        if (!function.ReturnsCString)
            return;
        var taken = function.Parameters.Select(parameter => parameter.ManagedName.TrimStart('@')).ToHashSet();
        var value = Unique(taken, "value");
        var destination = Unique(taken, "destination");
        var result = Unique(taken, "result");
        var leading = function.Parameters.Select(BindingSignature.Parameter).ToList();
        var callArguments = string.Join(", ", function.Parameters.Select(BindingSignature.Argument));
        var inheritCref = $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})";
        var call = $"{function.ManagedName}({callArguments})";

        output.AppendLine();
        BindingDocs.InheritedConvenience(
            output,
            inheritCref,
            "Decodes the returned C string to a managed string, or null when the pointer is null.");
        var stringParameters = string.Join(", ", leading.Append($"out string? {value}"));
        output.AppendLine(
            $"    public void {function.ManagedName}({stringParameters}) => " +
            $"{value} = Marshal.PtrToStringUTF8({call});");

        output.AppendLine();
        BindingDocs.InheritedConvenience(
            output,
            inheritCref,
            $"Decodes the returned C string into <paramref name=\"{destination}\"/> and returns the slice written.");
        var spanParameters = string.Join(
            ", ",
            leading.Append($"Span<char> {destination}").Append($"out ReadOnlySpan<char> {result}"));
        output.AppendLine($"    public unsafe void {function.ManagedName}({spanParameters})");
        output.AppendLine("    {");
        output.AppendLine($"        var pointer = {call};");
        output.AppendLine($"        if (pointer == 0) {{ {result} = default; return; }}");
        output.AppendLine("        var bytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)pointer);");
        output.AppendLine($"        System.Text.Unicode.Utf8.ToUtf16(bytes, {destination}, out _, out var written);");
        output.AppendLine($"        {result} = {destination}[..written];");
        output.AppendLine("    }");
    }

    /// <summary>Returns a unique generated parameter name.</summary>
    private static string Unique(HashSet<string> taken, string preferred)
    {
        while (taken.Contains(preferred))
            preferred += "_";
        taken.Add(preferred);
        return preferred;
    }
}
