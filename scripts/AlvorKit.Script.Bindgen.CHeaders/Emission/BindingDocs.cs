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
            var text = NativeSummary(
                name,
                function.Documentation?.Parameters.GetValueOrDefault(name),
                $"Native <c>{name}</c> parameter for <c>{function.NativeName}</c>.");
            output.AppendLine($"    /// <param name=\"{name}\">{text}</param>");
        }
        if (function.ReturnType != "void")
            output.AppendLine($"    /// <returns>{ReturnText(function)}</returns>");
        if (function.Documentation?.Remarks is { } remarks)
            output.AppendLine($"    /// <remarks><c>{function.NativeName}</c>: {remarks}</remarks>");
    }

    /// <summary>Emits inherited documentation and standard convenience-overload remarks.</summary>
    public static void InheritedConvenience(StringBuilder output, string cref, string nativeName, string details)
    {
        output.AppendLine($"    /// <inheritdoc cref=\"{cref}\"/>");
        ConvenienceRemarks(output, nativeName, details);
    }

    /// <summary>Emits the standard convenience-overload remarks prefix with operation-specific details.</summary>
    public static void ConvenienceRemarks(StringBuilder output, string nativeName, string details) =>
        output.AppendLine($"    /// <remarks>Managed overload for <c>{nativeName}</c>. {details}</remarks>");

    /// <summary>Returns a documentation sentence that starts from the native C symbol.</summary>
    public static string NativeSummary(string nativeName, string? original, string fallback) =>
        XmlDocComment.NativeSummary(nativeName, original, fallback);

    /// <summary>Capitalizes leading ASCII lowercase text for readable summaries.</summary>
    private static string Capitalize(string text) =>
        text.Length > 0 && char.IsAsciiLetterLower(text[0]) ? char.ToUpperInvariant(text[0]) + text[1..] : text;

    /// <summary>Returns XML documentation text that matches the managed return projection.</summary>
    private static string ReturnText(BindingFunction function) =>
        function.ReturnType == "bool" && function.ReturnInteropType != "bool"
            ? $"true when <c>{function.NativeName}</c> returns non-zero; otherwise, false."
            : function.Documentation?.Returns is { } returns
                ? $"Return value from <c>{function.NativeName}</c>: {returns}"
                : $"Return value from <c>{function.NativeName}</c>.";
}
