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
        members.Append(Operator(summary, target.TypeName, op, $"{matrix.TypeName} value",
            MatrixExpression.New(target, (column, row) => expr($"value[{column}, {row}]"))));
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
        MatrixExpression.New(target, (column, row) =>
            $"({target.Scalar.CSharpName}){left}[{column}, {row}] {op} ({target.Scalar.CSharpName}){right}");

    private static string ScalarLeftExpression(MatrixSpec target, string op, string left, string right) =>
        MatrixExpression.New(target, (column, row) =>
            $"({target.Scalar.CSharpName}){left} {op} ({target.Scalar.CSharpName}){right}[{column}, {row}]");

    private static string MatrixPairExpression(MatrixSpec target, string op, string left, string right) =>
        MatrixExpression.New(target, (column, row) =>
            $"({target.Scalar.CSharpName}){left}[{column}, {row}] {op} ({target.Scalar.CSharpName}){right}[{column}, {row}]");

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
            members.Append(Operator("Multiplies two column-major matrices.", result.TypeName, "*",
                $"{matrix.TypeName} left, {right.TypeName} right", MatrixProduct(matrix, result, "left", "right")));
        }
    }

    private static string VectorProduct(MatrixSpec matrix, ScalarSpec result, string left, string right)
    {
        var expressions = Enumerable.Range(0, matrix.Rows).Select(row => Sum(matrix.Columns,
            column => $"({result.CSharpName}){left}[{column}, {row}] * ({result.CSharpName}){right}.{MatrixSpec.ColumnComponent(column)}"));
        return NumericFunctionsEmitter.NewRaw(expressions);
    }

    private static string RowVectorProduct(MatrixSpec matrix, ScalarSpec result, string left, string right)
    {
        var expressions = Enumerable.Range(0, matrix.Columns).Select(column => Sum(matrix.Rows,
            row => $"({result.CSharpName}){left}.{MatrixSpec.ColumnComponent(row)} * ({result.CSharpName}){right}[{column}, {row}]"));
        return NumericFunctionsEmitter.NewRaw(expressions);
    }

    private static string MatrixProduct(MatrixSpec left, MatrixSpec result, string leftName, string rightName) =>
        MatrixExpression.New(result, (column, row) => Sum(left.Columns,
            inner => $"({result.Scalar.CSharpName}){leftName}[{inner}, {row}] * ({result.Scalar.CSharpName}){rightName}[{column}, {inner}]"));

    private static string Sum(int count, Func<int, string> component) =>
        string.Join(" + ", Enumerable.Range(0, count).Select(component));

    private static string Operator(string summary, string returnType, string op, string parameters, string expression) =>
        MathsTemplate.Fragment("operator-expression.csfrag.tmpl", ("Summary", summary), ("ReturnType", returnType),
            ("Operator", op), ("Parameters", parameters), ("Expression", expression));
}
