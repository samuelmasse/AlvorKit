namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated 3D quad type.</summary>
internal sealed record QuadSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated quad type name.</summary>
    public string TypeName => Scalar.QuadName();

    /// <summary>Gets the matching three-component vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 3D box type name.</summary>
    public string Box3TypeName => Scalar.BoxName(3);

    /// <summary>Gets the generated type byte size.</summary>
    public int SizeBytes => 12 * Scalar.SizeBytes;

    /// <summary>Gets the scalar literal used to average four corners.</summary>
    public string FourLiteral => Scalar.Kind == ScalarKind.Float ? "4f" : "4d";
}
