using MethodImpl = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
using Marshal = System.Runtime.InteropServices.Marshal;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

namespace AlvorKit.Maths.Test;

/// <summary>Locks quaternion SIMD operations to their selected component order and special-value behavior.</summary>
[TestClass]
public sealed class QuaternionSimdExactnessTest
{
    /// <summary>Float quaternion lane-independent operators and conjugation preserve every component bit.</summary>
    [TestMethod]
    public void Quat_ComponentOperations_PreserveExactBits()
    {
        var left = new Quat(Float(0x80000000), Float(0x7FC12345), float.PositiveInfinity, -3.5f);
        var right = new Quat(Float(0x00000000), 2f, float.NegativeInfinity, Float(0xFFC54321));
        float scalar = Float(0x80000000);

        AssertBits(FloatAdd(left, right), left + right);
        AssertBits(FloatSubtract(left, right), left - right);
        AssertBits(FloatVectorScalarAdd(left, scalar), left + scalar);
        AssertBits(FloatScalarVectorAdd(scalar, left), scalar + left);
        AssertBits(FloatVectorScalarSubtract(left, scalar), left - scalar);
        AssertBits(FloatScalarVectorSubtract(scalar, left), scalar - left);
        AssertBits(FloatNegate(left), -left);
        AssertBits(FloatScale(left, scalar), left * scalar);
        AssertBits(FloatScale(left, scalar), scalar * left);
        AssertBits(FloatDivide(left, scalar), left / scalar);
        AssertBits(FloatScalarDivide(2f, left), 2f / left);
        AssertBits(FloatConjugate(left), Quat.Conjugate(left));
    }

    /// <summary>Double quaternion lane-independent operators and conjugation preserve every component bit.</summary>
    [TestMethod]
    public void Quatd_ComponentOperations_PreserveExactBits()
    {
        var left = new Quatd(Double(0x8000000000000000), Double(0x7FF8123456789ABC), double.PositiveInfinity, -3.5d);
        var right = new Quatd(Double(0x0000000000000000), 2d, double.NegativeInfinity, Double(0xFFF8ABCDEF123456));
        double scalar = Double(0x8000000000000000);

        AssertBits(DoubleAdd(left, right), left + right);
        AssertBits(DoubleSubtract(left, right), left - right);
        AssertBits(DoubleVectorScalarAdd(left, scalar), left + scalar);
        AssertBits(DoubleScalarVectorAdd(scalar, left), scalar + left);
        AssertBits(DoubleVectorScalarSubtract(left, scalar), left - scalar);
        AssertBits(DoubleScalarVectorSubtract(scalar, left), scalar - left);
        AssertBits(DoubleNegate(left), -left);
        AssertBits(DoubleScale(left, scalar), left * scalar);
        AssertBits(DoubleScale(left, scalar), scalar * left);
        AssertBits(DoubleDivide(left, scalar), left / scalar);
        AssertBits(DoubleScalarDivide(2d, left), 2d / left);
        AssertBits(DoubleConjugate(left), Quatd.Conjugate(left));
    }

    /// <summary>Float and double Hamilton products use the packed System.Numerics evaluation algorithm.</summary>
    [TestMethod]
    public void QuaternionHamiltonProducts_UseSelectedFloatAndDoubleSemantics()
    {
        var floatLeft = new Quat(Float(0x80000000), Float(0x7FC12345), 3f, -4f);
        var floatRight = new Quat(5f, -6f, float.PositiveInfinity, Float(0xFFC54321));
        var doubleLeft = new Quatd(Double(0x8000000000000000), Double(0x7FF8123456789ABC), 3d, -4d);
        var doubleRight = new Quatd(5d, -6d, double.PositiveInfinity, Double(0xFFF8ABCDEF123456));

        var systemExpected = (Quat)((System.Numerics.Quaternion)floatLeft * (System.Numerics.Quaternion)floatRight);

        AssertBits(systemExpected, floatLeft * floatRight);
        AssertBits(DoubleHamilton(doubleLeft, doubleRight), doubleLeft * doubleRight);
    }

    /// <summary>Normalize and inverse retain ordered Dot reductions, lane division, and zero-length branches.</summary>
    [TestMethod]
    public void QuaternionNormalizeAndInverse_PreserveExactBitsAndDegenerateBranches()
    {
        var floatValue = new Quat(1.25f, -2.5f, 3.75f, -4.5f);
        var doubleValue = new Quatd(1.25d, -2.5d, 3.75d, -4.5d);
        var floatFallback = new Quat(8f, 7f, 6f, 5f);
        var doubleFallback = new Quatd(8d, 7d, 6d, 5d);

        AssertBits(FloatNormalize(floatValue), Quat.Normalize(floatValue));
        AssertBits(FloatInverse(floatValue), Quat.Invert(floatValue));
        AssertBits(DoubleNormalize(doubleValue), Quatd.Normalize(doubleValue));
        AssertBits(DoubleInverse(doubleValue), Quatd.Invert(doubleValue));
        AssertBits(floatFallback, Quat.Zero.NormalizedOr(floatFallback));
        AssertBits(doubleFallback, Quatd.Zero.NormalizedOr(doubleFallback));
        Assert.IsFalse(Quat.Zero.TryNormalize(out var floatNormalized));
        Assert.IsFalse(Quat.TryInvert(Quat.Zero, out var floatInverse));
        Assert.IsFalse(Quatd.Zero.TryNormalize(out var doubleNormalized));
        Assert.IsFalse(Quatd.TryInvert(Quatd.Zero, out var doubleInverse));
        AssertBits(default, floatNormalized);
        AssertBits(default, floatInverse);
        AssertBits(default, doubleNormalized);
        AssertBits(default, doubleInverse);
    }

    /// <summary>Vector rotation and interpolation keep exact formulas, shortest-arc choice, and near-equal fallback behavior.</summary>
    [TestMethod]
    public void QuaternionCompoundOperations_PreserveExactBitsAndBranches()
    {
        var floatFrom = FloatNormalize(new Quat(1f, -2f, 3f, -4f));
        var floatTo = FloatNormalize(new Quat(-2f, 1f, -4f, 3f));
        var doubleFrom = DoubleNormalize(new Quatd(1d, -2d, 3d, -4d));
        var doubleTo = DoubleNormalize(new Quatd(-2d, 1d, -4d, 3d));
        var floatVector = new Vec3(-0f, 2.5f, -3.75f);
        var doubleVector = new Vec3d(-0d, 2.5d, -3.75d);

        AssertBits(FloatTransform(floatFrom, floatVector), Quat.TransformVector(floatFrom, floatVector));
        AssertBits(DoubleTransform(doubleFrom, doubleVector), Quatd.TransformVector(doubleFrom, doubleVector));
        AssertBits(FloatLerp(floatFrom, floatTo, 0.375f), Quat.Lerp(floatFrom, floatTo, 0.375f));
        AssertBits(DoubleLerp(doubleFrom, doubleTo, 0.375d), Quatd.Lerp(doubleFrom, doubleTo, 0.375d));
        AssertBits(FloatNlerp(floatFrom, floatTo, 0.375f), Quat.Nlerp(floatFrom, floatTo, 0.375f));
        AssertBits(DoubleNlerp(doubleFrom, doubleTo, 0.375d), Quatd.Nlerp(doubleFrom, doubleTo, 0.375d));
        AssertBits(FloatSlerp(floatFrom, floatTo, 0.375f), Quat.Slerp(floatFrom, floatTo, 0.375f));
        AssertBits(DoubleSlerp(doubleFrom, doubleTo, 0.375d), Quatd.Slerp(doubleFrom, doubleTo, 0.375d));
        AssertBits(FloatNlerp(floatFrom, floatFrom, 0.25f), Quat.Slerp(floatFrom, floatFrom, 0.25f));
        AssertBits(DoubleNlerp(doubleFrom, doubleFrom, 0.25d), Quatd.Slerp(doubleFrom, doubleFrom, 0.25d));
    }

    /// <summary>Quaternion and System.Numerics conversions preserve layout, order, signed zero, and NaN payloads.</summary>
    [TestMethod]
    public void Quat_SystemNumericsInterop_PreservesExactBits()
    {
        var value = new Quat(Float(0x80000000), Float(0x7FC12345), float.PositiveInfinity, Float(0xFFC54321));
        System.Numerics.Quaternion system = value;
        Quat roundTrip = system;

        Assert.AreEqual(16, Unsafe.SizeOf<Quat>());
        Assert.AreEqual(Unsafe.SizeOf<Quat>(), Unsafe.SizeOf<System.Numerics.Quaternion>());
        Assert.AreEqual(0, Marshal.OffsetOf<Quat>(nameof(Quat.X)).ToInt32());
        Assert.AreEqual(4, Marshal.OffsetOf<Quat>(nameof(Quat.Y)).ToInt32());
        Assert.AreEqual(8, Marshal.OffsetOf<Quat>(nameof(Quat.Z)).ToInt32());
        Assert.AreEqual(12, Marshal.OffsetOf<Quat>(nameof(Quat.W)).ToInt32());
        Assert.AreEqual(BitConverter.SingleToInt32Bits(value.X), BitConverter.SingleToInt32Bits(system.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(value.Y), BitConverter.SingleToInt32Bits(system.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(value.Z), BitConverter.SingleToInt32Bits(system.Z));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(value.W), BitConverter.SingleToInt32Bits(system.W));
        AssertBits(value, roundTrip);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatAdd(Quat left, Quat right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatSubtract(Quat left, Quat right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatVectorScalarAdd(Quat left, float right) => new(left.X + right, left.Y + right, left.Z + right, left.W + right);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatScalarVectorAdd(float left, Quat right) => new(left + right.X, left + right.Y, left + right.Z, left + right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatVectorScalarSubtract(Quat left, float right) => new(left.X - right, left.Y - right, left.Z - right, left.W - right);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatScalarVectorSubtract(float left, Quat right) => new(left - right.X, left - right.Y, left - right.Z, left - right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatNegate(Quat value) => new(-value.X, -value.Y, -value.Z, -value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatScale(Quat value, float scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar, value.W * scalar);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatDivide(Quat value, float scalar) => new(value.X / scalar, value.Y / scalar, value.Z / scalar, value.W / scalar);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatScalarDivide(float scalar, Quat value) => new(scalar / value.X, scalar / value.Y, scalar / value.Z, scalar / value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatConjugate(Quat value) => new(-value.X, -value.Y, -value.Z, value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatNormalize(Quat value) => FloatDivide(value, ScalarMath.Sqrt(FloatDot(value, value)));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quat FloatInverse(Quat value) => FloatDivide(FloatConjugate(value), FloatDot(value, value));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float FloatDot(Quat left, Quat right) =>
        (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);

    private static Vec3 FloatTransform(Quat rotation, Vec3 vector)
    {
        var uv = FloatCross(rotation.Vector, vector);
        var uuv = FloatCross(rotation.Vector, uv);
        return FloatVectorAdd(vector, FloatVectorScale(FloatVectorAdd(FloatVectorScale(uv, rotation.W), uuv), 2f));
    }

    private static Quat FloatLerp(Quat from, Quat to, float amount) =>
        FloatAdd(FloatScale(from, 1f - amount), FloatScale(to, amount));

    private static Quat FloatNlerp(Quat from, Quat to, float amount) =>
        FloatNormalize(FloatLerp(from, FloatDot(from, to) < 0f ? FloatNegate(to) : to, amount));

    private static Quat FloatSlerp(Quat from, Quat to, float amount)
    {
        var end = to;
        var cosTheta = FloatDot(from, to);
        if (cosTheta < 0f)
        {
            end = FloatNegate(to);
            cosTheta = -cosTheta;
        }

        if (cosTheta > 1f - 1e-6f)
            return FloatNlerp(from, end, amount);

        var angle = ScalarMath.Acos(cosTheta);
        return FloatDivide(
            FloatAdd(FloatScale(from, ScalarMath.Sin((1f - amount) * angle)), FloatScale(end, ScalarMath.Sin(amount * angle))),
            ScalarMath.Sin(angle));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleAdd(Quatd left, Quatd right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleSubtract(Quatd left, Quatd right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleVectorScalarAdd(Quatd left, double right) => new(left.X + right, left.Y + right, left.Z + right, left.W + right);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleScalarVectorAdd(double left, Quatd right) => new(left + right.X, left + right.Y, left + right.Z, left + right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleVectorScalarSubtract(Quatd left, double right) => new(left.X - right, left.Y - right, left.Z - right, left.W - right);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleScalarVectorSubtract(double left, Quatd right) => new(left - right.X, left - right.Y, left - right.Z, left - right.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleNegate(Quatd value) => new(-value.X, -value.Y, -value.Z, -value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleScale(Quatd value, double scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar, value.W * scalar);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleDivide(Quatd value, double scalar) => new(value.X / scalar, value.Y / scalar, value.Z / scalar, value.W / scalar);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleScalarDivide(double scalar, Quatd value) => new(scalar / value.X, scalar / value.Y, scalar / value.Z, scalar / value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleConjugate(Quatd value) => new(-value.X, -value.Y, -value.Z, value.W);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleHamilton(Quatd left, Quatd right)
    {
        var rightPacked = Unsafe.BitCast<Quatd, System.Runtime.Intrinsics.Vector256<double>>(right);
        var result = rightPacked * left.W;
        result = System.Runtime.Intrinsics.Vector256.MultiplyAddEstimate(
            System.Runtime.Intrinsics.Vector256.Shuffle(
                rightPacked,
                System.Runtime.Intrinsics.Vector256.Create(3L, 2L, 1L, 0L)) * left.X,
            System.Runtime.Intrinsics.Vector256.Create(+1d, -1d, +1d, -1d),
            result);
        result = System.Runtime.Intrinsics.Vector256.MultiplyAddEstimate(
            System.Runtime.Intrinsics.Vector256.Shuffle(
                rightPacked,
                System.Runtime.Intrinsics.Vector256.Create(2L, 3L, 0L, 1L)) * left.Y,
            System.Runtime.Intrinsics.Vector256.Create(+1d, +1d, -1d, -1d),
            result);
        result = System.Runtime.Intrinsics.Vector256.MultiplyAddEstimate(
            System.Runtime.Intrinsics.Vector256.Shuffle(
                rightPacked,
                System.Runtime.Intrinsics.Vector256.Create(1L, 0L, 3L, 2L)) * left.Z,
            System.Runtime.Intrinsics.Vector256.Create(-1d, +1d, +1d, -1d),
            result);
        return Unsafe.BitCast<System.Runtime.Intrinsics.Vector256<double>, Quatd>(result);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleNormalize(Quatd value) => DoubleDivide(value, ScalarMath.Sqrt(DoubleDot(value, value)));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Quatd DoubleInverse(Quatd value) => DoubleDivide(DoubleConjugate(value), DoubleDot(value, value));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double DoubleDot(Quatd left, Quatd right) =>
        (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);

    private static Vec3d DoubleTransform(Quatd rotation, Vec3d vector)
    {
        var uv = DoubleCross(rotation.Vector, vector);
        var uuv = DoubleCross(rotation.Vector, uv);
        return DoubleVectorAdd(vector, DoubleVectorScale(DoubleVectorAdd(DoubleVectorScale(uv, rotation.W), uuv), 2d));
    }

    private static Quatd DoubleLerp(Quatd from, Quatd to, double amount) =>
        DoubleAdd(DoubleScale(from, 1d - amount), DoubleScale(to, amount));

    private static Quatd DoubleNlerp(Quatd from, Quatd to, double amount) =>
        DoubleNormalize(DoubleLerp(from, DoubleDot(from, to) < 0d ? DoubleNegate(to) : to, amount));

    private static Quatd DoubleSlerp(Quatd from, Quatd to, double amount)
    {
        var end = to;
        var cosTheta = DoubleDot(from, to);
        if (cosTheta < 0d)
        {
            end = DoubleNegate(to);
            cosTheta = -cosTheta;
        }

        if (cosTheta > 1d - 1e-12d)
            return DoubleNlerp(from, end, amount);

        var angle = ScalarMath.Acos(cosTheta);
        return DoubleDivide(
            DoubleAdd(DoubleScale(from, ScalarMath.Sin((1d - amount) * angle)), DoubleScale(end, ScalarMath.Sin(amount * angle))),
            ScalarMath.Sin(angle));
    }

    private static Vec3 FloatCross(Vec3 left, Vec3 right) =>
        new((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X));

    private static Vec3 FloatVectorAdd(Vec3 left, Vec3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    private static Vec3 FloatVectorScale(Vec3 value, float scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    private static Vec3d DoubleCross(Vec3d left, Vec3d right) =>
        new((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X));

    private static Vec3d DoubleVectorAdd(Vec3d left, Vec3d right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    private static Vec3d DoubleVectorScale(Vec3d value, double scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    private static float Float(uint bits) => BitConverter.Int32BitsToSingle(unchecked((int)bits));

    private static double Double(ulong bits) => BitConverter.Int64BitsToDouble(unchecked((long)bits));

    private static void AssertBits(Quat expected, Quat actual)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.W), BitConverter.SingleToInt32Bits(actual.W));
    }

    private static void AssertBits(Quatd expected, Quatd actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Z), BitConverter.DoubleToInt64Bits(actual.Z));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.W), BitConverter.DoubleToInt64Bits(actual.W));
    }

    private static void AssertBits(Vec3 expected, Vec3 actual)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
    }

    private static void AssertBits(Vec3d expected, Vec3d actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Z), BitConverter.DoubleToInt64Bits(actual.Z));
    }
}
