namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class EntIdxArchetypalAccessTest
{
    /// <summary>Generated accessors on owning and borrowed handles do not box during steady-state archetypal access.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void GeneratedAccess_DoesNotAllocate(bool owning)
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        var ptr = arena.Alloc();
        ptr.ArchValue = 0;

        if (owning)
            AssertNoAllocations(ptr);
        else AssertNoAllocations<EntMutIdx>(ptr);
    }

    /// <summary>Archetypal changes preserve sparse bag membership and do not dispatch sparse write observers.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void StructuralAccess_PreservesIndexedState(bool owning)
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);
        var writes = 0;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => writes++);
        using var arena = new EntIdxArena(context);
        var ptr = arena.Alloc();
        ptr.IsThing = true;
        ptr.Value = 7;

        if (owning)
            VerifyStructuralAccess(ptr);
        else VerifyStructuralAccess<EntMutIdx>(ptr);

        Assert.AreEqual(1, bag.Ents.Length);
        Assert.AreEqual(ptr.Handle, bag.Ents[0].Handle);
        Assert.AreEqual(7, ptr.Value);
        Assert.AreEqual(1, writes);
        ptr.Dispose();
        Assert.AreEqual(0, bag.Ents.Length);
    }

    private static void AssertNoAllocations<T>(T ent) where T : IEntMut
    {
        Access(ent, 4096);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var visits = Access(ent, 4096);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(4096, visits);
        Assert.AreEqual(8192, ent.ArchValue);
        Assert.AreEqual(0L, allocated);
    }

    private static int Access<T>(T ent, int count) where T : IEntMut
    {
        var visits = 0;

        for (var index = 0; index < count; index++)
        {
            ent.Mutate().ArchValue(ent.ArchValue + 1);

            if (ent.HasArchValue && !ent.UnsetIsArchThing())
                visits++;
        }

        return visits;
    }

    private static void VerifyStructuralAccess<T>(T ent) where T : IEntMut
    {
        Assert.IsFalse(ent.HasArchValue);
        Assert.AreEqual(0, ent.ArchValue);
        Assert.IsFalse(ent.UnsetArchValue());
        ent.ArchValue = 17;
        Assert.IsTrue(ent.HasArchValue);
        Assert.AreEqual(17, ent.ArchValue);
        Assert.IsTrue(ent.UnsetArchValue());
        Assert.IsFalse(ent.HasArchValue);
        Assert.AreEqual(0, ent.ArchValue);
        Assert.IsFalse(ent.UnsetArchValue());
    }
}
