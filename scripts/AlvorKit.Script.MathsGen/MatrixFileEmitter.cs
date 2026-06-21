namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated matrix source file.</summary>
internal static class MatrixFileEmitter
{
    /// <summary>Returns source code for <paramref name="matrix"/>.</summary>
    public static string Emit(MatrixSpec matrix)
    {
        var members = new MemberBlock();
        MatrixCoreEmitter.Emit(matrix, members);
        MatrixSpanInteropEmitter.Emit(matrix, members);
        MatrixConversionEmitter.Emit(matrix, members);
        MatrixOperatorEmitter.Emit(matrix, members);
        MatrixAlgebraEmitter.Emit(matrix, members);
        MatrixRelationEmitter.Emit(matrix, members);
        MatrixValueSemanticsEmitter.Emit(matrix, members);
        MatrixSystemNumericsEmitter.Emit(matrix, members);
        MatrixTransform2DEmitter.Emit(matrix, members);
        MatrixTransform3x2Emitter.Emit(matrix, members);
        MatrixTransformEmitter.Emit(matrix, members);

        return MathsTemplate.Render(
            "matrix-file.cs.tmpl",
            ("TypeSummary", TypeSummary(matrix)),
            ("ParameterDocs", ParameterDocs(matrix)),
            ("TypeName", matrix.TypeName),
            ("ConstructorParameters", ConstructorParameters(matrix)),
            ("ImplementedInterfaces", ImplementedInterfaces(matrix)),
            ("Members", members.ToString()));
    }

    /// <summary>Returns generated interfaces implemented by one matrix type.</summary>
    private static string ImplementedInterfaces(MatrixSpec matrix)
    {
        var interfaces = new List<string>
        {
            $"IMat{ShapeName(matrix)}<{matrix.TypeName}, {matrix.Scalar.CSharpName}, " +
            $"{matrix.ColumnTypeName}, {matrix.RowTypeName}, {matrix.TransposeTypeName}>",
            $"IMatScalarArithmeticOperators<{matrix.TypeName}, {matrix.Scalar.CSharpName}>",
            $"IMatRelationalOperators<{matrix.TypeName}, {matrix.Scalar.CSharpName}, Vec{matrix.Columns}b>",
            $"IMatQuery<{matrix.TypeName}, {matrix.Scalar.CSharpName}>",
        };

        if (matrix.Columns == 4 && matrix.Rows == 4)
        {
            interfaces.Add($"IMat4Transform<{matrix.TypeName}, {matrix.Scalar.CSharpName}, " +
                $"{matrix.Scalar.VectorName(2)}, {matrix.Scalar.VectorName(3)}, {matrix.Scalar.VectorName(4)}>");
            if (matrix.Scalar.Kind == ScalarKind.Float)
                interfaces.Add($"IMat4SystemNumerics<{matrix.TypeName}>");
        }

        if (matrix.Columns == 3 && matrix.Rows == 2)
        {
            interfaces.Add($"IMat3x2Transform2D<{matrix.TypeName}, {matrix.Scalar.CSharpName}, " +
                $"{matrix.Scalar.VectorName(2)}, {matrix.Scalar.VectorName(3)}, {matrix.TransposeTypeName}>");
        }

        if (matrix.Columns == 3 && matrix.Rows == 2 && matrix.Scalar.Kind == ScalarKind.Float)
        {
            interfaces.Add($"IMat3x2SystemNumerics<{matrix.TypeName}>");
        }

        if (matrix.Columns == 3 && matrix.Rows == 3)
        {
            interfaces.Add($"IMat3Transform2D<{matrix.TypeName}, {matrix.Scalar.CSharpName}, " +
                $"{matrix.Scalar.VectorName(2)}, {matrix.Scalar.VectorName(3)}>");
        }

        return string.Join($",{Environment.NewLine}      ", interfaces);
    }

    /// <summary>Returns the suffix used by shape-specific matrix interfaces.</summary>
    private static string ShapeName(MatrixSpec matrix) => matrix.IsSquare
        ? matrix.Columns.ToString(CultureInfo.InvariantCulture)
        : $"{matrix.Columns.ToString(CultureInfo.InvariantCulture)}x{matrix.Rows.ToString(CultureInfo.InvariantCulture)}";

    private static string TypeSummary(MatrixSpec matrix)
    {
        var shape = matrix.IsSquare ? $"{matrix.Columns}x{matrix.Rows}" : $"{matrix.Columns}-column, {matrix.Rows}-row";
        return $"Column-major {shape} {matrix.Scalar.Description} matrix for game math and graphics APIs.";
    }

    private static string ConstructorParameters(MatrixSpec matrix)
    {
        var pairs = matrix.ColumnParameters.Select(parameter => $"{matrix.ColumnTypeName} {parameter}");
        return string.Join(", ", pairs);
    }

    private static string ParameterDocs(MatrixSpec matrix)
    {
        var builder = new StringBuilder();
        for (var column = 0; column < matrix.Columns; column++)
        {
            builder.Append("/// <param name=\"")
                .Append(matrix.ColumnParameters[column])
                .Append("\">Column ")
                .Append(column.ToString(CultureInfo.InvariantCulture))
                .Append(".</param>")
                .AppendLine();
        }

        return builder.ToString();
    }
}
