namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated vector type.</summary>
internal sealed record VectorSpec(int Dimension, ScalarSpec Scalar)
{
    /// <summary>Gets the generated vector type name.</summary>
    public string TypeName => Scalar.VectorName(Dimension);

    /// <summary>Gets the matching Boolean vector type for this dimension.</summary>
    public string BoolTypeName => VectorCatalog.Bool.VectorName(Dimension);

    /// <summary>Gets the matching integer vector type for this dimension.</summary>
    public string IntTypeName => VectorCatalog.Int.VectorName(Dimension);

    /// <summary>Gets the component names available in this vector.</summary>
    public IReadOnlyList<string> Components => VectorCatalog.Components.Take(Dimension).ToArray();

    /// <summary>Gets the lower-case primary constructor parameter names.</summary>
    public IReadOnlyList<string> Parameters => VectorCatalog.Parameters.Take(Dimension).ToArray();
}
