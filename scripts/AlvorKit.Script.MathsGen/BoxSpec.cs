namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated axis-aligned box type.</summary>
internal sealed record BoxSpec(int Dimension, ScalarSpec Scalar)
{
    /// <summary>Gets the generated box type name.</summary>
    public string TypeName => Scalar.BoxName(Dimension);

    /// <summary>Gets the matching vector type name.</summary>
    public string VectorTypeName => Scalar.VectorName(Dimension);

    /// <summary>Gets the scalar return type used by vector distance helpers.</summary>
    public string DistanceTypeName => Scalar.Kind == ScalarKind.Int ? "float" : Scalar.CSharpName;

    /// <summary>Gets the component names available in the box corners.</summary>
    public IReadOnlyList<string> Components => VectorCatalog.Components.Take(Dimension).ToArray();

    /// <summary>Gets the lower-case component parameter names available in the box corners.</summary>
    public IReadOnlyList<string> Parameters => VectorCatalog.Parameters.Take(Dimension).ToArray();
}
