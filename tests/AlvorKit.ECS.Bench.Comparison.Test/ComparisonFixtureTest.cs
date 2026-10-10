using static AlvorKit.RaceFrentMixed.FrentContext;
using Frent;
using Frent.Systems;

namespace AlvorKit;

[TestClass]
public class ComparisonFixtureTest
{
    /// <summary>The mixed workload uses four signatures, including a deterministic uneven tail.</summary>
    [TestMethod]
    public void MixedCompositionsDistributeByEntIndex()
    {
        using var fixture = new RaceFrentMixed.FrentContext(33);
        Assert.AreEqual(9, Count<Padding1>(fixture.World));
        Assert.AreEqual(8, Count<Padding2>(fixture.World));
        Assert.AreEqual(8, Count<Padding3>(fixture.World));
        Assert.AreEqual(8, Count<Padding4>(fixture.World));
    }

    /// <summary>Padding adds nonmatching Ents while preserving the matching population and values.</summary>
    [TestMethod]
    public void PaddingCreatesOnlyNonmatchingEnts()
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(33, 10);
        Assert.AreEqual(330, Count<ComparisonPadding>(fixture.World));
        ComparisonFrentInspection.Read(fixture.World, 3).Verify(33, 3, 0, false);
    }

    private static int Count<T>(Frent.World world)
    {
        var count = 0;
        world.Query<T>().Delegate((ref T value) => count++);
        return count;
    }
}
