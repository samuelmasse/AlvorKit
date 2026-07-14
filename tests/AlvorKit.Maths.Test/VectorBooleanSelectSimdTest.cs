namespace AlvorKit.Maths.Test;

/// <summary>Tests complete-register Boolean mask selection.</summary>
[TestClass]
public sealed class VectorBooleanSelectSimdTest
{
    /// <summary>Float selection preserves every selected payload bit, including NaNs and signed zero.</summary>
    [TestMethod]
    public void SelectFloat_PreservesSelectedBits()
    {
        var positiveNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
        var negativeNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));
        var actual = new Vec4b(true, false, true, false).Select(
            new Vec4(positiveNaN, 1f, -0f, 3f),
            new Vec4(4f, negativeNaN, 6f, 0f));

        AssertBits(actual, new Vec4(positiveNaN, negativeNaN, -0f, 0f));
    }

    /// <summary>Int32 selection chooses complete lanes for every Boolean mask pattern.</summary>
    [TestMethod]
    public void SelectInt32_ChoosesMatchingLanes()
    {
        var whenTrue = new Vec4i(int.MinValue, -1, 0, int.MaxValue);
        var whenFalse = new Vec4i(7, 8, 9, 10);

        Assert.AreEqual(new Vec4i(int.MinValue, 8, 0, 10),
            new Vec4b(true, false, true, false).Select(whenTrue, whenFalse));
        Assert.AreEqual(whenTrue, new Vec4b(true).Select(whenTrue, whenFalse));
        Assert.AreEqual(whenFalse, new Vec4b(false).Select(whenTrue, whenFalse));
    }

    private static void AssertBits(Vec4 actual, Vec4 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.W), BitConverter.SingleToInt32Bits(actual.W));
    }
}
