namespace AlvorKit.Script.MathsGen;

/// <summary>Emits transpose, component-wise products, determinants, and inverses.</summary>
internal static class MatrixAlgebraEmitter
{
    /// <summary>Appends algebra helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Property("Gets the transposed matrix.", "readonly",
            matrix.TransposeTypeName, "Transposed", Transpose(matrix, "this")));
        members.Append(NumericFunctionsEmitter.Method("Returns the transpose of a matrix.", "static",
            matrix.TransposeTypeName, "Transpose", $"{matrix.TypeName} value", Transpose(matrix, "value")));
        members.Append(NumericFunctionsEmitter.Method("Multiplies matrices component by component.", "static",
            matrix.TypeName, "ComponentMultiply", $"{matrix.TypeName} left, {matrix.TypeName} right",
            ComponentMultiply(matrix)));
        if (!matrix.IsSquare)
            return;

        var determinantScalar = DeterminantScalar(matrix.Scalar);
        members.Append(NumericFunctionsEmitter.Property("Gets the matrix trace.", "readonly",
            determinantScalar.CSharpName, "Trace", Trace(matrix, determinantScalar)));
        members.Append(NumericFunctionsEmitter.Property("Gets the matrix determinant.", "readonly",
            determinantScalar.CSharpName, "Determinant", Determinant(matrix, determinantScalar)));
        members.Append(NumericFunctionsEmitter.Method("Returns the matrix adjugate.", "static",
            matrix.TypeName, "Adjugate", $"{matrix.TypeName} value", Adjugate(matrix, determinantScalar)));
        if (matrix.Scalar.IsFloating)
        {
            EmitInverse(matrix, members);
            members.Append(NumericFunctionsEmitter.Property("Gets the transposed inverse matrix.", "readonly",
                matrix.TypeName, "InverseTransposed", "InverseTranspose(this)"));
            members.Append(NumericFunctionsEmitter.Method("Returns the transposed inverse of a matrix.", "static",
                matrix.TypeName, "InverseTranspose", $"{matrix.TypeName} value", "Invert(value).Transposed"));
            EmitAffineInverse(matrix, members);
            EmitOrthonormalize(matrix, members);
        }
    }

    private static string Transpose(MatrixSpec matrix, string value)
    {
        var target = new MatrixSpec(matrix.Rows, matrix.Columns, matrix.Scalar);
        if (matrix.Columns == 4 && matrix.Rows == 4 && matrix.Scalar == VectorCatalog.Float)
        {
            return $"new(System.Numerics.Matrix4x4.Transpose({value}.packed))";
        }

        if (matrix.Columns == 4 && matrix.Rows == 4)
            return MatrixColumnExpression.New(target, column => $"{value}.Row{column}");

        return MatrixExpression.New(target, (column, row) => $"{value}[{row}, {column}]");
    }

    private static string ComponentMultiply(MatrixSpec matrix) =>
        MatrixColumnExpression.SupportsPair(matrix, "*")
            ? MatrixColumnExpression.Pair(matrix, "left", "*", "right")
            : MatrixExpression.New(matrix, (column, row) => $"left[{column}, {row}] * right[{column}, {row}]");

    private static ScalarSpec DeterminantScalar(ScalarSpec scalar) =>
        ScalarPromotion.BinaryNumericResult(scalar, scalar) ?? scalar;

    private static string Determinant(MatrixSpec matrix, ScalarSpec scalar) =>
        DetRaw(scalar, "this", Enumerable.Range(0, matrix.Columns).ToArray(), Enumerable.Range(0, matrix.Rows).ToArray());

    private static string Trace(MatrixSpec matrix, ScalarSpec scalar)
    {
        var terms = Enumerable.Range(0, matrix.Columns)
            .Select(index => $"({scalar.CSharpName})this[{index}, {index}]");
        return string.Join(" + ", terms);
    }

    private static string Det2(ScalarSpec scalar, string m00, string m10, string m01, string m11) =>
        $"(({scalar.CSharpName}){m00} * ({scalar.CSharpName}){m11}) - (({scalar.CSharpName}){m10} * ({scalar.CSharpName}){m01})";

    private static string DetRaw(ScalarSpec scalar, string value, IReadOnlyList<int> columns, IReadOnlyList<int> rows) => columns.Count switch
    {
        1 => $"(({scalar.CSharpName}){value}[{columns[0]}, {rows[0]}])",
        2 => Det2(scalar,
            $"{value}[{columns[0]}, {rows[0]}]",
            $"{value}[{columns[1]}, {rows[0]}]",
            $"{value}[{columns[0]}, {rows[1]}]",
            $"{value}[{columns[1]}, {rows[1]}]"),
        _ => DetExpanded(scalar, value, columns, rows),
    };

    private static string DetExpanded(ScalarSpec scalar, string value, IReadOnlyList<int> columns, IReadOnlyList<int> rows)
    {
        var terms = new List<string>();
        for (var index = 0; index < columns.Count; index++)
        {
            var column = columns[index];
            var minorColumns = columns.Where(candidate => candidate != column).ToArray();
            var minorRows = rows.Skip(1).ToArray();
            var term = $"(({scalar.CSharpName}){value}[{column}, {rows[0]}] * ({DetRaw(scalar, value, minorColumns, minorRows)}))";
            terms.Add(index % 2 == 0 ? term : $"- {term}");
        }

        return terms[0] + string.Concat(terms.Skip(1).Select(term => term.StartsWith("-", StringComparison.Ordinal) ? $" {term}" : $" + {term}"));
    }

    private static string Adjugate(MatrixSpec matrix, ScalarSpec scalar) =>
        MatrixExpression.New(matrix, (column, row) =>
            Sign(column, row) + MinorDeterminant(matrix, scalar, column, row));

    private static string MinorDeterminant(MatrixSpec matrix, ScalarSpec scalar, int column, int row)
    {
        var columns = Enumerable.Range(0, matrix.Columns).Where(candidate => candidate != row).ToArray();
        var rows = Enumerable.Range(0, matrix.Rows).Where(candidate => candidate != column).ToArray();
        return DetRaw(scalar, "value", columns, rows);
    }

    private static string Sign(int column, int row) => (column + row) % 2 == 0 ? string.Empty : "-";

    private static void EmitInverse(MatrixSpec matrix, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment(
            matrix.Columns == 4 && matrix.Rows == 4 && matrix.Scalar.Kind == ScalarKind.Float
                ? "matrix-inverse-system4.csfrag.tmpl"
                : "matrix-inverse.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
            ("Dimension", matrix.Columns.ToString(CultureInfo.InvariantCulture)),
            ("Width", (matrix.Columns * 2).ToString(CultureInfo.InvariantCulture)),
            ("WorkLength", (matrix.Columns * matrix.Columns * 2).ToString(CultureInfo.InvariantCulture)),
            ("InitializeWork", InitializeWork(matrix)),
            ("ResultArguments", InverseResult(matrix))));

    private static string InitializeWork(MatrixSpec matrix)
    {
        var statements = new List<string>();
        for (var row = 0; row < matrix.Rows; row++)
        {
            for (var column = 0; column < matrix.Columns; column++)
                statements.Add($"        work[{row * matrix.Columns * 2 + column}] = value[{column}, {row}];");
            for (var column = 0; column < matrix.Columns; column++)
                statements.Add($"        work[{row * matrix.Columns * 2 + matrix.Columns + column}] = " +
                    (row == column ? matrix.Scalar.OneLiteral : matrix.Scalar.ZeroLiteral) + ";");
        }

        return string.Join(Environment.NewLine, statements);
    }

    private static string InverseResult(MatrixSpec matrix) =>
        MatrixExpression.New(matrix, (column, row) => $"work[{(row * matrix.Columns * 2) + matrix.Columns + column}]");

    private static void EmitAffineInverse(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Columns == 3)
        {
            members.Append(MathsTemplate.Fragment("matrix-affine-inverse3.csfrag.tmpl",
                ("TypeName", matrix.TypeName),
                ("LinearType", matrix.Scalar.MatrixName(2, 2)),
                ("Vector2Type", matrix.Scalar.VectorName(2)),
                ("Vector3Type", matrix.Scalar.VectorName(3)),
                ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
                ("OneLiteral", matrix.Scalar.OneLiteral)));
        }

        if (matrix.Columns == 4)
        {
            members.Append(MathsTemplate.Fragment("matrix-affine-inverse4.csfrag.tmpl",
                ("TypeName", matrix.TypeName),
                ("LinearType", matrix.Scalar.MatrixName(3, 3)),
                ("Vector3Type", matrix.Scalar.VectorName(3)),
                ("Vector4Type", matrix.Scalar.VectorName(4)),
                ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
                ("OneLiteral", matrix.Scalar.OneLiteral)));
        }
    }

    private static void EmitOrthonormalize(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Columns != 3)
            return;

        members.Append(MathsTemplate.Fragment("matrix-orthonormalize3.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("Vector3Type", matrix.Scalar.VectorName(3))));
    }
}
