namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests retained Vec4 Int32 packed bit-function generation.</summary>
[TestClass]
public sealed class Int32BitFunctionGenerationTest
{
    /// <summary>Vec4i and Vec4u use the measured hardware-gated packed kernels.</summary>
    [TestMethod]
    public void Vec4Int32_UsesPackedBitKernels()
    {
        var signed = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));
        var unsigned = VectorFileEmitter.Emit(new(4, VectorCatalog.UInt));

        foreach (var source in new[] { signed, unsigned })
        {
            StringAssert.Contains(source, "System.Runtime.Intrinsics.X86.Ssse3.IsSupported");
            StringAssert.Contains(source, "BitCountPacked(PackUInt32(value))");
            StringAssert.Contains(source, "System.Runtime.Intrinsics.X86.Avx512CD.VL.IsSupported");
            StringAssert.Contains(source, "System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported");
            StringAssert.Contains(source, "LeadingZeroCountPacked(PackUInt32(value))");
            StringAssert.Contains(source, "TrailingZeroCountPacked(PackUInt32(value))");
            StringAssert.Contains(source, "FindLeastSignificantBitPacked(PackUInt32(value))");
            StringAssert.Contains(source, "FindMostSignificantBitPacked(PackUInt32(value))");
        }

        StringAssert.Contains(signed, "IsPowerOfTwoPacked(value)");
        StringAssert.Contains(signed, "[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        StringAssert.Contains(unsigned, "ScalarMath.IsPowerOfTwo(value.X)");
        Assert.IsFalse(unsigned.Contains("IsPowerOfTwoPacked", StringComparison.Ordinal));
    }

    /// <summary>Partial-register Int32 vectors retain scalar bit functions.</summary>
    [TestMethod]
    public void Vec2AndVec3Int32_RetainScalarBitFunctions()
    {
        foreach (var vector in new[]
                 {
                     new VectorSpec(2, VectorCatalog.Int),
                     new VectorSpec(3, VectorCatalog.Int),
                     new VectorSpec(2, VectorCatalog.UInt),
                     new VectorSpec(3, VectorCatalog.UInt),
                 })
        {
            var source = VectorFileEmitter.Emit(vector);
            Assert.IsFalse(source.Contains("BitCountPacked", StringComparison.Ordinal));
            StringAssert.Contains(source, "ScalarMath.BitCount(value.X)");
        }
    }
}
