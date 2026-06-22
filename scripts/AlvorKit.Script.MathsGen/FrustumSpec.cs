namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated 3D frustum type.</summary>
internal sealed record FrustumSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated frustum type name.</summary>
    public string TypeName => Scalar.FrustumName();

    /// <summary>Gets the matching 3D vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 4D vector type name.</summary>
    public string Vector4TypeName => Scalar.VectorName(4);

    /// <summary>Gets the matching 4x4 matrix type name.</summary>
    public string Matrix4TypeName => Scalar.MatrixName(4, 4);

    /// <summary>Gets the matching 3D plane type name.</summary>
    public string Plane3TypeName => Scalar.PlaneName();

    /// <summary>Gets the matching 3D axis-aligned box type name.</summary>
    public string Box3TypeName => Scalar.BoxName(3);

    /// <summary>Gets the matching 3D sphere type name.</summary>
    public string Sphere3TypeName => Scalar.SphereName();

    /// <summary>Gets the byte size of the frustum.</summary>
    public int SizeBytes => Scalar.SizeBytes * 24;
}
