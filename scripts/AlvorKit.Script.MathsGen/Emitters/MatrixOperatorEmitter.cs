namespace AlvorKit.Script.MathsGen;

/// <summary>Emits matrix arithmetic and multiplication operators.</summary>
internal static class MatrixOperatorEmitter
{
    /// <summary>Appends operators for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        EmitUnary(matrix, members, "+", "Returns the C#-promoted value.", ScalarPromotion.UnaryPlusResult, c => c);
        EmitUnary(matrix, members, "-", "Negates every component.", ScalarPromotion.UnaryNegationResult, c => $"-{c}");
        foreach (var (op, description) in new[] { ("+", "addition"), ("-", "subtraction"), ("/", "division"), ("%", "remainder") })
            EmitComponentPair(matrix, members, op, description);
        EmitScalarMultiply(matrix, members);
        EmitMatrixVectorProducts(matrix, members);
        EmitMatrixMatrixProducts(matrix, members);
    }

    private static void EmitUnary(MatrixSpec matrix, MemberBlock members, string op, string summary, Func<ScalarSpec, ScalarSpec?> result, Func<string, string> expr)
    {
        var resultScalar = result(matrix.Scalar);
        if (resultScalar is null)
            return;

        var target = matrix with { Scalar = resultScalar };
        var expression = MatrixColumnExpression.SupportsUnary(matrix, op)
            ? MatrixColumnExpression.Unary(target, op, "value")
            : MatrixExpression.New(target, (column, row) => expr($"value[{column}, {row}]"));
        members.Append(Operator(summary, target.TypeName, op, $"{matrix.TypeName} value",
            expression));
    }

    private static void EmitComponentPair(MatrixSpec matrix, MemberBlock members, string op, string description)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(matrix.Scalar, matrix.Scalar) ?? matrix.Scalar;
        var target = matrix with { Scalar = resultScalar };
        var scalar = matrix.Scalar.CSharpName;
        members.Append(Operator($"Applies component-wise {description} with a scalar.", target.TypeName, op,
            $"{matrix.TypeName} left, {scalar} right", ScalarRightExpression(target, op, "left", "right")));
        members.Append(Operator($"Applies component-wise {description} from a scalar.", target.TypeName, op,
            $"{scalar} left, {matrix.TypeName} right", ScalarLeftExpression(target, op, "left", "right")));
        members.Append(Operator($"Applies component-wise {description}.", target.TypeName, op,
            $"{matrix.TypeName} left, {matrix.TypeName} right", MatrixPairExpression(target, op, "left", "right")));
    }

    private static void EmitScalarMultiply(MatrixSpec matrix, MemberBlock members)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(matrix.Scalar, matrix.Scalar) ?? matrix.Scalar;
        var target = matrix with { Scalar = resultScalar };
        var scalar = matrix.Scalar.CSharpName;
        members.Append(Operator("Multiplies every component by a scalar.", target.TypeName, "*",
            $"{matrix.TypeName} left, {scalar} right", ScalarRightExpression(target, "*", "left", "right")));
        members.Append(Operator("Multiplies every component from a scalar.", target.TypeName, "*",
            $"{scalar} left, {matrix.TypeName} right", ScalarLeftExpression(target, "*", "left", "right")));
    }

    private static string ScalarRightExpression(MatrixSpec target, string op, string left, string right) =>
        MatrixColumnExpression.SupportsVectorScalar(target, op)
            ? MatrixColumnExpression.VectorScalar(target, left, op, right)
            : MatrixExpression.New(target, (column, row) =>
                $"({target.Scalar.CSharpName}){left}[{column}, {row}] {op} ({target.Scalar.CSharpName}){right}");

    private static string ScalarLeftExpression(MatrixSpec target, string op, string left, string right) =>
        MatrixColumnExpression.SupportsScalarVector(target, op)
            ? MatrixColumnExpression.ScalarVector(target, left, op, right)
            : MatrixExpression.New(target, (column, row) =>
                $"({target.Scalar.CSharpName}){left} {op} ({target.Scalar.CSharpName}){right}[{column}, {row}]");

    private static string MatrixPairExpression(MatrixSpec target, string op, string left, string right)
    {
        if (op == "+" && IsSystemMatrix4(target))
            return $"new({left}.packed + {right}.packed)";

        return MatrixColumnExpression.SupportsPair(target, op)
            ? MatrixColumnExpression.Pair(target, left, op, right)
            : MatrixExpression.New(target, (column, row) =>
                $"({target.Scalar.CSharpName}){left}[{column}, {row}] {op} ({target.Scalar.CSharpName}){right}[{column}, {row}]");
    }

    private static void EmitMatrixVectorProducts(MatrixSpec matrix, MemberBlock members)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(matrix.Scalar, matrix.Scalar) ?? matrix.Scalar;
        var leftVector = matrix.Scalar.VectorName(matrix.Rows);
        var rightVector = matrix.Scalar.VectorName(matrix.Columns);
        members.Append(Operator("Transforms a column vector by this matrix.", resultScalar.VectorName(matrix.Rows), "*",
            $"{matrix.TypeName} left, {rightVector} right", VectorProduct(matrix, resultScalar, "left", "right")));
        members.Append(Operator("Transforms a row vector by this matrix.", resultScalar.VectorName(matrix.Columns), "*",
            $"{leftVector} left, {matrix.TypeName} right", RowVectorProduct(matrix, resultScalar, "left", "right")));
    }

    private static void EmitMatrixMatrixProducts(MatrixSpec matrix, MemberBlock members)
    {
        var resultScalar = ScalarPromotion.BinaryNumericResult(matrix.Scalar, matrix.Scalar) ?? matrix.Scalar;
        foreach (var right in MatrixCatalog.Matrices.Where(right => right.Scalar == matrix.Scalar && right.Rows == matrix.Columns))
        {
            var result = new MatrixSpec(right.Columns, matrix.Rows, resultScalar);
            var declaration = Operator("Multiplies two column-major matrices.", result.TypeName, "*",
                $"{matrix.TypeName} left, {right.TypeName} right", MatrixProduct(matrix, result, "left", "right"));
            members.Append(MatrixColumnExpression.IsDoubleFourByFour(matrix) && right.TypeName == matrix.TypeName
                ? Inline(declaration)
                : declaration);
        }
    }

    private static string VectorProduct(MatrixSpec matrix, ScalarSpec result, string left, string right)
    {
        if (IsSystemMatrix4(matrix))
        {
            return $"Unsafe.BitCast<System.Numerics.Vector4, {result.VectorName(4)}>(" +
                "System.Numerics.Vector4.Transform(" +
                $"Unsafe.BitCast<{matrix.Scalar.VectorName(4)}, System.Numerics.Vector4>({right}), " +
                $"{left}.packed))";
        }

        if (MatrixColumnExpression.SupportsOrderedProduct(matrix))
            return MatrixColumnExpression.MatrixVector(matrix, left, right);

        var expressions = Enumerable.Range(0, matrix.Rows).Select(row => Sum(matrix.Columns,
            column => $"({result.CSharpName}){left}[{column}, {row}] * ({result.CSharpName}){right}.{MatrixSpec.ColumnComponent(column)}"));
        return NumericFunctionsEmitter.NewRaw(expressions);
    }

    private static string RowVectorProduct(MatrixSpec matrix, ScalarSpec result, string left, string right)
    {
        if (matrix.Scalar.IsFloating)
        {
            var columnExpressions = Enumerable.Range(0, matrix.Columns)
                .Select(column => $"{matrix.ColumnTypeName}.Dot({left}, {right}.Column{column})");
            return NumericFunctionsEmitter.NewRaw(columnExpressions);
        }

        var expressions = Enumerable.Range(0, matrix.Columns).Select(column => Sum(matrix.Rows,
            row => $"({result.CSharpName}){left}.{MatrixSpec.ColumnComponent(row)} * ({result.CSharpName}){right}[{column}, {row}]"));
        return NumericFunctionsEmitter.NewRaw(expressions);
    }

    private static string MatrixProduct(MatrixSpec left, MatrixSpec result, string leftName, string rightName)
    {
        if (IsSystemMatrix4(left) && result.Columns == 4)
            return $"new({rightName}.packed * {leftName}.packed)";

        if (MatrixColumnExpression.IsDoubleFourByFour(left) && result.Columns == 4)
        {
            return MatrixColumnExpression.New(result,
                column => DirectDoubleFourByFourVector(leftName, $"{rightName}.Column{column}"));
        }

        if (MatrixColumnExpression.SupportsOrderedProduct(left))
        {
            return MatrixColumnExpression.New(result,
                column => MatrixColumnExpression.MatrixVector(left, leftName, $"{rightName}.Column{column}"));
        }

        return MatrixExpression.New(result, (column, row) => Sum(left.Columns,
            inner => $"({result.Scalar.CSharpName}){leftName}[{inner}, {row}] * ({result.Scalar.CSharpName}){rightName}[{column}, {inner}]"));
    }

    /// <summary>Builds one exact ordered Mat4d column without matrix indexers or whole-vector temporaries.</summary>
    private static string DirectDoubleFourByFourVector(string matrix, string vector) =>
        NumericFunctionsEmitter.NewRaw(Enumerable.Range(0, 4).Select(row => Sum(4,
            column => $"{matrix}.Column{column}.{MatrixSpec.RowComponent(row)} * {vector}.{MatrixSpec.ColumnComponent(column)}")));

    private static string Sum(int count, Func<int, string> component) =>
        string.Join(" + ", Enumerable.Range(0, count).Select(component));

    private static bool IsSystemMatrix4(MatrixSpec matrix) =>
        matrix.Columns == 4 && matrix.Rows == 4 && matrix.Scalar.Kind == ScalarKind.Float;

    private static string Operator(string summary, string returnType, string op, string parameters, string expression) =>
        MathsTemplate.Fragment("operator-expression.csfrag.tmpl", ("Summary", summary), ("ReturnType", returnType),
            ("Operator", op), ("Parameters", parameters), ("Expression", expression));

    /// <summary>Marks the measured large Mat4d product for aggressive caller inlining.</summary>
    private static string Inline(string declaration)
    {
        var firstLineEnd = declaration.IndexOf(Environment.NewLine, StringComparison.Ordinal);
        return declaration.Insert(firstLineEnd + Environment.NewLine.Length,
            $"    [MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}");
    }
}
