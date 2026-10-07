namespace AlvorKit;

/// <summary>Exercises lifetime and registration-order boundaries around the optimized dispatch path.</summary>
[TestClass]
public class EntIdxBoundaryTest
{
    /// <summary>Adding change observation preserves an index's earlier write callbacks, including equal writes and removals.</summary>
    [TestMethod]
    public void ChangeRegistrationPreservesExistingIndexWrites()
    {
        using var context = new EntIdxContext();
        var values = new List<int>();
        var changes = 0;
        context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.Value>(ent => values.Add(ent.Value));
        context.OnChange<int, EntIdxTestComponents.Value>((ent, in change) => changes++);
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        ent.Value = 7;
        ent.Value = 7;
        Assert.IsTrue(ent.UnsetValue());
        CollectionAssert.AreEqual(new[] { 7, 7, 0 }, values);
        Assert.AreEqual(2, changes);
        Assert.IsFalse(default(EntPtrIdx).UnsetValue());
    }

    /// <summary>Dead arenas reject allocation, and recycled context identities cannot resolve to a new owner.</summary>
    [TestMethod]
    public void ReleasedContextIdentityCannotResolveReplacement()
    {
        using var context = new EntIdxContext();
        var identity = context.Identity;
        using var arena = new EntIdxArena(context);
        arena.Dispose();
        Assert.ThrowsExactly<EntArenaDisposedException>(() => arena.Alloc());
        context.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => EntIdxContexts.Get(identity));
        using var replacement = new EntIdxContext();
        Assert.ThrowsExactly<ObjectDisposedException>(() => EntIdxContexts.Get(identity));
        Assert.AreSame(replacement, EntIdxContexts.Get(replacement.Identity));
    }

    /// <summary>A nested callback may end an idle foreign context while active contexts retain their lifetime guard.</summary>
    [TestMethod]
    public void NestedReactionMayDisposeIdleForeignContext()
    {
        using var first = new EntIdxContext();
        using var second = new EntIdxContext();
        using var idle = new EntIdxContext();
        using var firstArena = new EntIdxArena(first);
        using var secondArena = new EntIdxArena(second);
        EntPtrIdx secondEnt = default;
        first.OnWrite<int, EntIdxTestComponents.Value>(ent => secondEnt.Value = ent.Value);
        second.OnWrite<int, EntIdxTestComponents.Value>(ent => idle.Dispose());
        var firstEnt = firstArena.Alloc();
        secondEnt = secondArena.Alloc();
        firstEnt.Value = 5;
        Assert.IsFalse(idle.IsAlive);
        Assert.IsTrue(first.IsAlive);
        Assert.IsTrue(second.IsAlive);
        Assert.AreEqual(5, secondEnt.Value);
    }
}
