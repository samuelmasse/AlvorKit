namespace AlvorKit;

[TestClass]
public class EntIdxContextLifetimeTest
{
    /// <summary>Context disposal clears captures, including cycles back to its owner.</summary>
    [TestMethod]
    public void Dispose_ReleasesCapturedOwners()
    {
        using var context = new EntIdxContext();
        var retained = AttachOwner(context);
        GC.Collect();
        Assert.IsTrue(retained.IsAlive);

        context.Dispose();
        context.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(context.IsAlive);
        Assert.IsFalse(retained.IsAlive);
        Assert.ThrowsExactly<ObjectDisposedException>(() => context.OnDisposing(ent => { }));
        Assert.ThrowsExactly<ObjectDisposedException>(() => new EntIdxArena(context));
        GC.KeepAlive(context);
    }

    /// <summary>A shared context survives the first arena and cannot be disposed before the last arena.</summary>
    [TestMethod]
    public void Dispose_RequiresEveryArenaToEnd()
    {
        using var context = new EntIdxContext();
        int calls = 0;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => calls++);
        using var first = new EntIdxArena(context);
        using var second = new EntIdxArena(context);
        first.Alloc().Mutate().Value(1);
        first.Dispose();
        Assert.ThrowsExactly<InvalidOperationException>(() => context.Dispose());
        Assert.IsTrue(context.IsAlive);

        var survivor = second.Alloc();
        survivor.Value = 2;
        Assert.AreEqual(2, calls);
        second.Dispose();
        context.Dispose();
        Assert.IsFalse(survivor.IsAlive);
        Assert.IsFalse(context.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachOwner(EntIdxContext context)
    {
        var owner = new HookOwner(context);
        context.OnWrite<int, EntIdxTestComponents.Value>(owner.Run);
        context.OnClearing(owner.Remove);
        context.OnDisposing(owner.Remove);
        context.AddIndex(owner.Remove);
        return new(owner);
    }

    private class HookOwner(EntIdxContext context)
    {
        public void Run(EntMutIdx _) => GC.KeepAlive(context);

        public void Remove(EntMutIdx _) => GC.KeepAlive(context);
    }
}
