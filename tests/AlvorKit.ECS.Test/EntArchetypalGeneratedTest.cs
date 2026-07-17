namespace AlvorKit.ECS.Test;

[Components]
public interface IGeneratedArchComponents
{
    int SparseValue { get; set; }

    [Archetypal]
    [ComponentToString]
    int Health { get; set; }

    [Archetypal]
    string? Label { get; set; }

    [Archetypal]
    [ComponentLazyInitialize]
    List<int> Items { get; set; }
}

[TestClass]
public sealed class EntArchetypalGeneratedTest
{
    /// <summary>Generated mixed accessors route marked properties through one archetype group and preserve sparse properties.</summary>
    [TestMethod]
    public void GeneratedArchetypalAccessors_MixedStorage_ReadWriteUnsetAndBuild()
    {
        using var arena = new EntArena();
        EntPtr ptr = arena.Alloc();
        EntRefMut ent = ptr;

        ent.SparseValue = 3;
        ent.Health = 10;
        ent.Label = "unit";
        ent.Items.Add(4);

        Assert.IsTrue(ent.HasSparseValue);
        Assert.IsTrue(ent.HasHealth);
        Assert.IsTrue(ent.HasLabel);
        Assert.IsTrue(ent.HasItems);
        Assert.AreEqual(3, ent.SparseValue);
        Assert.AreEqual(10, ent.Health);
        Assert.AreEqual("unit", ent.Label);
        CollectionAssert.AreEqual(new[] { 4 }, ent.Items);

        ent.Mutate().Health(20).Label("updated");
        Assert.AreEqual(20, ent.Health);
        Assert.AreEqual("updated", ent.Label);

        var debugComponents = new EntDebugView(ent).Components;
        Assert.AreEqual(4, debugComponents.Length);
        Assert.IsTrue(debugComponents.Any(component =>
            component is EntDebugView.DebugViewComponentPrimitive primitive &&
            primitive.Name == "GeneratedArchComponents.Health" &&
            Equals(primitive.Value, 20)));
        Assert.IsTrue(debugComponents.Any(component =>
            component is EntDebugView.DebugViewComponentPrimitive primitive &&
            primitive.Name == "GeneratedArchComponents.Label" &&
            Equals(primitive.Value, "updated")));
        StringAssert.Contains(ent.ToString(), "GeneratedArchComponents.Health = 20");

        Assert.IsTrue(ent.UnsetLabel());
        Assert.IsFalse(ent.HasLabel);
        Assert.IsNull(ent.Label);

        ent.Clear();
        Assert.IsFalse(ent.HasSparseValue);
        Assert.IsFalse(ent.HasHealth);
        Assert.IsFalse(ent.HasItems);
        Assert.IsFalse(((EntMut)ptr).Has<EntArchLoc, GeneratedArchComponents>());
    }

    /// <summary>Generated rows flatten matching archs and expose aligned direct refs without allocating in the loop.</summary>
    [TestMethod]
    public void GeneratedArchetypalRows_MultipleComponents_ReadWriteAndAllocateNothing()
    {
        using var arena = new EntArena();
        EntMut first = arena.Alloc();
        EntMut second = arena.Alloc();
        EntMut third = arena.Alloc();
        EntMut excluded = arena.Alloc();

        first.Health = 10;
        first.Label = "a";
        second.Health = 20;
        second.Label = "bb";
        second.Items.Add(1);
        third.Health = 30;
        third.Label = "ccc";
        excluded.Health = 40;

        var query = arena.QueryArchetypal<GeneratedArchComponents>()
            .WithHealth()
            .WithLabel();

        long warmup = 0;
        foreach (var row in query.Rows())
            warmup += row.Health;
        Assert.AreEqual(60, warmup);

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int count = 0;
        foreach (var row in query.Rows())
        {
            Assert.AreEqual(row.Health, row.Ent.Health);
            row.Health += row.Label!.Length;
            row.Label = "updated by row";
            count++;
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        Assert.AreEqual(3, count);
        Assert.AreEqual(0, allocated);
        Assert.AreEqual(11, first.Health);
        Assert.AreEqual(22, second.Health);
        Assert.AreEqual(33, third.Health);
        Assert.AreEqual(40, excluded.Health);
        Assert.AreEqual("updated by row", first.Label);
        Assert.AreEqual("updated by row", second.Label);
        Assert.AreEqual("updated by row", third.Label);
    }
}
