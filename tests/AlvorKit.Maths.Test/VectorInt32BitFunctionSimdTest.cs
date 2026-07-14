namespace AlvorKit.Maths.Test;

/// <summary>Tests exact semantics of retained Vec4 Int32 packed bit functions.</summary>
[TestClass]
public sealed class VectorInt32BitFunctionSimdTest
{
    /// <summary>Signed bit functions match their scalar component definitions.</summary>
    [TestMethod]
    public void SignedBitFunctions_MatchScalarResults()
    {
        foreach (var value in new[]
                 {
                     new Vec4i(0, -1, int.MinValue, int.MaxValue),
                     new Vec4i(1, 2, 4, 8),
                     new Vec4i(31, 32, 63, 64),
                     new Vec4i(unchecked((int)0xAAAAAAAA), 0x55555555, -17, 17),
                 })
        {
            Assert.AreEqual(Map(value, ScalarMath.BitCount), Vec4i.BitCount(value));
            Assert.AreEqual(Map(value, ScalarMath.LeadingZeroCount), Vec4i.LeadingZeroCount(value));
            Assert.AreEqual(Map(value, ScalarMath.TrailingZeroCount), Vec4i.TrailingZeroCount(value));
            Assert.AreEqual(Map(value, ScalarMath.FindLeastSignificantBit), Vec4i.FindLeastSignificantBit(value));
            Assert.AreEqual(Map(value, ScalarMath.FindMostSignificantBit), Vec4i.FindMostSignificantBit(value));
            Assert.AreEqual(MapBool(value, ScalarMath.IsPowerOfTwo), Vec4i.IsPowerOfTwo(value));
        }
    }

    /// <summary>Unsigned bit functions match their scalar component definitions.</summary>
    [TestMethod]
    public void UnsignedBitFunctions_MatchScalarResults()
    {
        foreach (var value in new[]
                 {
                     new Vec4u(0U, uint.MaxValue, 0x80000000U, 0x7fffffffU),
                     new Vec4u(1U, 2U, 4U, 8U),
                     new Vec4u(31U, 32U, 63U, 64U),
                     new Vec4u(0xAAAAAAAAU, 0x55555555U, 17U, 0x01010101U),
                 })
        {
            Assert.AreEqual(Map(value, ScalarMath.BitCount), Vec4u.BitCount(value));
            Assert.AreEqual(Map(value, ScalarMath.LeadingZeroCount), Vec4u.LeadingZeroCount(value));
            Assert.AreEqual(Map(value, ScalarMath.TrailingZeroCount), Vec4u.TrailingZeroCount(value));
            Assert.AreEqual(Map(value, ScalarMath.FindLeastSignificantBit), Vec4u.FindLeastSignificantBit(value));
            Assert.AreEqual(Map(value, ScalarMath.FindMostSignificantBit), Vec4u.FindMostSignificantBit(value));
            Assert.AreEqual(MapBool(value, ScalarMath.IsPowerOfTwo), Vec4u.IsPowerOfTwo(value));
        }
    }

    private static Vec4i Map(Vec4i value, Func<int, int> function) =>
        new(function(value.X), function(value.Y), function(value.Z), function(value.W));

    private static Vec4i Map(Vec4u value, Func<uint, int> function) =>
        new(function(value.X), function(value.Y), function(value.Z), function(value.W));

    private static Vec4b MapBool(Vec4i value, Func<int, bool> function) =>
        new(function(value.X), function(value.Y), function(value.Z), function(value.W));

    private static Vec4b MapBool(Vec4u value, Func<uint, bool> function) =>
        new(function(value.X), function(value.Y), function(value.Z), function(value.W));
}
