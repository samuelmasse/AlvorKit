namespace AlvorKit.Script.MathsGen;

/// <summary>Builds component-order statement blocks for generated vector value semantics.</summary>
internal static class ValueSemanticsSequenceEmitter
{
    /// <summary>Returns the statements that append formatted components and separators.</summary>
    public static string TryFormatComponents(VectorSpec vector) =>
        ComponentStatements(vector, TryFormatComponentStatement, TryFormatSeparatorStatement);

    /// <summary>Returns the statements that append UTF-8 formatted components and separators.</summary>
    public static string TryFormatUtf8Components(VectorSpec vector) =>
        ComponentStatements(vector, TryFormatUtf8ComponentStatement, TryFormatUtf8SeparatorStatement);

    /// <summary>Returns the statements that compare components in vector order.</summary>
    public static string CompareComponents(VectorSpec vector)
    {
        var statements = new List<string>();
        foreach (var component in vector.Components)
        {
            var variableName = component.ToLowerInvariant() + "Comparison";
            statements.Add($"        var {variableName} = {component}.CompareTo(other.{component});");
            statements.Add($"        if ({variableName} != 0)");
            statements.Add($"            return {variableName};");
            statements.Add("");
        }

        return string.Join(Environment.NewLine, statements);
    }

    /// <summary>Returns the statements that parse character components in vector order.</summary>
    public static string ParseComponents(VectorSpec vector) => ParseComponents(vector, utf8: false);

    /// <summary>Returns the statements that parse UTF-8 components in vector order.</summary>
    public static string ParseUtf8Components(VectorSpec vector) => ParseComponents(vector, utf8: true);

    /// <summary>Builds component-wise statement lists with optional separator statements.</summary>
    private static string ComponentStatements(
        VectorSpec vector,
        Func<string, string> componentStatement,
        Func<string> separatorStatement)
    {
        var statements = new List<string>();
        for (var index = 0; index < vector.Dimension; index++)
        {
            if (index > 0)
                statements.Add(separatorStatement());

            statements.Add("");
            statements.Add(componentStatement(vector.Components[index]));
        }

        return string.Join(Environment.NewLine, statements);
    }

    /// <summary>Returns character formatting statements for one component.</summary>
    private static string TryFormatComponentStatement(string component) =>
        $"        if (!TryAppendComponent({component}, ref remainder, ref charsWritten, format, formatProvider))" +
        $"{Environment.NewLine}            return FailFormat(out charsWritten);";

    /// <summary>Returns UTF-8 formatting statements for one component.</summary>
    private static string TryFormatUtf8ComponentStatement(string component) =>
        $"        if (!TryAppendUtf8Component({component}, ref remainder, ref bytesWritten, format, formatProvider))" +
        $"{Environment.NewLine}            return FailFormat(out bytesWritten);";

    /// <summary>Returns character separator formatting statements.</summary>
    private static string TryFormatSeparatorStatement() =>
        "        if (!TryAppend(\", \".AsSpan(), ref remainder, ref charsWritten))" +
        $"{Environment.NewLine}            return FailFormat(out charsWritten);";

    /// <summary>Returns UTF-8 separator formatting statements.</summary>
    private static string TryFormatUtf8SeparatorStatement() =>
        "        if (!TryAppendUtf8(\", \"u8, ref remainder, ref bytesWritten))" +
        $"{Environment.NewLine}            return FailFormat(out bytesWritten);";

    /// <summary>Returns component parsing statements for one text encoding.</summary>
    private static string ParseComponents(VectorSpec vector, bool utf8)
    {
        var statements = new List<string>();
        for (var index = 0; index < vector.Dimension; index++)
        {
            var isLast = index == vector.Dimension - 1;
            var method = ParseMethodName(utf8, isLast);
            var source = isLast ? LastComponentSource(utf8) : "ref body";

            statements.Add("");
            statements.Add($"        if (!{method}({source}, formatProvider, out var {vector.Parameters[index]}))");
            statements.Add("            return false;");
        }

        return string.Join(Environment.NewLine, statements);
    }

    /// <summary>Returns the parse helper name for one text encoding and component position.</summary>
    private static string ParseMethodName(bool utf8, bool isLast) => (utf8, isLast) switch
    {
        (true, true) => "TryParseUtf8Component",
        (true, false) => "TryReadUtf8Component",
        (false, true) => "TryParseComponent",
        _ => "TryReadComponent",
    };

    /// <summary>Returns the final component source expression for one text encoding.</summary>
    private static string LastComponentSource(bool utf8) => utf8 ? "TrimAsciiWhitespace(body)" : "body.Trim()";
}
