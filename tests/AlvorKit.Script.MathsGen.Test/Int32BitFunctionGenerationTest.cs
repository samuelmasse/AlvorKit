namespace AlvorKit;

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
        StringAssert.Contains(unsigned, "value.X > 0u && uint.IsPow2(value.X)");
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
            StringAssert.Contains(source, "System.Numerics.BitOperations.PopCount(");
        }
    }

    /// <summary>Narrow and 128-bit vectors emit width-correct direct operations without generic wrappers.</summary>
    [TestMethod]
    public void OtherIntegerWidths_UseDirectWidthCorrectBitFunctions()
    {
        var signedByteScalar = VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int8);
        var unsignedShortScalar = VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt16);
        var signedByte = VectorFileEmitter.Emit(new(2, signedByteScalar));
        var unsignedShort = VectorFileEmitter.Emit(new(2, unsignedShortScalar));
        var unsigned128 = VectorFileEmitter.Emit(new(2, VectorCatalog.UInt128));

        StringAssert.Contains(signedByte, "System.Numerics.BitOperations.PopCount((uint)(byte)value.X)");
        StringAssert.Contains(signedByte, "System.Numerics.BitOperations.LeadingZeroCount((uint)(byte)value.X) - 24");
        StringAssert.Contains(signedByte, "value.X == (sbyte)0 ? 8 : System.Numerics.BitOperations.TrailingZeroCount");
        StringAssert.Contains(unsignedShort, "System.Numerics.BitOperations.LeadingZeroCount((uint)value.X) - 16");
        StringAssert.Contains(unsigned128, "(int)UInt128.PopCount(value.X)");
        StringAssert.Contains(unsigned128, "value.X > (UInt128)0 && UInt128.IsPow2(value.X)");
        Assert.IsFalse(signedByte.Contains("ScalarMath.BitCount", StringComparison.Ordinal));
        Assert.IsFalse(unsignedShort.Contains("ScalarMath.LeadingZeroCount", StringComparison.Ordinal));
        Assert.IsFalse(unsigned128.Contains("ScalarMath.IsPowerOfTwo", StringComparison.Ordinal));
    }
}
