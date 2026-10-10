namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class EntIdxArchetypalArenaTest
{
    /// <summary>Final-shape creation closes registration at allocation and preserves sparse membership and disposal hooks.</summary>
    [TestMethod]
    public void FinalShape_PreservesIndexedOwnership()
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        var shape = arena.AllocArchetypal<EntIdxTestComponents>()
            .With<int, EntIdxTestComponents.ArchValue>(42).With<bool, EntIdxTestComponents.IsArchThing>(true);
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);
        var writes = 0;
        var disposedValue = 0;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => writes++);
        context.OnDisposing(ent => disposedValue = ent.ArchValue);
        var ent = shape.Create();
        Assert.AreEqual(42, ent.ArchValue);
        Assert.IsTrue(ent.IsArchThing);
        Assert.AreEqual(0, bag.Count);
        Assert.ThrowsExactly<EntIdxRegistrationException>(() => context.OnDisposing(value => { }));
        ent.Value = 7;
        ent.IsThing = true;
        Assert.AreEqual(1, writes);
        Assert.AreEqual(1, bag.Count);
        ent.Dispose();
        Assert.AreEqual(42, disposedValue);
        Assert.AreEqual(0, bag.Count);
        Assert.AreEqual(0, arena.Allocated);
    }

    /// <summary>Bulk dense writes are unobserved, arena-local, and leave sparse state and membership unchanged.</summary>
    [TestMethod]
    public void Query_IsArenaLocalAndUnobserved()
    {
        using var context = new EntIdxContext();
        var bag = new EntIdxBag<EntIdxTestComponents.IsThing>();
        context.AddBag(bag);
        var writes = 0;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => writes++);
        using var first = new EntIdxArena(context);
        using var second = new EntIdxArena(context);
        var one = first.AllocArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>(10).Create();
        var two = second.AllocArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>(20).Create();
        one.IsThing = true;
        two.IsThing = true;
        one.Value = 3;

        foreach (var chunk in first.QueryArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>())
        {
            foreach (ref var value in chunk.Get<int, EntIdxTestComponents.ArchValue>())
                value++;
        }

        Assert.AreEqual(11, one.ArchValue);
        Assert.AreEqual(20, two.ArchValue);
        Assert.AreEqual(3, one.Value);
        Assert.AreEqual(1, writes);
        Assert.AreEqual(2, bag.Count);
    }

    /// <summary>Captured builders and queries reject arena disposal, including after another arena reuses its storage.</summary>
    [TestMethod]
    public void RetainedOperations_RejectDisposedArena()
    {
        using var context = new EntIdxContext();
        var arena = new EntIdxArena(context);
        var shape = arena.AllocArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>(1);
        var query = arena.QueryArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>();
        arena.Dispose();
        using var replacement = new EntIdxArena(context);
        replacement.Alloc();
        Assert.ThrowsExactly<EntArenaDisposedException>(() => shape.Create());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => query.GetEnumerator());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => arena.AllocArchetypal<EntIdxTestComponents>());
        Assert.ThrowsExactly<EntArenaDisposedException>(() => arena.QueryArchetypal<EntIdxTestComponents>());
        Assert.ThrowsExactly<EntArenaDisposedException>(() =>
            default(EntIdxArchCreate<EntIdxTestComponents>).With<int, EntIdxTestComponents.ArchValue>(0).Create());
    }

    /// <summary>Final-shape allocation cannot bypass the prohibition on allocating from an index callback.</summary>
    [TestMethod]
    public void FinalShape_RejectsAllocationDuringIndexMaintenance()
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        var shape = arena.AllocArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>(1);
        var rejected = false;
        context.AddIndex(ent => { }).OnChange<int, EntIdxTestComponents.Value>((EntMutIdx ent, in EntChange<int> change) =>
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => shape.Create());
            rejected = true;
        });
        var ent = arena.Alloc();
        ent.Value = 1;
        Assert.IsTrue(rejected);
        Assert.AreEqual(1, arena.Allocated);
    }

    /// <summary>Repeated final-shape recycling and bulk traversal do not introduce hot-path allocation.</summary>
    [TestMethod]
    public void FinalShapeAndQuery_DoNotAllocateAfterWarmup()
    {
        using var context = new EntIdxContext();
        using var arena = new EntIdxArena(context);
        // Exhaust the first 4,096-slot page before measuring recycled IDs and the warmed free list.
        Cycle(arena, 8192);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var sum = Cycle(arena, 128);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(256, sum);
        Assert.AreEqual(0L, allocated);
        Assert.AreEqual(0, arena.Allocated);
    }

    private static int Cycle(EntIdxArena arena, int count)
    {
        var sum = 0;

        for (var iteration = 0; iteration < count; iteration++)
        {
            var ent = arena.AllocArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>(1).Create();

            foreach (var chunk in arena.QueryArchetypal<EntIdxTestComponents>().With<int, EntIdxTestComponents.ArchValue>())
            {
                foreach (ref var value in chunk.Get<int, EntIdxTestComponents.ArchValue>())
                    sum += ++value;
            }

            ent.Dispose();
        }

        return sum;
    }
}
