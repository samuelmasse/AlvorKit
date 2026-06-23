namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated 3D sphere type.</summary>
internal sealed record SphereSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated sphere type name.</summary>
    public string TypeName => Scalar.SphereName();

    /// <summary>Gets the matching 3D vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 3D axis-aligned box type name.</summary>
    public string Box3TypeName => Scalar.BoxName(3);

    /// <summary>Gets the byte size of the sphere.</summary>
    public int SizeBytes => Scalar.SizeBytes * 4;
}
