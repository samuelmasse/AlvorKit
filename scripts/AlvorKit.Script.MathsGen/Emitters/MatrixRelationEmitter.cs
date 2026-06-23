namespace AlvorKit.Script.MathsGen;

/// <summary>Emits matrix comparison masks and matrix query helpers.</summary>
internal static class MatrixRelationEmitter
{
    /// <summary>Appends relational and query helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (!matrix.Scalar.IsFloating)
            return;

        members.Append(MathsTemplate.Fragment("matrix-relations.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("ColumnMaskType", ColumnMaskType(matrix)),
            ("ExactEqualColumns", ExactEqualColumns(matrix)),
            ("EpsilonEqualColumns", EpsilonEqualColumns(matrix, "epsilon")),
            ("MatrixEpsilonEqualColumns", MatrixEpsilonEqualColumns(matrix, "epsilon")),
            ("NullColumns", NullColumns(matrix)),
            ("IdentityExpression", IdentityExpression(matrix))));

        if (!matrix.IsSquare)
            return;

        members.Append(MathsTemplate.Fragment("matrix-square-query.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("OneLiteral", matrix.Scalar.OneLiteral),
            ("NormalizedColumnsAndRows", NormalizedColumnsAndRows(matrix)),
            ("OrthogonalColumnsAndRows", OrthogonalColumnsAndRows(matrix))));
    }

    private static string ColumnMaskType(MatrixSpec matrix) =>
        $"Vec{matrix.Columns.ToString(CultureInfo.InvariantCulture)}b";

    private static string ExactEqualColumns(MatrixSpec matrix)
    {
        var columns = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"{matrix.ColumnTypeName}.Equal(left.Column{column}, right.Column{column}).All");
        return string.Join($",{Environment.NewLine}            ", columns);
    }

    private static string EpsilonEqualColumns(MatrixSpec matrix, string epsilon)
    {
        var columns = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"({matrix.ColumnTypeName}.Abs(left.Column{column} - right.Column{column}) <= {epsilon}).All");
        return string.Join($",{Environment.NewLine}            ", columns);
    }

    private static string MatrixEpsilonEqualColumns(MatrixSpec matrix, string epsilon)
    {
        var columns = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"({matrix.ColumnTypeName}.Abs(left.Column{column} - right.Column{column}) <= {epsilon}.Column{column}).All");
        return string.Join($",{Environment.NewLine}            ", columns);
    }

    private static string NullColumns(MatrixSpec matrix)
    {
        var columns = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"({matrix.ColumnTypeName}.Abs(value.Column{column}) <= epsilon).All");
        return string.Join($" &&{Environment.NewLine}        ", columns);
    }

    private static string IdentityExpression(MatrixSpec matrix)
    {
        var expressions = new List<string>();
        for (var column = 0; column < matrix.Columns; column++)
        {
            for (var row = 0; row < matrix.Rows; row++)
            {
                var expected = column == row ? matrix.Scalar.OneLiteral : matrix.Scalar.ZeroLiteral;
                expressions.Add($"ScalarMath.Abs(value[{column}, {row}] - {expected}) <= epsilon");
            }
        }

        return string.Join($" &&{Environment.NewLine}        ", expressions);
    }

    private static string NormalizedColumnsAndRows(MatrixSpec matrix)
    {
        var expressions = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"ScalarMath.Abs(value.Column{column}.LengthSquared - {matrix.Scalar.OneLiteral}) <= epsilon")
            .Concat(Enumerable.Range(0, matrix.Rows)
                .Select(row => $"ScalarMath.Abs(value.Row{row}.LengthSquared - {matrix.Scalar.OneLiteral}) <= epsilon"));
        return string.Join($" &&{Environment.NewLine}        ", expressions);
    }

    private static string OrthogonalColumnsAndRows(MatrixSpec matrix)
    {
        var expressions = new List<string>();
        for (var left = 0; left < matrix.Columns; left++)
        {
            for (var right = left + 1; right < matrix.Columns; right++)
                expressions.Add($"ScalarMath.Abs({matrix.ColumnTypeName}.Dot(value.Column{left}, value.Column{right})) <= epsilon");
        }

        for (var left = 0; left < matrix.Rows; left++)
        {
            for (var right = left + 1; right < matrix.Rows; right++)
                expressions.Add($"ScalarMath.Abs({matrix.RowTypeName}.Dot(value.Row{left}, value.Row{right})) <= epsilon");
        }

        return string.Join($" &&{Environment.NewLine}        ", expressions);
    }
}
