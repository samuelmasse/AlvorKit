namespace AlvorKit.Script.MathsGen;

/// <summary>Builds expressions over the private packed System.Numerics view of float vectors.</summary>
internal static class FloatVectorExpression
{
    /// <summary>Returns the matching fixed-size System.Numerics type.</summary>
    public static string SystemType(VectorSpec vector) =>
        $"System.Numerics.Vector{vector.Dimension.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns a packed function call wrapped in the vector's zero-cost packed factory.</summary>
    public static string Function(VectorSpec vector, string method, params string[] arguments) =>
        $"new({SystemType(vector)}.{method}({string.Join(", ", arguments.Select(Packed))}))";

    /// <summary>Returns direct access to a vector expression's packed view.</summary>
    public static string Packed(string expression) => $"{expression}.packed";
}
