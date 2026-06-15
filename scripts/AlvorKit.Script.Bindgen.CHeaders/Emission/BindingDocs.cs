namespace AlvorKit.Script.Bindgen;

/// <summary>Emits XML documentation for generated methods.</summary>
internal static class BindingDocs
{
    /// <summary>Emits summary, parameter, return, and remark XML docs for a function.</summary>
    public static void Function(StringBuilder output, BindingFunction function)
    {
        if (function.Documentation?.Summary is { } summary)
        {
            var purpose = Capitalize(summary);
            var terminated = purpose.EndsWith('.') || purpose.EndsWith('!') || purpose.EndsWith('?');
            output.AppendLine($"    /// <summary><c>{function.NativeName}</c> - {purpose}{(terminated ? "" : ".")}</summary>");
        }
        else
        {
            output.AppendLine($"    /// <summary>Calls native <c>{function.NativeName}</c>.</summary>");
        }

        foreach (var parameter in function.Parameters)
        {
            var name = parameter.ManagedName.TrimStart('@');
            var text = function.Documentation?.Parameters.GetValueOrDefault(name) ?? $"Native <c>{name}</c> parameter.";
            output.AppendLine($"    /// <param name=\"{name}\">{text}</param>");
        }
        if (function.ReturnType != "void")
            output.AppendLine($"    /// <returns>{ReturnText(function)}</returns>");
        if (function.Documentation?.Remarks is { } remarks)
            output.AppendLine($"    /// <remarks>{remarks}</remarks>");
    }

    /// <summary>Emits inherited documentation and standard convenience-overload remarks.</summary>
    public static void InheritedConvenience(StringBuilder output, string cref, string details)
    {
        output.AppendLine($"    /// <inheritdoc cref=\"{cref}\"/>");
        ConvenienceRemarks(output, details);
    }

    /// <summary>Emits the standard convenience-overload remarks prefix with operation-specific details.</summary>
    public static void ConvenienceRemarks(StringBuilder output, string details) =>
        output.AppendLine(
            $"    /// <remarks>Convenience overload. {details}</remarks>");

    /// <summary>Capitalizes leading ASCII lowercase text for readable summaries.</summary>
    private static string Capitalize(string text) =>
        text.Length > 0 && char.IsAsciiLetterLower(text[0]) ? char.ToUpperInvariant(text[0]) + text[1..] : text;

    /// <summary>Returns XML documentation text that matches the managed return projection.</summary>
    private static string ReturnText(BindingFunction function) =>
        function.ReturnType == "bool" && function.ReturnInteropType != "bool"
            ? "true when the native function returns non-zero; otherwise, false."
            : function.Documentation?.Returns ?? "Native return value.";
}
