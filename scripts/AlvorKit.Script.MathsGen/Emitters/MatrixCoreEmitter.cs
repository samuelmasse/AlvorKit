namespace AlvorKit.Script.MathsGen;

/// <summary>Emits fields, constructors, constants, rows, and indexers for matrices.</summary>
internal static class MatrixCoreEmitter
{
    /// <summary>Appends core members for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        members.Append(Const("The number of matrix columns.", "ColumnCount", matrix.Columns));
        members.Append(Const("The number of matrix rows.", "RowCount", matrix.Rows));
        members.Append(Const($"The number of scalar components in a {matrix.TypeName}.", "ComponentCount", matrix.ComponentCount));
        members.Append(Const($"The byte size of a {matrix.TypeName}.", "SizeInBytes", matrix.ComponentCount * matrix.Scalar.SizeBytes));
        EmitFields(matrix, members);
        EmitConstructors(matrix, members);
        EmitFactories(matrix, members);
        EmitInterfaceMembers(matrix, members);
        EmitDiagonal(matrix, members);
        EmitRows(matrix, members);
        members.Append(MathsTemplate.Fragment("matrix-indexers.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ColumnType", matrix.ColumnTypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("MaxColumnIndex", (matrix.Columns - 1).ToString(CultureInfo.InvariantCulture))));
    }

    private static string Const(string summary, string name, int value) =>
        MathsTemplate.Fragment("const-int.csfrag.tmpl", ("Summary", summary), ("Name", name), ("Value", value.ToString(CultureInfo.InvariantCulture)));

    private static void EmitFields(MatrixSpec matrix, MemberBlock members)
    {
        for (var column = 0; column < matrix.Columns; column++)
        {
            members.Append(MathsTemplate.Fragment("matrix-column-field.csfrag.tmpl",
                ("FieldOffset", ColumnFieldOffset(matrix, column)),
                ("ColumnType", matrix.ColumnTypeName),
                ("Name", matrix.ColumnNames[column]),
                ("Initializer", matrix.ColumnParameters[column]),
                ("Column", column.ToString(CultureInfo.InvariantCulture))));
        }

        if (MatrixSystemNumericsEmitter.SupportsPackedStorage(matrix))
        {
            members.Append(MathsTemplate.Fragment("matrix-packed-system-field.csfrag.tmpl",
                ("PackedType", MatrixSystemNumericsEmitter.PackedType(matrix))));
            members.Append(MathsTemplate.Fragment("matrix-packed-system-constructor.csfrag.tmpl",
                ("TypeName", matrix.TypeName),
                ("PackedType", MatrixSystemNumericsEmitter.PackedType(matrix)),
                ("Arguments", string.Join(", ", Enumerable.Repeat("default", matrix.Columns)))));
        }
    }

    private static string ColumnFieldOffset(MatrixSpec matrix, int column) =>
        MatrixSystemNumericsEmitter.SupportsPackedStorage(matrix)
            ? $"[FieldOffset({(column * matrix.Rows * matrix.Scalar.SizeBytes).ToString(CultureInfo.InvariantCulture)})]{Environment.NewLine}    "
            : string.Empty;

    private static void EmitConstructors(MatrixSpec matrix, MemberBlock members)
    {
        members.Append(MathsTemplate.Fragment("matrix-diagonal-constructor.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("Arguments", DiagonalArguments(matrix, "diagonal"))));
        members.Append(MathsTemplate.Fragment("matrix-component-constructor.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("Parameters", ComponentParameters(matrix)),
            ("Arguments", ComponentArguments(matrix))));
    }

    private static void EmitFactories(MatrixSpec matrix, MemberBlock members)
    {
        members.Append(NumericFunctionsEmitter.Method("Creates a matrix from column vectors.", "static", matrix.TypeName,
            "CreateColumns", ColumnParameters(matrix), $"new({string.Join(", ", matrix.ColumnParameters)})"));
        members.Append(NumericFunctionsEmitter.Method("Creates a matrix from row vectors.", "static", matrix.TypeName,
            "CreateRows", RowParameters(matrix), MatrixExpression.New(matrix, (column, row) => $"row{row}.{MatrixSpec.ColumnComponent(column)}")));
        members.Append(NumericFunctionsEmitter.Method("Creates a matrix with diagonal components set to a scalar value.", "static",
            matrix.TypeName, "CreateDiagonal", $"{matrix.Scalar.CSharpName} diagonal", $"new(diagonal)"));
        members.Append(NumericFunctionsEmitter.Method("Creates a matrix with diagonal components set from a vector.", "static",
            matrix.TypeName, "CreateDiagonal", $"{DiagonalVectorType(matrix)} diagonal", DiagonalVectorExpression(matrix)));
        members.Append(NumericFunctionsEmitter.Method("Creates a matrix from the outer product of a column vector and row vector.", "static",
            matrix.TypeName, "CreateOuterProduct", $"{matrix.ColumnTypeName} columnVector, {matrix.RowTypeName} rowVector",
            OuterProductExpression(matrix)));
        members.Append(NumericFunctionsEmitter.Method("Linearly interpolates between two matrices component by component.", "static",
            matrix.TypeName, "Lerp", $"{matrix.TypeName} from, {matrix.TypeName} to, {matrix.Scalar.CSharpName} amount",
            LerpExpression(matrix)));
        members.Append(NumericFunctionsEmitter.Property("Gets the zero matrix.", "static", matrix.TypeName, "Zero", "default"));
        if (matrix.IsSquare)
            members.Append(NumericFunctionsEmitter.Property("Gets the identity matrix.", "static", matrix.TypeName,
                "Identity", $"new({matrix.Scalar.OneLiteral})"));
    }

    private static void EmitInterfaceMembers(MatrixSpec matrix, MemberBlock members)
    {
        var interfaceType = $"IMat<{matrix.TypeName}, {matrix.Scalar.CSharpName}, " +
            $"{matrix.ColumnTypeName}, {matrix.RowTypeName}, {matrix.TransposeTypeName}>";
        members.Append(StaticInterfaceProperty("Gets the number of matrix columns.", "int", interfaceType, "ColumnCount", "ColumnCount"));
        members.Append(StaticInterfaceProperty("Gets the number of matrix rows.", "int", interfaceType, "RowCount", "RowCount"));
        members.Append(StaticInterfaceProperty("Gets the number of scalar components.", "int", interfaceType, "ComponentCount", "ComponentCount"));
        members.Append(StaticInterfaceProperty("Gets the byte size.", "int", interfaceType, "SizeInBytes", "SizeInBytes"));
        members.Append(StaticInterfaceProperty("Gets the additive identity.", matrix.TypeName,
            $"IAdditiveIdentity<{matrix.TypeName}, {matrix.TypeName}>", "AdditiveIdentity", "Zero"));
        if (matrix.IsSquare)
            members.Append(StaticInterfaceProperty("Gets the multiplicative identity.", matrix.TypeName,
                $"IMultiplicativeIdentity<{matrix.TypeName}, {matrix.TypeName}>", "MultiplicativeIdentity", "Identity"));
    }

    private static string StaticInterfaceProperty(string summary, string type, string interfaceType, string name, string expression) =>
        MathsTemplate.Fragment("matrix-static-interface-property.csfrag.tmpl", ("Summary", summary), ("Type", type),
            ("Interface", interfaceType), ("Name", name), ("Expression", expression));

    private static void EmitDiagonal(MatrixSpec matrix, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment("matrix-diagonal-property.csfrag.tmpl",
            ("DiagonalType", DiagonalVectorType(matrix)),
            ("Components", DiagonalComponents(matrix)),
            ("Assignments", DiagonalAssignments(matrix))));

    private static void EmitRows(MatrixSpec matrix, MemberBlock members)
    {
        for (var row = 0; row < matrix.Rows; row++)
        {
            members.Append(MathsTemplate.Fragment("matrix-row-property.csfrag.tmpl",
                ("RowType", matrix.RowTypeName),
                ("Name", $"Row{row}"),
                ("Components", RowComponents(matrix, row)),
                ("Assignments", RowAssignments(matrix, row)),
                ("Row", row.ToString(CultureInfo.InvariantCulture))));
        }
    }

    private static string ColumnParameters(MatrixSpec matrix) =>
        string.Join(", ", matrix.ColumnParameters.Select(parameter => $"{matrix.ColumnTypeName} {parameter}"));

    private static string RowParameters(MatrixSpec matrix) =>
        string.Join(", ", Enumerable.Range(0, matrix.Rows).Select(row => $"{matrix.RowTypeName} row{row}"));

    private static string ComponentParameters(MatrixSpec matrix)
    {
        var names = MatrixExpression.ComponentParameterNames(matrix).Select(name => $"{matrix.Scalar.CSharpName} {name}");
        return string.Join(", ", names);
    }

    private static string ComponentArguments(MatrixSpec matrix) =>
        MatrixExpression.ColumnArguments(matrix, (column, row) => $"m{column}{row}");

    private static string DiagonalArguments(MatrixSpec matrix, string diagonal) =>
        MatrixExpression.ColumnArguments(matrix, (column, row) => column == row ? diagonal : matrix.Scalar.ZeroLiteral);

    private static string DiagonalVectorType(MatrixSpec matrix) =>
        matrix.Scalar.VectorName(Math.Min(matrix.Columns, matrix.Rows));

    private static string DiagonalVectorExpression(MatrixSpec matrix) =>
        MatrixExpression.New(matrix, (column, row) =>
            column == row && column < Math.Min(matrix.Columns, matrix.Rows)
                ? $"diagonal.{MatrixSpec.RowComponent(column)}"
                : matrix.Scalar.ZeroLiteral);

    private static string OuterProductExpression(MatrixSpec matrix) =>
        MatrixExpression.New(matrix, (column, row) =>
            $"columnVector.{MatrixSpec.RowComponent(row)} * rowVector.{MatrixSpec.ColumnComponent(column)}");

    private static string LerpExpression(MatrixSpec matrix) =>
        matrix.Scalar.Kind == ScalarKind.Float || MatrixColumnExpression.IsDoubleFourByFour(matrix)
            ? MatrixColumnExpression.New(matrix,
                column => $"from.Column{column} + ((to.Column{column} - from.Column{column}) * amount)")
            : MatrixExpression.New(matrix,
                (column, row) => $"ScalarMath.Lerp(from[{column}, {row}], to[{column}, {row}], amount)");

    private static string DiagonalComponents(MatrixSpec matrix)
    {
        var components = Enumerable.Range(0, Math.Min(matrix.Columns, matrix.Rows))
            .Select(index => $"{matrix.ColumnNames[index]}.{MatrixSpec.RowComponent(index)}");
        return string.Join(", ", components);
    }

    private static string DiagonalAssignments(MatrixSpec matrix)
    {
        var assignments = Enumerable.Range(0, Math.Min(matrix.Columns, matrix.Rows))
            .Select(index =>
                $"            {matrix.ColumnNames[index]}.{MatrixSpec.RowComponent(index)} = value.{MatrixSpec.RowComponent(index)};");
        return string.Join(Environment.NewLine, assignments);
    }

    private static string RowComponents(MatrixSpec matrix, int row)
    {
        var components = Enumerable.Range(0, matrix.Columns).Select(column => $"{matrix.ColumnNames[column]}.{MatrixSpec.RowComponent(row)}");
        return string.Join(", ", components);
    }

    private static string RowAssignments(MatrixSpec matrix, int row)
    {
        var assignments = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"            {matrix.ColumnNames[column]}.{MatrixSpec.RowComponent(row)} = value.{MatrixSpec.ColumnComponent(column)};");
        return string.Join(Environment.NewLine, assignments);
    }
}
