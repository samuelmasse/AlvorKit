namespace AlvorKit;

[TestClass]
public class EntIdxSurfaceTest
{
    /// <summary>Read-only bags maintain membership and Indexed handles forward non-owning reads and mutation.</summary>
    [TestMethod]
    public void HandlesAndBags_ExposeCurrentState()
    {
        using var context = new EntIdxContext();
        var plain = new EntIdxBag<EntIdxTestComponents.IsThing>();
        var gated = new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsReady>();
        context.AddBag(plain);
        context.AddGatedBag(gated);
        using var arena = new EntIdxArena(context);
        var ptr = arena.Alloc();
        EntMutIdx ent = ptr;
        Span<EntMutIdx> copies = stackalloc EntMutIdx[arena.Allocated];
        copies[0] = ent;
        copies[0].Value = 6;
        Assert.AreEqual(6, ptr.Value);
        ent.IsThing = true;
        ent.Value = 7;
        Assert.IsTrue(plain.Contains(ent));
        Assert.AreEqual(1, plain.Ents.Length);
        Assert.AreEqual(0, gated.Count);
        ent.IsReady = true;
        Assert.IsTrue(gated.Contains(ent));
        Ent read = ent;
        Assert.AreEqual(ptr.Handle, read.Handle);
        Assert.IsTrue(ent.Unset<int, EntIdxTestComponents.Value>());
        Assert.IsTrue(ent.ToString().StartsWith("Ent", StringComparison.Ordinal));
        ent.Clear();
        Assert.AreEqual(0, plain.Count);
        Assert.AreEqual(0, gated.Count);
        Assert.IsTrue(ptr.IsAlive);
    }
}
