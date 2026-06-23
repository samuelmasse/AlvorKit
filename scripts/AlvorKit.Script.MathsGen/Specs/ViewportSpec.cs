namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated viewport type.</summary>
internal sealed record ViewportSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated viewport type name.</summary>
    public string TypeName => Scalar.ViewportName();

    /// <summary>Gets the matching 2D box type name.</summary>
    public string Box2TypeName => Scalar.BoxName(2);

    /// <summary>Gets the matching interval type name.</summary>
    public string IntervalTypeName => Scalar.IntervalName();

    /// <summary>Gets the matching 2D vector type name.</summary>
    public string Vector2TypeName => Scalar.VectorName(2);

    /// <summary>Gets the matching 3D vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 4D vector type name.</summary>
    public string Vector4TypeName => Scalar.VectorName(4);

    /// <summary>Gets the matching 4x4 matrix type name.</summary>
    public string Matrix4TypeName => Scalar.MatrixName(4, 4);

    /// <summary>Gets the matching 3D ray type name.</summary>
    public string Ray3TypeName => Scalar.RayName();

    /// <summary>Gets the byte size of the viewport.</summary>
    public int SizeBytes => Scalar.SizeBytes * 6;
}
