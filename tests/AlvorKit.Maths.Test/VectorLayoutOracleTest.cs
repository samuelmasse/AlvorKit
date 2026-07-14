using MemoryMarshal = System.Runtime.InteropServices.MemoryMarshal;
using Marshal = System.Runtime.InteropServices.Marshal;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

namespace AlvorKit.Maths.Test;

/// <summary>Protects the public unmanaged layout used by vector SIMD implementations.</summary>
[TestClass]
public sealed class VectorLayoutOracleTest
{
    private const byte LeadingSentinel = 0xA5;
    private const byte TrailingSentinel = 0x5A;
    private const int SentinelSize = 16;

    /// <summary>Every numeric vector is densely laid out in X, Y, Z, W order at its scalar-width stride.</summary>
    [TestMethod]
    public void NumericVectors_AreDenseAndMatchPublishedSizes()
    {
        AssertNumericFamily<Vec2, Vec3, Vec4, float>();
        AssertNumericFamily<Vec2d, Vec3d, Vec4d, double>();
        AssertNumericFamily<Vec2h, Vec3h, Vec4h, Half>();
        AssertNumericFamily<Vec2i8, Vec3i8, Vec4i8, sbyte>();
        AssertNumericFamily<Vec2u8, Vec3u8, Vec4u8, byte>();
        AssertNumericFamily<Vec2i16, Vec3i16, Vec4i16, short>();
        AssertNumericFamily<Vec2u16, Vec3u16, Vec4u16, ushort>();
        AssertNumericFamily<Vec2i, Vec3i, Vec4i, int>();
        AssertNumericFamily<Vec2u, Vec3u, Vec4u, uint>();
        AssertNumericFamily<Vec2i64, Vec3i64, Vec4i64, long>();
        AssertNumericFamily<Vec2u64, Vec3u64, Vec4u64, ulong>();
        AssertNumericFamily<Vec2i128, Vec3i128, Vec4i128, Int128>();
        AssertNumericFamily<Vec2u128, Vec3u128, Vec4u128, UInt128>();
    }

    /// <summary>Boolean vectors retain their explicit four-byte component stride and published sizes.</summary>
    [TestMethod]
    public void BooleanVectors_UseExplicitFourByteComponentStride()
    {
        AssertBooleanLayout<Vec2b>(2);
        AssertBooleanLayout<Vec3b>(3);
        AssertBooleanLayout<Vec4b>(4);
    }

    /// <summary>Natural-register candidates round-trip through raw bytes without changing component order or bits.</summary>
    [TestMethod]
    public void NaturalRegisterCandidates_RawBytesRoundTripExactly()
    {
        AssertRawRoundTrip(
            new Vec4i(unchecked((int)0x01020304), -1, int.MinValue, int.MaxValue),
            (ReadOnlySpan<int>)[unchecked((int)0x01020304), -1, int.MinValue, int.MaxValue]);
        AssertRawRoundTrip(
            new Vec4u(0x01020304u, uint.MaxValue, 0x80000000u, 0x7FFFFFFFu),
            (ReadOnlySpan<uint>)[0x01020304u, uint.MaxValue, 0x80000000u, 0x7FFFFFFFu]);

        var positiveNaN = BitConverter.Int64BitsToDouble(unchecked((long)0x7FF8123456789ABC));
        var negativeNaN = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8ABCDEF012345));
        AssertRawRoundTrip(new Vec2d(-0d, positiveNaN), (ReadOnlySpan<double>)[-0d, positiveNaN]);
        AssertRawRoundTrip(
            new Vec4d(-0d, positiveNaN, negativeNaN, double.PositiveInfinity),
            (ReadOnlySpan<double>)[-0d, positiveNaN, negativeNaN, double.PositiveInfinity]);
    }

    /// <summary>Partial-register vector payloads do not overlap leading or trailing sentinel storage.</summary>
    [TestMethod]
    public void PartialRegisterCandidates_RawRoundTripsPreserveTailSentinels()
    {
        AssertGuardedRawRoundTrip(new Vec3i(unchecked((int)0x89ABCDEF), 0x12345678, -1));
        AssertGuardedRawRoundTrip(new Vec3u(0x89ABCDEFu, 0x12345678u, uint.MaxValue));
        AssertGuardedRawRoundTrip(new Vec3d(-0d, double.Epsilon, double.NegativeInfinity));
    }

    private static void AssertNumericFamily<TVec2, TVec3, TVec4, TScalar>()
        where TVec2 : unmanaged, IVec<TVec2, TScalar>
        where TVec3 : unmanaged, IVec<TVec3, TScalar>
        where TVec4 : unmanaged, IVec<TVec4, TScalar>
        where TScalar : unmanaged
    {
        var stride = Unsafe.SizeOf<TScalar>();
        AssertLayout<TVec2, TScalar>(2, stride);
        AssertLayout<TVec3, TScalar>(3, stride);
        AssertLayout<TVec4, TScalar>(4, stride);
    }

    private static void AssertLayout<TVector, TScalar>(int componentCount, int componentStride)
        where TVector : unmanaged, IVec<TVector, TScalar>
        where TScalar : unmanaged
    {
        var expectedSize = componentCount * componentStride;
        Assert.AreEqual(componentCount, TVector.ComponentCount, $"{typeof(TVector).Name}.ComponentCount");
        Assert.AreEqual(expectedSize, TVector.SizeInBytes, $"{typeof(TVector).Name}.SizeInBytes");
        Assert.AreEqual(expectedSize, Unsafe.SizeOf<TVector>(), $"Unsafe.SizeOf<{typeof(TVector).Name}>()");

        TVector value = default;
        ref var firstByte = ref Unsafe.As<TVector, byte>(ref value);
        string[] coordinateNames = ["X", "Y", "Z", "W"];
        string[] colorNames = ["R", "G", "B", "A"];
        string[] textureNames = ["S", "T", "P", "Q"];
        for (var index = 0; index < componentCount; index++)
        {
            var expectedOffset = index * componentStride;
            ref var component = ref TVector.ComponentRef(ref value, index);
            ref var componentByte = ref Unsafe.As<TScalar, byte>(ref component);
            var actualOffset = (long)Unsafe.ByteOffset(ref firstByte, ref componentByte);

            Assert.AreEqual(expectedOffset, actualOffset, $"{typeof(TVector).Name} component {index}");
            AssertFieldOffset<TVector>(coordinateNames[index], expectedOffset);
            AssertFieldOffset<TVector>(colorNames[index], expectedOffset);
            AssertFieldOffset<TVector>(textureNames[index], expectedOffset);
        }

        var pair = new TVector[2];
        ref var first = ref pair[0];
        ref var second = ref pair[1];
        Assert.AreEqual(expectedSize, (long)Unsafe.ByteOffset(ref first, ref second), $"{typeof(TVector).Name} array stride");
    }

    private static void AssertBooleanLayout<TVector>(int componentCount)
        where TVector : unmanaged, IVec<TVector, bool>
    {
        const int componentStride = sizeof(int);
        var publishedSize = componentCount * componentStride;
        var managedSize = ((componentCount - 1) * componentStride) + Unsafe.SizeOf<bool>();
        Assert.AreEqual(componentCount, TVector.ComponentCount, $"{typeof(TVector).Name}.ComponentCount");
        Assert.AreEqual(publishedSize, TVector.SizeInBytes, $"{typeof(TVector).Name}.SizeInBytes");
        Assert.AreEqual(publishedSize, Marshal.SizeOf<TVector>(), $"Marshal.SizeOf<{typeof(TVector).Name}>()");
        Assert.AreEqual(managedSize, Unsafe.SizeOf<TVector>(), $"Unsafe.SizeOf<{typeof(TVector).Name}>()");

        TVector value = default;
        ref var firstByte = ref Unsafe.As<TVector, byte>(ref value);
        string[] coordinateNames = ["X", "Y", "Z", "W"];
        string[] colorNames = ["R", "G", "B", "A"];
        string[] textureNames = ["S", "T", "P", "Q"];
        for (var index = 0; index < componentCount; index++)
        {
            var expectedOffset = index * componentStride;
            ref var component = ref TVector.ComponentRef(ref value, index);
            ref var componentByte = ref Unsafe.As<bool, byte>(ref component);
            var actualOffset = (long)Unsafe.ByteOffset(ref firstByte, ref componentByte);

            Assert.AreEqual(expectedOffset, actualOffset, $"{typeof(TVector).Name} component {index}");
            AssertFieldOffset<TVector>(coordinateNames[index], expectedOffset);
            AssertFieldOffset<TVector>(colorNames[index], expectedOffset);
            AssertFieldOffset<TVector>(textureNames[index], expectedOffset);
        }

        var pair = new TVector[2];
        ref var first = ref pair[0];
        ref var second = ref pair[1];
        Assert.AreEqual(managedSize, (long)Unsafe.ByteOffset(ref first, ref second), $"{typeof(TVector).Name} managed array stride");
    }

    private static void AssertFieldOffset<TVector>(string fieldName, int expectedOffset)
        where TVector : unmanaged
    {
        Assert.AreEqual(expectedOffset, Marshal.OffsetOf<TVector>(fieldName).ToInt32(), $"{typeof(TVector).Name}.{fieldName}");
    }

    private static void AssertRawRoundTrip<TVector, TScalar>(TVector value, ReadOnlySpan<TScalar> expectedComponents)
        where TVector : unmanaged
        where TScalar : unmanaged
    {
        Span<byte> bytes = stackalloc byte[Unsafe.SizeOf<TVector>()];
        MemoryMarshal.Write(bytes, in value);
        Assert.IsTrue(bytes.SequenceEqual(MemoryMarshal.AsBytes(expectedComponents)), $"{typeof(TVector).Name} component bytes");

        var roundTrip = MemoryMarshal.Read<TVector>(bytes);
        Span<byte> roundTripBytes = stackalloc byte[Unsafe.SizeOf<TVector>()];
        MemoryMarshal.Write(roundTripBytes, in roundTrip);
        Assert.IsTrue(bytes.SequenceEqual(roundTripBytes), $"{typeof(TVector).Name} round trip");
    }

    private static void AssertGuardedRawRoundTrip<TVector>(TVector value)
        where TVector : unmanaged
    {
        var valueSize = Unsafe.SizeOf<TVector>();
        Span<byte> guarded = stackalloc byte[(SentinelSize * 2) + valueSize];
        guarded[..SentinelSize].Fill(LeadingSentinel);
        guarded[(SentinelSize + valueSize)..].Fill(TrailingSentinel);

        var payload = guarded.Slice(SentinelSize, valueSize);
        MemoryMarshal.Write(payload, in value);
        var roundTrip = MemoryMarshal.Read<TVector>(payload);

        Assert.IsTrue(guarded[..SentinelSize].IndexOfAnyExcept(LeadingSentinel) < 0, $"{typeof(TVector).Name} leading sentinel");
        Assert.IsTrue(guarded[(SentinelSize + valueSize)..].IndexOfAnyExcept(TrailingSentinel) < 0, $"{typeof(TVector).Name} trailing sentinel");

        Span<byte> roundTripBytes = stackalloc byte[valueSize];
        MemoryMarshal.Write(roundTripBytes, in roundTrip);
        Assert.IsTrue(payload.SequenceEqual(roundTripBytes), $"{typeof(TVector).Name} guarded round trip");
    }
}
