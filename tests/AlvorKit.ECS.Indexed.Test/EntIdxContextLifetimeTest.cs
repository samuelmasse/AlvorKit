using System.Runtime.CompilerServices;

namespace AlvorKit;

[TestClass]
public class EntIdxContextLifetimeTest
{
    /// <summary>Disposing a context clears hook captures that point back to the context owner.</summary>
    [TestMethod]
    public void Dispose_ReleasesCapturedOwnersAndRejectsRegistration()
    {
        using var context = new EntIdxContextBuilder();
        var handle = context.Ent;
        var retained = AttachOwner(context);
        GC.Collect();
        Assert.IsTrue(retained.IsAlive);

        context.Dispose();
        context.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(HasHooks(handle));
        Assert.IsFalse(retained.IsAlive);
        Assert.ThrowsExactly<ObjectDisposedException>(() => context.AddPreDispose(static _ => { }));
        GC.KeepAlive(context);
    }

    /// <summary>A borrowed context survives one arena's teardown and remains usable until all its arenas end.</summary>
    [TestMethod]
    public void ArenaDispose_LeavesSharedContextAlive()
    {
        using var context = new EntIdxContextBuilder();
        var calls = 0;
        context.AddPost<int, EntIdxTestComponents.Value>(_ => calls++);
        using var first = new EntIdxArena(context.Ent);
        using var second = new EntIdxArena(context.Ent);
        first.Alloc().Mutate().Value(1);
        first.Dispose();
        Assert.IsTrue(HasHooks(context.Ent));

        var survivor = second.Alloc();
        survivor.Value = 2;
        Assert.AreEqual(2, calls);
        Assert.IsTrue(survivor.IsAlive);
        second.Dispose();
        context.Dispose();
        Assert.IsFalse(survivor.IsAlive);
        Assert.IsFalse(HasHooks(context.Ent));
    }

    private static bool HasHooks(Ent context) =>
        context.Has<ReadOnlyMemory<EntIdxPostHook>, EntIdxPost<int, EntIdxTestComponents.Value>>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachOwner(EntIdxContextBuilder context)
    {
        var owner = new HookOwner(context);
        context.AddPost<int, EntIdxTestComponents.Value>(owner.Run);
        return new(owner);
    }

    private class HookOwner(EntIdxContextBuilder context)
    {
        public void Run(EntMutIdx _) => GC.KeepAlive(context);
    }
}
