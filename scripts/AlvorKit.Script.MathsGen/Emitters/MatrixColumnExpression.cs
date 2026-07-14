namespace AlvorKit.Script.MathsGen;

/// <summary>Builds matrix expressions from exact-order operations on complete public columns.</summary>
internal static class MatrixColumnExpression
{
    /// <summary>Gets whether the column vector has a retained packed unary implementation.</summary>
    public static bool SupportsUnary(MatrixSpec matrix, string op)
    {
        var column = new VectorSpec(matrix.Rows, matrix.Scalar);
        return matrix.Scalar.Kind == ScalarKind.Float && op == "-" || DoubleVectorExpression.SupportsUnary(column, op) ||
            IsDoubleFourByFour(matrix) && op == "-";
    }

    /// <summary>Gets whether the column vector has a retained packed vector-pair implementation.</summary>
    public static bool SupportsPair(MatrixSpec matrix, string op)
    {
        var column = new VectorSpec(matrix.Rows, matrix.Scalar);
        return matrix.Scalar.Kind == ScalarKind.Float && op is "+" or "-" or "*" or "/" ||
            DoubleVectorExpression.SupportsBinary(column, op) || IsDoubleFourByFour(matrix) && op is "+" or "-" or "*";
    }

    /// <summary>Gets whether the column vector has a retained packed vector-right-scalar implementation.</summary>
    public static bool SupportsVectorScalar(MatrixSpec matrix, string op)
    {
        var column = new VectorSpec(matrix.Rows, matrix.Scalar);
        return matrix.Scalar.Kind == ScalarKind.Float && op is "*" or "/" ||
            DoubleVectorExpression.SupportsVectorScalar(column, op) || IsDoubleFourByFour(matrix) && op == "*";
    }

    /// <summary>Gets whether the column vector has a retained packed scalar-left-vector implementation.</summary>
    public static bool SupportsScalarVector(MatrixSpec matrix, string op) =>
        matrix.Scalar.Kind == ScalarKind.Float && op == "*";

    /// <summary>Gets whether ordered column linear combinations use retained packed operations.</summary>
    public static bool SupportsOrderedProduct(MatrixSpec matrix) =>
        matrix.Scalar.Kind == ScalarKind.Float || IsDoubleFourByFour(matrix);

    /// <summary>Gets whether the complete Mat4d caller won with direct public-column composition.</summary>
    public static bool IsDoubleFourByFour(MatrixSpec matrix) =>
        matrix.Columns == 4 && matrix.Rows == 4 && matrix.Scalar.Kind == ScalarKind.Double;

    /// <summary>Constructs a matrix from one expression per column.</summary>
    public static string New(MatrixSpec matrix, Func<int, string> column) =>
        $"new({string.Join(", ", Enumerable.Range(0, matrix.Columns).Select(column))})";

    /// <summary>Constructs a unary matrix expression from public columns.</summary>
    public static string Unary(MatrixSpec matrix, string op, string value) =>
        New(matrix, column => $"{op}{value}.Column{column}");

    /// <summary>Constructs a vector-pair matrix expression from corresponding public columns.</summary>
    public static string Pair(MatrixSpec matrix, string left, string op, string right) =>
        New(matrix, column => $"{left}.Column{column} {op} {right}.Column{column}");

    /// <summary>Constructs a vector-right-scalar matrix expression from public columns.</summary>
    public static string VectorScalar(MatrixSpec matrix, string left, string op, string right) =>
        New(matrix, column => $"{left}.Column{column} {op} {right}");

    /// <summary>Constructs a scalar-left-vector matrix expression from public columns.</summary>
    public static string ScalarVector(MatrixSpec matrix, string left, string op, string right) =>
        New(matrix, column => $"{left} {op} {right}.Column{column}");

    /// <summary>Constructs one exact left-associative column linear combination.</summary>
    public static string MatrixVector(MatrixSpec matrix, string left, string right)
    {
        var terms = Enumerable.Range(0, matrix.Columns)
            .Select(column => $"{left}.Column{column} * {right}.{MatrixSpec.ColumnComponent(column)}");
        return string.Join(" + ", terms);
    }
}
