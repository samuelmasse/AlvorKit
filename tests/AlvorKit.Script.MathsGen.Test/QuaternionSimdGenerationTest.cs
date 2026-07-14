namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Verifies selected SIMD emission for generated quaternions.</summary>
[TestClass]
public sealed class QuaternionSimdGenerationTest
{
    /// <summary>Float quaternion component work, Hamilton products, and normalization use the packed System representation while Dot remains ordered.</summary>
    [TestMethod]
    public void Quat_UsesPackedSystemRepresentationForLaneIndependentOperations()
    {
        var source = QuaternionFileEmitter.Emit(new(VectorCatalog.Float));

        StringAssert.Contains(source, "[StructLayout(LayoutKind.Explicit)]");
        StringAssert.Contains(source, "private System.Numerics.Quaternion packed;");
        StringAssert.Contains(source, "FromPacked(left.packed + right.packed)");
        StringAssert.Contains(source, "FromPacked(left.packed * right)");
        StringAssert.Contains(source, "Vector128.Create(int.MinValue, int.MinValue, int.MinValue, 0))");
        StringAssert.Contains(source, "System.Runtime.Intrinsics.Vector128.Create(right)");
        StringAssert.Contains(source, "FromPacked(left.packed * right.packed)");
        StringAssert.Contains(source,
            "System.Runtime.CompilerServices.Unsafe.BitCast<System.Numerics.Quaternion, Quat>(value)");
        StringAssert.Contains(source,
            "(left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W)");
        StringAssert.Contains(source, "var uv = System.Numerics.Vector3.Cross(rotationVector, input);");
        StringAssert.Contains(source, "FromPacked(System.Numerics.Quaternion.Normalize(value.packed))");
        Assert.IsFalse(source.Contains("IsHardwareAccelerated", StringComparison.Ordinal));
    }

    /// <summary>Double quaternion component work and Hamilton products use Vector256 while rotation stays scalar.</summary>
    [TestMethod]
    public void Quatd_UsesVector256ForLaneIndependentOperationsAndRotation()
    {
        var source = QuaternionFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(source, "[StructLayout(LayoutKind.Sequential)]");
        Assert.IsFalse(source.Contains("private System.Numerics.Quaternion packed", StringComparison.Ordinal));
        StringAssert.Contains(source, "Unsafe.BitCast<Quatd, System.Runtime.Intrinsics.Vector256<double>>(left) -");
        StringAssert.Contains(source, "System.Runtime.Intrinsics.Vector256.Create(right)");
        StringAssert.Contains(source, "Vector256.Create(long.MinValue, long.MinValue, long.MinValue, 0L))");
        StringAssert.Contains(source, "var uvX = (rotation.Y * vector.Z) - (rotation.Z * vector.Y);");
        StringAssert.Contains(source, "var uuvZ = (rotation.X * uvY) - (rotation.Y * uvX);");
        StringAssert.Contains(source,
            "System.Runtime.Intrinsics.Vector256.MultiplyAddEstimate(");
        StringAssert.Contains(source, "System.Runtime.Intrinsics.Vector256.Shuffle(");
        StringAssert.Contains(source, "HamiltonProduct(left, right)");
        StringAssert.Contains(source,
            "(left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W)");
        Assert.IsFalse(source.Contains("Unsafe.BitCast<Vec3d", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("IsHardwareAccelerated", StringComparison.Ordinal));
    }

    /// <summary>System quaternion conversions use the exact private packed view without component reconstruction.</summary>
    [TestMethod]
    public void Quat_SystemNumericsInterop_UsesPackedView()
    {
        var source = QuaternionFileEmitter.Emit(new(VectorCatalog.Float));

        StringAssert.Contains(source, $"implicit operator System.Numerics.Quaternion(Quat value) =>{Environment.NewLine}" +
            "        value.packed;");
        StringAssert.Contains(source, $"implicit operator Quat(System.Numerics.Quaternion value) =>{Environment.NewLine}" +
            "        FromPacked(value);");
        Assert.IsFalse(source.Contains(
            $"implicit operator System.Numerics.Quaternion(Quat value) =>{Environment.NewLine}" +
            "        new(value.X, value.Y, value.Z, value.W)",
            StringComparison.Ordinal));
    }
}
