namespace AlvorKit.Script.MathsGen;

/// <summary>Builds generated constructor expressions for matrices.</summary>
internal static class MatrixExpression
{
    /// <summary>Returns a line-wrapped target-typed matrix constructor expression.</summary>
    public static string New(MatrixSpec matrix, Func<int, int, string> component)
    {
        var arguments = ColumnArguments(matrix, component);
        return $"new({Environment.NewLine}            {arguments})";
    }

    /// <summary>Returns a line-wrapped matrix column argument list.</summary>
    public static string ColumnArguments(MatrixSpec matrix, Func<int, int, string> component)
    {
        var columns = Enumerable.Range(0, matrix.Columns).Select(column => Column(matrix, column, component));
        return string.Join($",{Environment.NewLine}            ", columns);
    }

    /// <summary>Returns a line-wrapped target-typed vector constructor expression for one matrix column.</summary>
    public static string Column(MatrixSpec matrix, int column, Func<int, int, string> component)
    {
        var components = Enumerable.Range(0, matrix.Rows)
            .Select(row => matrix.Scalar.CastArithmetic(component(column, row)));
        return $"new {matrix.ColumnTypeName}({string.Join(", ", components)})";
    }

    /// <summary>Returns scalar component parameter names in column-major order.</summary>
    public static IEnumerable<string> ComponentParameterNames(MatrixSpec matrix)
    {
        for (var column = 0; column < matrix.Columns; column++)
        {
            for (var row = 0; row < matrix.Rows; row++)
                yield return $"m{column}{row}";
        }
    }
}
