namespace AlvorKit;

/// <summary>Verifies small and fat exception clauses against baseline IL boundaries.</summary>
[TestClass]
public sealed class LoadedExceptionSectionDecoderTest
{
    /// <summary>Decodes a small typed-catch clause and its metadata token.</summary>
    [TestMethod]
    public void DecodeSmallCatchSection()
    {
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 0,
            tryLength: 2,
            handlerOffset: 2,
            handlerLength: 2,
            catchToken: 0x02000003);
        var body = LoadedMethodBodyFixture.Fat(
            [0x00, 0x00, 0x00, 0x00, 0x2A],
            section: section);

        var region = LoadedMethodBodyDecoder.Decode(body)
            .ExceptionRegions
            .Single();

        Assert.AreEqual(LoadedExceptionRegionKind.Catch, region.Kind);
        Assert.AreEqual(LoadedExceptionRegionFormat.Small, region.Format);
        Assert.AreEqual(0, region.TryOffset);
        Assert.AreEqual(2, region.TryLength);
        Assert.AreEqual(2, region.HandlerOffset);
        Assert.AreEqual(2, region.HandlerLength);
        Assert.AreEqual(0x02000003, region.CatchTypeToken);
        Assert.AreEqual(-1, region.FilterOffset);
    }

    /// <summary>Decodes a fat filter clause and retains its filter coordinate.</summary>
    [TestMethod]
    public void DecodeFatFilterSection()
    {
        var section = LoadedMethodBodyFixture.FatFilter(
            tryOffset: 0,
            tryLength: 1,
            handlerOffset: 2,
            handlerLength: 2,
            filterOffset: 1);
        var body = LoadedMethodBodyFixture.Fat(
            [0x00, 0x00, 0x00, 0x00, 0x2A],
            section: section);

        var region = LoadedMethodBodyDecoder.Decode(body)
            .ExceptionRegions
            .Single();

        Assert.AreEqual(LoadedExceptionRegionKind.Filter, region.Kind);
        Assert.AreEqual(LoadedExceptionRegionFormat.Fat, region.Format);
        Assert.AreEqual(1, region.FilterOffset);
        Assert.AreEqual(0, region.CatchTypeToken);
    }
}
