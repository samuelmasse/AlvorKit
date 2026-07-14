namespace AlvorKit.Maths.Test;

/// <summary>Tests exact SIMD variable-count Int32 shift semantics.</summary>
[TestClass]
public sealed class VectorVariableShiftSimdTest
{
    /// <summary>Signed shifts match scalar C# behavior for negative and oversized per-lane counts.</summary>
    [TestMethod]
    public void Vec4iVariableShifts_MatchScalarComponents()
    {
        Vec4i value = (int.MinValue, -1, 0x4000_0000, int.MaxValue);
        Vec4i count = (-65, -1, 32, 65);

        Assert.AreEqual(new(value.X << count.X, value.Y << count.Y, value.Z << count.Z, value.W << count.W), value << count);
        Assert.AreEqual(new(value.X >> count.X, value.Y >> count.Y, value.Z >> count.Z, value.W >> count.W), value >> count);
        Assert.AreEqual(new(value.X >>> count.X, value.Y >>> count.Y, value.Z >>> count.Z, value.W >>> count.W), value >>> count);
    }

    /// <summary>Unsigned shifts match scalar C# behavior for negative and oversized per-lane counts.</summary>
    [TestMethod]
    public void Vec4uVariableShifts_MatchScalarComponents()
    {
        Vec4u value = (uint.MinValue, 1U, 0x8000_0000U, uint.MaxValue);
        Vec4i count = (-65, -1, 32, 65);

        Assert.AreEqual(new(value.X << count.X, value.Y << count.Y, value.Z << count.Z, value.W << count.W), value << count);
        Assert.AreEqual(new(value.X >> count.X, value.Y >> count.Y, value.Z >> count.Z, value.W >> count.W), value >> count);
        Assert.AreEqual(new(value.X >>> count.X, value.Y >>> count.Y, value.Z >>> count.Z, value.W >>> count.W), value >>> count);
    }
}
