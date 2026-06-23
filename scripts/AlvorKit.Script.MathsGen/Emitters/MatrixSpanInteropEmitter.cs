namespace AlvorKit.Script.MathsGen;

/// <summary>Emits span and array transfer helpers for matrices.</summary>
internal static class MatrixSpanInteropEmitter
{
    /// <summary>Appends span constructors and copy helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment("matrix-span-interop.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("ColumnType", matrix.ColumnTypeName),
            ("ColumnMajorArguments", ColumnsFromSpan(matrix, rowMajor: false)),
            ("RowMajorArguments", ColumnsFromSpan(matrix, rowMajor: true)),
            ("ColumnMajorAssignments", CopyAssignments(matrix, rowMajor: false)),
            ("RowMajorAssignments", CopyAssignments(matrix, rowMajor: true))));

    private static string ColumnsFromSpan(MatrixSpec matrix, bool rowMajor)
    {
        var columns = Enumerable.Range(0, matrix.Columns).Select(column =>
        {
            var components = Enumerable.Range(0, matrix.Rows).Select(row => $"ComponentFromSpan(values, {Index(matrix, column, row, rowMajor)})");
            return $"new {matrix.ColumnTypeName}({string.Join(", ", components)})";
        });

        return string.Join($",{Environment.NewLine}            ", columns);
    }

    private static string CopyAssignments(MatrixSpec matrix, bool rowMajor)
    {
        var assignments = new List<string>();
        for (var column = 0; column < matrix.Columns; column++)
        {
            for (var row = 0; row < matrix.Rows; row++)
            {
                assignments.Add(
                    $"        destination[{Index(matrix, column, row, rowMajor)}] = " +
                    $"{matrix.ColumnNames[column]}.{MatrixSpec.RowComponent(row)};");
            }
        }

        return string.Join(Environment.NewLine, assignments);
    }

    private static string Index(MatrixSpec matrix, int column, int row, bool rowMajor)
    {
        var index = rowMajor ? (row * matrix.Columns) + column : (column * matrix.Rows) + row;
        return index.ToString(CultureInfo.InvariantCulture);
    }
}
