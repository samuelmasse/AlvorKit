using Entitas;
using static AlvorKit.RaceEntitasContext;

namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonEntitasTest
{
    /// <summary>Creation produces independent, zero-initialized components at empty, small, and odd counts.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(1, 1)]
    [DataRow(1, 33)]
    [DataRow(1, 257)]
    [DataRow(2, 0)]
    [DataRow(2, 1)]
    [DataRow(2, 33)]
    [DataRow(2, 257)]
    [DataRow(3, 0)]
    [DataRow(3, 1)]
    [DataRow(3, 33)]
    [DataRow(3, 257)]
    public void CreationInitializesEveryComponent(int components, int count)
    {
        using var fixture = new RaceEntitasContext(components);
        RunCreation(fixture, components, count);
        Assert.AreEqual(count, fixture.World.count);
        VerifyData(fixture.World.GetEntities(), count, components, 0, 0);
        var firstComponents = fixture.World.GetEntities().Select(ent => ent.GetComponent(0)).ToArray();
        Assert.AreEqual(count, firstComponents.Distinct(ReferenceEqualityComparer.Instance).Count());
    }

    /// <summary>Reused pooled components are reinitialized rather than retaining their previous values.</summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void CreationResetsPooledValues(int components)
    {
        using var fixture = new RaceEntitasContext(components);
        RunCreation(fixture, components, 7);

        foreach (var ent in fixture.World.GetEntities())
        {
            ((Component1)ent.GetComponent(0)).Value = 91;

            if (components >= 2)
                ((Component2)ent.GetComponent(1)).Value = 92;

            if (components == 3)
                ((Component3)ent.GetComponent(2)).Value = 93;
        }

        fixture.World.DestroyAllEntities();
        RunCreation(fixture, components, 7);
        Assert.AreEqual(7, fixture.World.count);
        VerifyData(fixture.World.GetEntities(), 7, components, 0, 0);
    }

    /// <summary>All update arities process every matching Ent for all passes and exclude interleaved padding.</summary>
    [TestMethod]
    [DataRow(1, 0, 10)]
    [DataRow(1, 1, 0)]
    [DataRow(1, 33, 1)]
    [DataRow(1, 257, 10)]
    [DataRow(2, 0, 10)]
    [DataRow(2, 1, 0)]
    [DataRow(2, 33, 1)]
    [DataRow(2, 257, 10)]
    [DataRow(3, 0, 10)]
    [DataRow(3, 1, 0)]
    [DataRow(3, 33, 1)]
    [DataRow(3, 257, 10)]
    public void UpdatesPreserveMatchingPopulation(int components, int count, int padding)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateUpdate(count, padding, components);
        Assert.AreEqual(count * (padding + 1), fixture.World.count);
        VerifyData(fixture.Group.GetEntities(), count, components, 0, 1);
        RunUpdate(fixture, components, count, 64);
        VerifyData(fixture.Group.GetEntities(), count, components, 64 * Math.Max(1, components - 1), 1);
        var padded = fixture.World.GetEntities().Where(ent => ent.HasComponent(3)).ToArray();
        Assert.AreEqual(count * padding, padded.Length);

        foreach (var ent in padded)
        {
            Assert.HasCount(1, ent.GetComponents());
            Assert.IsInstanceOfType<Padding>(ent.GetComponent(3));
        }
    }

    /// <summary>Mixed traversal updates all four signatures, including uneven distributions.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(33)]
    [DataRow(257)]
    public void MixedPreservesFourSignatures(int count)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateMixed(count);
        Assert.AreEqual(count, fixture.World.count);
        RaceEntitasMixedGroupDirect.Run(fixture, count, 64);
        var ents = fixture.Group.GetEntities();
        Assert.HasCount(count, ents);
        var markers = new int[4];

        foreach (var ent in ents)
        {
            Assert.AreEqual(64, ((Component1)ent.GetComponent(0)).Value);
            Assert.AreEqual(1, ((Component2)ent.GetComponent(1)).Value);
            Assert.HasCount(3, ent.GetComponents());
            Assert.IsFalse(ent.HasComponent(2));

            for (var i = 0; i < markers.Length; i++)
            {
                if (ent.HasComponent(3 + i))
                    markers[i]++;
            }
        }

        for (var i = 0; i < markers.Length; i++)
            Assert.AreEqual((count + 3 - i) / 4, markers[i]);
    }

    /// <summary>Fixture disposal removes all Ents and releases the matching group's retained references.</summary>
    [TestMethod]
    public void DisposalClearsContextAndGroup()
    {
        var fixture = RaceEntitasUpdateFixture.CreateMixed(33);
        fixture.Dispose();
        fixture.Dispose();
        Assert.AreEqual(0, fixture.World.count);
        Assert.AreEqual(0, fixture.World.retainedEntitiesCount);
        Assert.AreEqual(0, fixture.Group.count);
    }

    private static void VerifyData(Entity[] ents, int count, int components, int first, int other)
    {
        Assert.HasCount(count, ents);

        foreach (var ent in ents)
        {
            Assert.HasCount(components, ent.GetComponents());
            Assert.AreEqual(first, ((Component1)ent.GetComponent(0)).Value);

            if (components >= 2)
                Assert.AreEqual(other, ((Component2)ent.GetComponent(1)).Value);

            if (components == 3)
                Assert.AreEqual(other, ((Component3)ent.GetComponent(2)).Value);
        }
    }

    private static void RunCreation(RaceEntitasContext fixture, int components, int count)
    {
        switch (components)
        {
            case 1: RaceEntitasCreate1Default.Run(fixture, count, 1); break;
            case 2: RaceEntitasCreate2Default.Run(fixture, count, 1); break;
            case 3: RaceEntitasCreate3Default.Run(fixture, count, 1); break;
            default: throw new ArgumentOutOfRangeException(nameof(components));
        }
    }

    private static void RunUpdate(RaceEntitasUpdateFixture fixture, int components, int count, int passes)
    {
        switch (components)
        {
            case 1: RaceEntitasUpdate1GroupDirect.Run(fixture, count, passes); break;
            case 2: RaceEntitasUpdate2GroupDirect.Run(fixture, count, passes); break;
            case 3: RaceEntitasUpdate3GroupDirect.Run(fixture, count, passes); break;
            default: throw new ArgumentOutOfRangeException(nameof(components));
        }
    }
}
