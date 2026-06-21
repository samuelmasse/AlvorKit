namespace AlvorKit.Script.MathsGen;

/// <summary>Emits equality, hashing, comparison, and text conversion for matrices.</summary>
internal static class MatrixValueSemanticsEmitter
{
    /// <summary>Appends value semantics for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment("matrix-value-semantics.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ColumnType", matrix.ColumnTypeName),
            ("Columns", string.Join(", ", matrix.ColumnNames)),
            ("TryFormatColumns", TryFormatColumns(matrix, utf8: false)),
            ("TryFormatUtf8Columns", TryFormatColumns(matrix, utf8: true)),
            ("CompareColumns", CompareColumns(matrix)),
            ("ParseColumns", ParseColumns(matrix, utf8: false)),
            ("ParseUtf8Columns", ParseColumns(matrix, utf8: true)),
            ("ParsedColumnArguments", string.Join(", ", matrix.ColumnParameters)),
            ("EqualityExpression", EqualityExpression(matrix))));

    private static string TryFormatColumns(MatrixSpec matrix, bool utf8) =>
        ColumnStatements(matrix, utf8 ? TryFormatUtf8ColumnStatement : TryFormatColumnStatement,
            utf8 ? TryFormatUtf8SeparatorStatement : TryFormatSeparatorStatement);

    private static string CompareColumns(MatrixSpec matrix)
    {
        var statements = new List<string>();
        foreach (var column in matrix.ColumnNames)
        {
            var variable = char.ToLowerInvariant(column[0]) + column[1..] + "Comparison";
            statements.Add($"        var {variable} = {column}.CompareTo(other.{column});");
            statements.Add($"        if ({variable} != 0)");
            statements.Add($"            return {variable};");
            statements.Add("");
        }

        return string.Join(Environment.NewLine, statements);
    }

    private static string ParseColumns(MatrixSpec matrix, bool utf8)
    {
        var statements = new List<string>();
        for (var index = 0; index < matrix.Columns; index++)
        {
            var isLast = index == matrix.Columns - 1;
            var method = ParseMethodName(utf8, isLast);
            var source = isLast ? LastColumnSource(utf8) : "ref body";

            statements.Add("");
            statements.Add(
                $"        if (!{method}({source}, formatProvider, out {matrix.ColumnTypeName} {matrix.ColumnParameters[index]}))");
            statements.Add("            return false;");
        }

        return string.Join(Environment.NewLine, statements);
    }

    private static string EqualityExpression(MatrixSpec matrix) =>
        string.Join(" && ", matrix.ColumnNames.Select(column => $"{column}.Equals(other.{column})"));

    private static string ColumnStatements(MatrixSpec matrix, Func<string, string> columnStatement, Func<string> separatorStatement)
    {
        var statements = new List<string>();
        for (var index = 0; index < matrix.Columns; index++)
        {
            if (index > 0)
                statements.Add(separatorStatement());

            statements.Add("");
            statements.Add(columnStatement(matrix.ColumnNames[index]));
        }

        return string.Join(Environment.NewLine, statements);
    }

    private static string TryFormatColumnStatement(string column) =>
        $"        if (!MathsFormatHelper.TryAppendFormatted({column}, ref remainder, ref charsWritten, format, formatProvider))" +
        $"{Environment.NewLine}            return MathsFormatHelper.Fail(out charsWritten);";

    private static string TryFormatUtf8ColumnStatement(string column) =>
        $"        if (!MathsFormatHelper.TryAppendFormatted({column}, ref remainder, ref bytesWritten, format, formatProvider))" +
        $"{Environment.NewLine}            return MathsFormatHelper.Fail(out bytesWritten);";

    private static string TryFormatSeparatorStatement() =>
        "        if (!MathsFormatHelper.TryAppend(\", \".AsSpan(), ref remainder, ref charsWritten))" +
        $"{Environment.NewLine}            return MathsFormatHelper.Fail(out charsWritten);";

    private static string TryFormatUtf8SeparatorStatement() =>
        "        if (!MathsFormatHelper.TryAppend(\", \"u8, ref remainder, ref bytesWritten))" +
        $"{Environment.NewLine}            return MathsFormatHelper.Fail(out bytesWritten);";

    private static string ParseMethodName(bool utf8, bool isLast) => (utf8, isLast) switch
    {
        (true, true) => "MathsParseHelper.TryParseComponent",
        (true, false) => "MathsComponentParseHelper.TryReadNextTopLevelComponent",
        (false, true) => "MathsParseHelper.TryParseComponent",
        _ => "MathsComponentParseHelper.TryReadNextTopLevelComponent",
    };

    private static string LastColumnSource(bool utf8) => utf8 ? "MathsUtf8TextHelper.TrimAsciiWhitespace(body)" : "body.Trim()";
}
