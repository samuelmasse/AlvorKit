namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonAlvorStorageTest
{
    /// <summary>Sparse updates match the scalar contract, preserve composition, and exclude padded Ents.</summary>
    [TestMethod]
    [DataRow("Update1", 0, 0)]
    [DataRow("Update1", 1, 0)]
    [DataRow("Update1", 33, 0)]
    [DataRow("Update1", 257, 10)]
    [DataRow("Update2", 0, 0)]
    [DataRow("Update2", 1, 0)]
    [DataRow("Update2", 33, 0)]
    [DataRow("Update2", 257, 10)]
    [DataRow("Update3", 0, 0)]
    [DataRow("Update3", 1, 0)]
    [DataRow("Update3", 33, 0)]
    [DataRow("Update3", 257, 10)]
    [DataRow("Mixed", 0, 0)]
    [DataRow("Mixed", 1, 0)]
    [DataRow("Mixed", 33, 0)]
    [DataRow("Mixed", 257, 0)]
    public void SparseUpdatesPreserveEveryMatchingEnt(string scenario, int count, int padding)
    {
        var components = scenario switch { "Update1" => 1, "Update3" => 3, _ => 2 };
        using var fixture = new RaceAlvorSparseContext(count, padding, components, mixed: scenario == "Mixed");
        Assert.AreEqual(count * (padding + 1), fixture.Arena.Allocated);
        Assert.AreEqual(count, fixture.Ents.Length);
        VerifySparseValues(fixture, components, 0);
        Assert.AreEqual(0, ComparisonAlvorKitInspection.Read(fixture.Arena, 1).Count);
        Assert.AreEqual(0, ComparisonAlvorKitInspection.Padding(fixture.Arena));

        switch (scenario)
        {
            case "Update1": RaceAlvorUpdate1SparseHandles.Run(fixture, 3); break;
            case "Update2": RaceAlvorUpdate2SparseHandles.Run(fixture, 3); break;
            case "Update3": RaceAlvorUpdate3SparseHandles.Run(fixture, 3); break;
            case "Mixed": RaceAlvorMixedSparseHandles.Run(fixture, 3); break;
        }

        VerifySparseValues(fixture, components, 3);

        for (var i = 0; i < count; i++)
        {
            var ent = fixture.Ents[i];
            Assert.AreEqual(scenario == "Mixed" && i % 4 == 0, ent.HasSparsePadding1);
            Assert.AreEqual(scenario == "Mixed" && i % 4 == 1, ent.HasSparsePadding2);
            Assert.AreEqual(scenario == "Mixed" && i % 4 == 2, ent.HasSparsePadding3);
            Assert.AreEqual(scenario == "Mixed" && i % 4 == 3, ent.HasSparsePadding4);
        }
    }

    /// <summary>Builder variants create distinct allocations with exactly the requested zero-valued components.</summary>
    [TestMethod]
    [DataRow("SparseMutator", 1, 1)]
    [DataRow("SparseMutator", 1, 33)]
    [DataRow("SparseMutator", 1, 257)]
    [DataRow("SparseMutator", 2, 1)]
    [DataRow("SparseMutator", 2, 33)]
    [DataRow("SparseMutator", 2, 257)]
    [DataRow("SparseMutator", 3, 1)]
    [DataRow("SparseMutator", 3, 33)]
    [DataRow("SparseMutator", 3, 257)]
    [DataRow("ArchetypalMutator", 1, 1)]
    [DataRow("ArchetypalMutator", 1, 33)]
    [DataRow("ArchetypalMutator", 1, 257)]
    [DataRow("ArchetypalMutator", 2, 1)]
    [DataRow("ArchetypalMutator", 2, 33)]
    [DataRow("ArchetypalMutator", 2, 257)]
    [DataRow("ArchetypalMutator", 3, 1)]
    [DataRow("ArchetypalMutator", 3, 33)]
    [DataRow("ArchetypalMutator", 3, 257)]
    [DataRow("ArchetypalReusedBuilder", 1, 1)]
    [DataRow("ArchetypalReusedBuilder", 1, 33)]
    [DataRow("ArchetypalReusedBuilder", 1, 257)]
    [DataRow("ArchetypalReusedBuilder", 2, 1)]
    [DataRow("ArchetypalReusedBuilder", 2, 33)]
    [DataRow("ArchetypalReusedBuilder", 2, 257)]
    [DataRow("ArchetypalReusedBuilder", 3, 1)]
    [DataRow("ArchetypalReusedBuilder", 3, 33)]
    [DataRow("ArchetypalReusedBuilder", 3, 257)]
    public void BuildersInitializeTheRequestedStorage(string variant, int components, int count)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var last = (variant, components) switch
        {
            ("SparseMutator", 1) => RaceAlvorCreate1SparseMutator.Run(fixture, count, 2),
            ("SparseMutator", 2) => RaceAlvorCreate2SparseMutator.Run(fixture, count, 2),
            ("SparseMutator", 3) => RaceAlvorCreate3SparseMutator.Run(fixture, count, 2),
            ("ArchetypalMutator", 1) => RaceAlvorCreate1ArchetypalMutator.Run(fixture, count, 2),
            ("ArchetypalMutator", 2) => RaceAlvorCreate2ArchetypalMutator.Run(fixture, count, 2),
            ("ArchetypalMutator", 3) => RaceAlvorCreate3ArchetypalMutator.Run(fixture, count, 2),
            ("ArchetypalReusedBuilder", 1) => RaceAlvorCreate1ArchetypalReusedBuilder.Run(fixture, count, 2),
            ("ArchetypalReusedBuilder", 2) => RaceAlvorCreate2ArchetypalReusedBuilder.Run(fixture, count, 2),
            ("ArchetypalReusedBuilder", 3) => RaceAlvorCreate3ArchetypalReusedBuilder.Run(fixture, count, 2),
            _ => throw new ArgumentException("Unknown creation variant."),
        };
        Assert.AreEqual(count * 2, fixture.Arena.Allocated);
        Assert.IsTrue(last.IsAlive);

        if (variant == "SparseMutator")
        {
            Assert.IsTrue(last.HasSparseComponent1);
            Assert.AreEqual(components >= 2, last.HasSparseComponent2);
            Assert.AreEqual(components == 3, last.HasSparseComponent3);
            Assert.AreEqual(0, last.SparseComponent1);
            Assert.AreEqual(0, last.SparseComponent2);
            Assert.AreEqual(0, last.SparseComponent3);
            Assert.IsFalse(last.HasComponent1);
            Assert.AreEqual(0, ComparisonAlvorKitInspection.Read(fixture.Arena, 1).Count);
        }
        else
        {
            ComparisonAlvorKitInspection.Read(fixture.Arena, components).Verify(count * 2, components, 1, true);
            Assert.IsTrue(last.HasComponent1);
            Assert.AreEqual(components >= 2, last.HasComponent2);
            Assert.AreEqual(components == 3, last.HasComponent3);
            Assert.IsFalse(last.HasSparseComponent1);
        }
    }

    private static void VerifySparseValues(RaceAlvorSparseContext fixture, int components, int passes)
    {
        foreach (var ent in fixture.Ents)
        {
            var expected = passes * Math.Max(1, components - 1);
            Assert.IsTrue(ent.HasSparseComponent1);
            Assert.AreEqual(components >= 2, ent.HasSparseComponent2);
            Assert.AreEqual(components == 3, ent.HasSparseComponent3);
            Assert.AreEqual(expected, ent.SparseComponent1);
            Assert.AreEqual(components >= 2 ? 1 : 0, ent.SparseComponent2);
            Assert.AreEqual(components == 3 ? 1 : 0, ent.SparseComponent3);
            Assert.IsFalse(ent.HasComponent1);
            Assert.IsFalse(ent.HasComponent2);
            Assert.IsFalse(ent.HasComponent3);
        }
    }
}
