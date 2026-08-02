namespace AlvorKit.Hashing.Test;

[TestClass]
public class AdditiveChecksum64Test
{
    [TestMethod]
    public void Default_HasZeroValue()
    {
        AdditiveChecksum64 checksum = default;

        Assert.AreEqual(0UL, checksum.Value);
    }

    [TestMethod]
    public void Add_AllInputShapes_ContributeTheirNumericValue()
    {
        AdditiveChecksum64 checksum = default;

        checksum.Add(false);
        checksum.Add(true);
        checksum.Add(2);
        checksum.Add(3u);
        checksum.Add(4L);
        checksum.Add(5UL);

        Assert.AreEqual(15UL, checksum.Value);
    }

    [TestMethod]
    public void Add_SignedInputs_UseTwosComplementValues()
    {
        AdditiveChecksum64 checksum = default;

        checksum.Add(-1);
        checksum.Add(1L);

        Assert.AreEqual(0UL, checksum.Value);
    }

    [TestMethod]
    public void Add_Overflow_WrapsModuloTwoToTheSixtyFourth()
    {
        AdditiveChecksum64 checksum = default;

        checksum.Add(ulong.MaxValue);
        checksum.Add(2UL);

        Assert.AreEqual(1UL, checksum.Value);
    }

    [TestMethod]
    public void Copy_PreservesIndependentValues()
    {
        AdditiveChecksum64 first = default;
        first.Add(7UL);
        var second = first;

        first.Add(5UL);
        second.Add(11UL);

        Assert.AreEqual(12UL, first.Value);
        Assert.AreEqual(18UL, second.Value);
    }
}
