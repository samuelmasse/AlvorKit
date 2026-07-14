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
}
