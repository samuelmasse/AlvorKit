namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated quaternion type.</summary>
internal sealed record QuaternionSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated quaternion type name.</summary>
    public string TypeName => Scalar.QuaternionName();

    /// <summary>Gets the matching Boolean vector mask type name.</summary>
    public string MaskTypeName => VectorCatalog.Bool.VectorName(4);

    /// <summary>Gets the matching 3D vector type name.</summary>
    public string Vector3TypeName => Scalar.VectorName(3);

    /// <summary>Gets the matching 4D vector type name.</summary>
    public string Vector4TypeName => Scalar.VectorName(4);

    /// <summary>Gets the matching 3x3 matrix type name.</summary>
    public string Matrix3TypeName => Scalar.MatrixName(3, 3);

    /// <summary>Gets the matching 4x4 matrix type name.</summary>
    public string Matrix4TypeName => Scalar.MatrixName(4, 4);

    /// <summary>Gets the byte size of the quaternion.</summary>
    public int SizeBytes => Scalar.SizeBytes * 4;
}
