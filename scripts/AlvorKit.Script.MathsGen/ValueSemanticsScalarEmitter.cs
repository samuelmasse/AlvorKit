namespace AlvorKit.Script.MathsGen;

/// <summary>Builds scalar-specific statement bodies for generated vector value semantics.</summary>
internal static class ValueSemanticsScalarEmitter
{
    /// <summary>Returns the scalar equality expression for this vector family.</summary>
    public static string EqualScalarExpression(VectorSpec vector) =>
        vector.Scalar.IsFloating ? "left.Equals(right)" : "left == right";
}
