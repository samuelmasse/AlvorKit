namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated 3D triangle type.</summary>
internal sealed record TriangleSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated triangle type name.</summary>
    public string TypeName => Scalar.TriangleName();

    /// <summary>Gets the matching 3D vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 4D vector type name.</summary>
    public string Vector4TypeName => Scalar.VectorName(4);

    /// <summary>Gets the matching 3D plane type name.</summary>
    public string Plane3TypeName => Scalar.PlaneName();

    /// <summary>Gets the matching 3D ray type name.</summary>
    public string Ray3TypeName => Scalar.RayName();

    /// <summary>Gets the matching 3D axis-aligned box type name.</summary>
    public string Box3TypeName => Scalar.BoxName(3);

    /// <summary>Gets the matching 3D sphere type name.</summary>
    public string Sphere3TypeName => Scalar.SphereName();

    /// <summary>Gets the matching 3D frustum type name.</summary>
    public string Frustum3TypeName => Scalar.FrustumName();

    /// <summary>Gets the matching interval type name.</summary>
    public string IntervalTypeName => Scalar.IntervalName();

    /// <summary>Gets the byte size of the triangle.</summary>
    public int SizeBytes => Scalar.SizeBytes * 9;
}
