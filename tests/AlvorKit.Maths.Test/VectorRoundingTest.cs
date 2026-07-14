namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-backed single-precision vector rounding operations.</summary>
[TestClass]
public sealed class VectorRoundingTest
{
    private static readonly float PositiveQuietNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativeQuietNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));
    private static readonly float PositiveSignalingNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FA12345));
    private static readonly float NegativeSignalingNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFA54321));

    /// <summary>Round preserves scalar result bits for every midpoint mode in every supported float-vector dimension.</summary>
    [TestMethod]
    public void Round_AllMidpointModesMatchScalarBits()
    {
        var vec2 = new Vec2(2.5f, -2.5f);
        var vec3 = new Vec3(1.5f, -1.5f, 1.25f);
        var vec4 = new Vec4(3.5f, -3.5f, 2.75f, -2.75f);

        foreach (var mode in Enum.GetValues<MidpointRounding>())
        {
            AssertSameBits(Vec2.Round(vec2, mode), ScalarRound(vec2, mode));
            AssertSameBits(Vec3.Round(vec3, mode), ScalarRound(vec3, mode));
            AssertSameBits(Vec4.Round(vec4, mode), ScalarRound(vec4, mode));
        }
    }

    /// <summary>All rounding operations preserve scalar bits for signed zeros, infinities, and quiet payload NaNs.</summary>
    [TestMethod]
    public void RoundingOperations_MatchExceptionalScalarBits()
    {
        AssertRoundingOperations(new Vec2(0f, -0f));
        AssertRoundingOperations(new Vec3(float.PositiveInfinity, float.NegativeInfinity, PositiveQuietNaN));
        AssertRoundingOperations(new Vec4(NegativeQuietNaN, -0f, float.PositiveInfinity, float.NegativeInfinity));
    }

    /// <summary>Truncate preserves signaling-NaN sign, signaling bit, and payload in every supported float-vector dimension.</summary>
    [TestMethod]
    public void Truncate_PreservesSignalingNaNBits()
    {
        var vec2 = new Vec2(PositiveSignalingNaN, NegativeSignalingNaN);
        var vec3 = new Vec3(NegativeSignalingNaN, PositiveSignalingNaN, PositiveSignalingNaN);
        var vec4 = new Vec4(PositiveSignalingNaN, NegativeSignalingNaN, NegativeSignalingNaN, PositiveSignalingNaN);

        AssertSameBits(Vec2.Truncate(vec2), ScalarTruncate(vec2));
        AssertSameBits(Vec3.Truncate(vec3), ScalarTruncate(vec3));
        AssertSameBits(Vec4.Truncate(vec4), ScalarTruncate(vec4));
    }

    /// <summary>Vector rounding rejects invalid midpoint modes with the same exception type and parameter name as scalar rounding.</summary>
    [TestMethod]
    public void Round_InvalidMidpointModeMatchesScalarException()
    {
        var mode = (MidpointRounding)int.MaxValue;
        var scalarException = Assert.ThrowsException<ArgumentException>(() => ScalarMath.Round(1f, mode));

        AssertMatchingException(scalarException, Assert.ThrowsException<ArgumentException>(() => Vec2.Round(Vec2.One, mode)));
        AssertMatchingException(scalarException, Assert.ThrowsException<ArgumentException>(() => Vec3.Round(Vec3.One, mode)));
        AssertMatchingException(scalarException, Assert.ThrowsException<ArgumentException>(() => Vec4.Round(Vec4.One, mode)));
    }

    private static void AssertRoundingOperations(Vec2 value)
    {
        AssertSameBits(Vec2.Floor(value), new(ScalarMath.Floor(value.X), ScalarMath.Floor(value.Y)));
        AssertSameBits(Vec2.Ceiling(value), new(ScalarMath.Ceiling(value.X), ScalarMath.Ceiling(value.Y)));
        AssertSameBits(Vec2.Round(value), ScalarRound(value, MidpointRounding.ToEven));
        AssertSameBits(Vec2.Truncate(value), ScalarTruncate(value));
    }

    private static void AssertRoundingOperations(Vec3 value)
    {
        AssertSameBits(Vec3.Floor(value), new(ScalarMath.Floor(value.X), ScalarMath.Floor(value.Y), ScalarMath.Floor(value.Z)));
        AssertSameBits(Vec3.Ceiling(value), new(ScalarMath.Ceiling(value.X), ScalarMath.Ceiling(value.Y), ScalarMath.Ceiling(value.Z)));
        AssertSameBits(Vec3.Round(value), ScalarRound(value, MidpointRounding.ToEven));
        AssertSameBits(Vec3.Truncate(value), ScalarTruncate(value));
    }

    private static void AssertRoundingOperations(Vec4 value)
    {
        AssertSameBits(Vec4.Floor(value),
            new(ScalarMath.Floor(value.X), ScalarMath.Floor(value.Y), ScalarMath.Floor(value.Z), ScalarMath.Floor(value.W)));
        AssertSameBits(Vec4.Ceiling(value),
            new(ScalarMath.Ceiling(value.X), ScalarMath.Ceiling(value.Y), ScalarMath.Ceiling(value.Z), ScalarMath.Ceiling(value.W)));
        AssertSameBits(Vec4.Round(value), ScalarRound(value, MidpointRounding.ToEven));
        AssertSameBits(Vec4.Truncate(value), ScalarTruncate(value));
    }

    private static Vec2 ScalarRound(Vec2 value, MidpointRounding mode) =>
        new(ScalarMath.Round(value.X, mode), ScalarMath.Round(value.Y, mode));

    private static Vec3 ScalarRound(Vec3 value, MidpointRounding mode) =>
        new(ScalarMath.Round(value.X, mode), ScalarMath.Round(value.Y, mode), ScalarMath.Round(value.Z, mode));

    private static Vec4 ScalarRound(Vec4 value, MidpointRounding mode) =>
        new(ScalarMath.Round(value.X, mode), ScalarMath.Round(value.Y, mode), ScalarMath.Round(value.Z, mode), ScalarMath.Round(value.W, mode));

    private static Vec2 ScalarTruncate(Vec2 value) =>
        new(ScalarMath.Truncate(value.X), ScalarMath.Truncate(value.Y));

    private static Vec3 ScalarTruncate(Vec3 value) =>
        new(ScalarMath.Truncate(value.X), ScalarMath.Truncate(value.Y), ScalarMath.Truncate(value.Z));

    private static Vec4 ScalarTruncate(Vec4 value) =>
        new(ScalarMath.Truncate(value.X), ScalarMath.Truncate(value.Y), ScalarMath.Truncate(value.Z), ScalarMath.Truncate(value.W));

    private static void AssertMatchingException(ArgumentException expected, ArgumentException actual)
    {
        Assert.AreEqual(expected.GetType(), actual.GetType());
        Assert.AreEqual(expected.ParamName, actual.ParamName);
    }

    private static void AssertSameBits(Vec2 actual, Vec2 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
    }

    private static void AssertSameBits(Vec3 actual, Vec3 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
        AssertSameBits(actual.Z, expected.Z);
    }

    private static void AssertSameBits(Vec4 actual, Vec4 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
        AssertSameBits(actual.Z, expected.Z);
        AssertSameBits(actual.W, expected.W);
    }

    private static void AssertSameBits(float actual, float expected) =>
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
