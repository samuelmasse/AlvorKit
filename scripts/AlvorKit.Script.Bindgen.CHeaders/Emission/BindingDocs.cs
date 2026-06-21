namespace AlvorKit.Script.Bindgen;

/// <summary>Emits XML documentation for generated methods.</summary>
internal static class BindingDocs
{
    private const int XmlDocTextColumn = 130;

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
            Remarks(output, function.NativeName, remarks);
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

    /// <summary>Returns a wrapped XML documentation summary block.</summary>
    public static string Summary(string documentation, string indent = "") => Element("summary", documentation, indent);

    /// <summary>Returns a wrapped XML documentation parameter block.</summary>
    public static string Parameter(string name, string documentation, string indent = "") =>
        Element($"param name=\"{name}\"", documentation, indent, "param");

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

    /// <summary>Returns a wrapped XML documentation block for one element.</summary>
    private static string Element(string openElement, string documentation, string indent, string? closeElement = null)
    {
        closeElement ??= openElement;
        var output = new StringBuilder();
        output.AppendLine($"{indent}/// <{openElement}>");
        foreach (var line in WrapXmlDocLine(documentation))
            output.AppendLine($"{indent}/// {line}");
        output.AppendLine($"{indent}/// </{closeElement}>");
        return output.ToString();
    }

    /// <summary>Emits wrapped remark paragraphs so large upstream docs stay readable in generated source.</summary>
    private static void Remarks(StringBuilder output, string nativeName, string remarks)
    {
        output.AppendLine("    /// <remarks>");
        foreach (var paragraph in RemarkParagraphs(nativeName, remarks))
        {
            output.AppendLine("    /// <para>");
            foreach (var line in WrapXmlDocLine(paragraph))
                output.AppendLine($"    /// {line}");
            output.AppendLine("    /// </para>");
        }
        output.AppendLine("    /// </remarks>");
    }

    /// <summary>Returns native-anchored remark paragraphs split at common upstream documentation headings.</summary>
    private static IEnumerable<string> RemarkParagraphs(string nativeName, string remarks)
    {
        var sectioned = Regex.Replace(
            remarks,
            @"(^|\s+)(Parameters|Return Value|Thread Safety|Callback Safety|Remarks|See Also|Example \d+)\s+",
            match => $"{(match.Groups[1].Value.Length == 0 ? "" : Environment.NewLine)}{match.Groups[2].Value}: ");
        var first = true;
        foreach (var paragraph in sectioned.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return first ? $"<c>{nativeName}</c>: {paragraph}" : paragraph;
            first = false;
        }
    }

    /// <summary>Wraps one XML documentation paragraph at whitespace without splitting XML tags.</summary>
    private static IEnumerable<string> WrapXmlDocLine(string text)
    {
        var remaining = text.Trim();
        while (remaining.Length > XmlDocTextColumn)
        {
            var length = Math.Min(XmlDocTextColumn, remaining.Length);
            var breakAt = remaining.LastIndexOf(' ', length - 1, length);
            if (breakAt <= 0)
                breakAt = length;
            yield return remaining[..breakAt].TrimEnd();
            remaining = remaining[breakAt..].TrimStart();
        }
        if (remaining.Length > 0)
            yield return remaining;
    }
}
