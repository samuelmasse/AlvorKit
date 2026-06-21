namespace AlvorKit.Maths.Test;

/// <summary>Tests single-precision three-component vector behavior.</summary>
[TestClass]
public sealed class Vec3Test
{
    /// <summary>Constructors, aliases, indexing, and deconstruction expose the same component storage.</summary>
    [TestMethod]
    public void CoreMembers_Work()
    {
        var value = new Vec3(1f, 2f, 3f)
        {
            R = 4f,
            T = 5f,
            [2] = 6f
        };
        value[0] = 4f;
        value[1] = 5f;
        var (x, y, z) = value;

        Assert.AreEqual(new Vec3(4f, 5f, 6f), value);
        Assert.AreEqual(new Vec3(2f), Vec3.One + 1f);
        Assert.AreEqual(4f, value.S);
        Assert.AreEqual(5f, value.G);
        Assert.AreEqual(6f, value.B);
        Assert.AreEqual(6f, value.P);
        Assert.AreEqual(4f, value[0]);
        Assert.AreEqual(5f, value[1]);
        Assert.AreEqual(6f, value[2]);
        Assert.AreEqual((4f, 5f, 6f), (x, y, z));
        Assert.AreEqual(Vec3.ComponentCount, 3);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => value[3] = 0f);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = value[3]);
    }

    /// <summary>Tuple and cross-vector conversions follow widening, truncating, and explicit mask rules.</summary>
    [TestMethod]
    public void Conversions_Work()
    {
        Vec3 fromTuple = (1f, 2f, 3f);
        (float X, float Y, float Z) tuple = fromTuple;
        Vec3 widened = new Vec3i(7, 8, 9);
        var truncated = (Vec3i)new Vec3(1.9f, -2.9f, 3.1f);
        var fromMask = (Vec3)new Vec3b(true, false, true);
        var fromInverseMask = (Vec3)new Vec3b(false, true, false);
        var toMask = (Vec3b)new Vec3(0f, -1f, float.NaN);
        var toInverseMask = (Vec3b)new Vec3(1f, 0f, 0f);
        System.Numerics.Vector3 system = fromTuple;
        Vec3 fromSystem = system;

        Assert.AreEqual((1f, 2f, 3f), tuple);
        Assert.AreEqual(new Vec3(7f, 8f, 9f), widened);
        Assert.AreEqual(new Vec3i(1, -2, 3), truncated);
        Assert.AreEqual(new Vec3(1f, 0f, 1f), fromMask);
        Assert.AreEqual(new Vec3(0f, 1f, 0f), fromInverseMask);
        Assert.AreEqual(new Vec3b(false, true, true), toMask);
        Assert.AreEqual(new Vec3b(true, false, false), toInverseMask);
        Assert.AreEqual(new System.Numerics.Vector3(1f, 2f, 3f), system);
        Assert.AreEqual(fromTuple, fromSystem);
    }

    /// <summary>Arithmetic operators support scalar and component-wise vector operations.</summary>
    [TestMethod]
    public void ArithmeticOperators_Work()
    {
        Vec3 left = (6f, 8f, 10f);
        Vec3 right = (2f, 4f, 5f);
        Assert.AreEqual(new Vec3(8f, 12f, 15f), left + right);
        Assert.AreEqual(new Vec3(4f, 4f, 5f), left - right);
        Assert.AreEqual(new Vec3(12f, 32f, 50f), left * right);
        Assert.AreEqual(new Vec3(3f, 2f, 2f), left / right);
        Assert.AreEqual(new Vec3(7f, 9f, 11f), left + 1f);
        Assert.AreEqual(new Vec3(7f, 9f, 11f), 1f + left);
        Assert.AreEqual(new Vec3(5f, 7f, 9f), left - 1f);
        Assert.AreEqual(new Vec3(4f, 2f, 0f), 10f - left);
        Assert.AreEqual(new Vec3(12f, 16f, 20f), 2f * left);
        Assert.AreEqual(new Vec3(12f, 8f, 6f), 24f / new Vec3(2f, 3f, 4f));
        Assert.AreEqual(new Vec3(-6f, -8f, -10f), -left);
        Assert.AreEqual(left, +left);
        Assert.AreEqual(new Vec3(7f, 9f, 11f), ++left);
        Assert.AreEqual(new Vec3(6f, 8f, 10f), --left);
        Assert.IsTrue(left == (6f, 8f, 10f));
        Assert.IsTrue(left != right);
    }

    /// <summary>Geometric helpers return expected dot, cross, length, interpolation, and clamp results.</summary>
    [TestMethod]
    public void GeometryHelpers_Work()
    {
        Vec3 value = (3f, 4f, 0f);
        Assert.AreEqual(25f, value.LengthSquared);
        Assert.AreEqual(5f, value.Length);
        Assert.AreEqual(new Vec3(0.6f, 0.8f, 0f), value.Normalized);
        Assert.AreEqual(32f, Vec3.Dot(new Vec3(1f, 2f, 3f), new Vec3(4f, 5f, 6f)));
        Assert.AreEqual(Vec3.UnitZ, Vec3.Cross(Vec3.UnitX, Vec3.UnitY));
        Assert.AreEqual(new Vec3(5f, 5f, 5f), Vec3.Lerp(Vec3.Zero, new Vec3(10f), 0.5f));
        Assert.AreEqual(new Vec3(5f, 0f, 15f), Vec3.Lerp(Vec3.Zero, new Vec3(10f), new Vec3(0.5f, 0f, 1.5f)));
        Assert.AreEqual(new Vec3(3f, 2.5f, 3f), Vec3.Barycentric(new Vec3(1f), new Vec3(5f, 1f, 1f), new Vec3(1f, 7f, 9f), 0.5f, 0.25f));
        Assert.AreEqual(new Vec3(1f, 1f, 0f), Vec3.Reflect(new Vec3(1f, -1f, 0f), Vec3.UnitY));
        Assert.AreEqual(Vec3.UnitY, Vec3.FaceForward(Vec3.UnitY, -Vec3.UnitY, Vec3.UnitY));
        Assert.AreEqual(-Vec3.UnitY, Vec3.FaceForward(Vec3.UnitY, Vec3.UnitY, Vec3.UnitY));
        Assert.AreEqual(-Vec3.UnitY, Vec3.Refract(-Vec3.UnitY, Vec3.UnitY, 1f));
        Assert.AreEqual(new Vec3(2f, 3f, 4f), Vec3.Clamp(new Vec3(1f, 3f, 9f), new Vec3(2f), new Vec3(4f)));
        Assert.AreEqual(25f, Vec3.DistanceSquared(Vec3.Zero, value));
        Assert.AreEqual(5f, Vec3.Distance(Vec3.Zero, value));
        Assert.AreEqual(new Vec3(1f, 2f, 3f), Vec3.Abs(new Vec3(-1f, -2f, 3f)));
        Assert.AreEqual(new Vec3(0.6f, 0.8f, 0f), value.NormalizedOrZero);
        Assert.AreEqual(new Vec3(0.6f, 0.8f, 0f), Vec3.Normalize(value));
        Assert.AreEqual(Vec3.UnitX, Vec3.Zero.NormalizedOr(Vec3.UnitX));
        Assert.IsTrue(value.TryNormalize(out var normalized));
        Assert.AreEqual(new Vec3(0.6f, 0.8f, 0f), normalized);
        Assert.IsFalse(Vec3.Zero.TryNormalize(out var zeroNormalized));
        Assert.AreEqual(Vec3.Zero, zeroNormalized);
    }

    /// <summary>Common floating-point helpers apply component-wise and keep GLSL-style modulo explicit.</summary>
    [TestMethod]
    public void CommonHelpers_Work()
    {
        Vec3 value = (1.25f, -1.25f, 2.5f);
        var angles = Vec3.Atan2(new Vec3(0f, 1f, 1f), new Vec3(1f, 1f, 0f));

        Assert.AreEqual(new Vec3(1f, -2f, 2f), Vec3.Floor(value));
        Assert.AreEqual(new Vec3(2f, -1f, 3f), Vec3.Ceiling(value));
        Assert.AreEqual(new Vec3(1f, -1f, 2f), Vec3.Round(value));
        Assert.AreEqual(new Vec3(1f, -1f, 3f), Vec3.Round(value, MidpointRounding.AwayFromZero));
        Assert.AreEqual(new Vec3(1f, -1f, 2f), Vec3.Truncate(value));
        Assert.AreEqual(new Vec3(0.25f, 0.75f, 0.5f), Vec3.FractionalPart(value));
        Assert.AreEqual(new Vec3(2f, 2f, 3f), Vec3.Modulo(new Vec3(-1f, 5f, 7f), new Vec3(3f, 3f, 4f)));
        Assert.AreEqual(new Vec3(2f, 2f, 1f), Vec3.Modulo(new Vec3(-1f, 5f, 7f), 3f));
        Assert.AreEqual(new Vec3(2f, 2f, 3f), Vec3.Mod(new Vec3(-1f, 5f, 7f), new Vec3(3f, 3f, 4f)));
        Assert.AreEqual(new Vec3(2f, 2f, 1f), Vec3.Mod(new Vec3(-1f, 5f, 7f), 3f));
        Assert.AreEqual(new Vec3(0f, 0.5f, 1f), Vec3.Saturate(new Vec3(-1f, 0.5f, 2f)));
        Assert.AreEqual(new Vec3(2f, 3f, 4f), Vec3.Sqrt(new Vec3(4f, 9f, 16f)));
        Assert.AreEqual(new Vec3(0.5f, 1f / 3f, 0.25f), Vec3.InverseSqrt(new Vec3(4f, 9f, 16f)));
        Assert.AreEqual(new Vec3(10f), Vec3.FusedMultiplyAdd(new Vec3(2f), new Vec3(3f), new Vec3(4f)));
        Assert.AreEqual(Vec3.Zero, Vec3.Sin(Vec3.Zero));
        Assert.AreEqual(0f, angles.X, 0.000001f);
        Assert.AreEqual(MathF.PI / 4f, angles.Y, 0.000001f);
        Assert.AreEqual(MathF.PI / 2f, angles.Z, 0.000001f);
        Assert.AreEqual(new Vec3(0f, 1f, 1f), Vec3.Step(0.5f, new Vec3(0.25f, 0.5f, 0.75f)));
        Assert.AreEqual(new Vec3(0f, 0.5f, 1f), Vec3.SmoothStep(0f, 1f, new Vec3(0f, 0.5f, 1f)));
        Assert.AreEqual(new Vec3(2f, 3f, 4f), Vec3.Clamp(new Vec3(1f, 3f, 9f), 2f, 4f));
    }

    /// <summary>Equality, hashing, and tuple-style formatting expose stable value semantics.</summary>
    [TestMethod]
    public void ValueSemantics_Work()
    {
        Vec3 value = (1f, 2f, 3f);
        Assert.IsTrue(value.Equals((object)new Vec3(1f, 2f, 3f)));
        Assert.IsFalse(value.Equals(new object()));
        Assert.AreEqual(value.GetHashCode(), new Vec3(1f, 2f, 3f).GetHashCode());
        Assert.AreEqual("(1.0, 2.0, 3.0)", value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>Comparison helpers return component-wise Boolean masks.</summary>
    [TestMethod]
    public void ComparisonHelpers_ReturnMasks()
    {
        Vec3 left = (1f, 3f, 5f);
        Vec3 right = (2f, 3f, 4f);
        Assert.AreEqual(new Vec3b(true, false, false), Vec3.LessThan(left, right));
        Assert.AreEqual(new Vec3b(true, true, false), Vec3.LessThanOrEqual(left, right));
        Assert.AreEqual(new Vec3b(false, false, true), Vec3.GreaterThan(left, right));
        Assert.AreEqual(new Vec3b(false, true, true), Vec3.GreaterThanOrEqual(left, right));
        Assert.AreEqual(new Vec3b(true, false, false), left < right);
        Assert.AreEqual(new Vec3b(true, true, false), left <= right);
        Assert.AreEqual(new Vec3b(false, false, true), left > right);
        Assert.AreEqual(new Vec3b(false, true, true), left >= right);
        Assert.AreEqual(new Vec3b(false, true, false), Vec3.Equal(left, right));
        Assert.AreEqual(new Vec3b(true, false, true), Vec3.NotEqual(left, right));
        Assert.AreEqual(new Vec3b(true, false, false), Vec3.IsNaN(new Vec3(float.NaN, 1f, float.PositiveInfinity)));
        Assert.AreEqual(new Vec3b(false, false, true), Vec3.IsInfinity(new Vec3(float.NaN, 1f, float.PositiveInfinity)));
        Assert.AreEqual(new Vec3b(false, true, false), Vec3.IsFinite(new Vec3(float.NaN, 1f, float.PositiveInfinity)));
    }

    /// <summary>Integer rounding helpers make non-truncating conversions explicit.</summary>
    [TestMethod]
    public void IntegerRoundingHelpers_Work()
    {
        Vec3 value = (1.2f, -1.7f, 2.5f);
        Assert.AreEqual(new Vec3i(1, -1, 2), value.TruncateToVec3i());
        Assert.AreEqual(new Vec3i(1, -2, 2), value.FloorToVec3i());
        Assert.AreEqual(new Vec3i(2, -1, 3), value.CeilingToVec3i());
        Assert.AreEqual(new Vec3i(1, -2, 2), value.RoundToVec3i());
    }
}
