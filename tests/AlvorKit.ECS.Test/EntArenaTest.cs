namespace AlvorKit.ECS.Test;

[TestClass]
public class EntArenaTest
{
    /// <summary>Verifies EntArena NoOp Works.</summary>
    [TestMethod]
    public void EntArena_NoOp_Works()
    {
        using var arena = new EntArena();
    }

    /// <summary>Verifies EntArena ManyDispose IsNoOp.</summary>
    [TestMethod]
    public void EntArena_ManyDispose_IsNoOp()
    {
        using var arena = new EntArena();

        for (int i = 0; i < 10; i++)
            arena.Dispose();
    }

    /// <summary>Verifies EntArena SingleAlloc Works.</summary>
    [TestMethod]
    public void EntArena_SingleAlloc_Works()
    {
        var arena = new EntArena();

        arena.Alloc();

        Assert.AreEqual(1, arena.Allocated);
        Assert.IsTrue(arena.Capacity > 0);
        Assert.IsTrue(arena.Free > 0);
        Assert.AreEqual(arena.Capacity - arena.Free, arena.Allocated);

        arena.Dispose();

        Assert.AreEqual(0, arena.Capacity);
        Assert.AreEqual(0, arena.Allocated);
        Assert.AreEqual(0, arena.Free);
    }

    /// <summary>Verifies EntArena MultipleAlloc Works.</summary>
    [TestMethod]
    public void EntArena_MultipleAlloc_Works()
    {
        var arena = new EntArena();

        for (int i = 0; i < 50000; i++)
            arena.Alloc();

        Assert.AreEqual(50000, arena.Allocated);
        Assert.IsTrue(arena.Capacity > 0);
        Assert.IsTrue(arena.Free > 0);
        Assert.AreEqual(arena.Capacity - arena.Free, arena.Allocated);

        arena.Dispose();

        Assert.AreEqual(0, arena.Capacity);
        Assert.AreEqual(0, arena.Allocated);
        Assert.AreEqual(0, arena.Free);
    }

    /// <summary>Verifies EntArena ParallelAlloc Works.</summary>
    [TestMethod]
    public void EntArena_ParallelAlloc_Works()
    {
        var arena = new EntArena();

        Parallel.For(0, 50000, new ParallelOptions() { MaxDegreeOfParallelism = 128 }, (i) => arena.Alloc());

        Assert.AreEqual(50000, arena.Allocated);
        Assert.IsTrue(arena.Capacity > 0);
        Assert.IsTrue(arena.Free > 0);
        Assert.AreEqual(arena.Capacity - arena.Free, arena.Allocated);

        arena.Dispose();

        Assert.AreEqual(0, arena.Capacity);
        Assert.AreEqual(0, arena.Allocated);
        Assert.AreEqual(0, arena.Free);
    }

    /// <summary>Verifies EntArena Dispose ErasesComponents.</summary>
    [TestMethod]
    public void EntArena_Dispose_ErasesComponents()
    {
        var arena = new EntArena();

        var ent = arena.Alloc();
        ent.First = 43;
        ent.Third = "hi";

        Assert.IsTrue(ent.HasFirst);
        Assert.IsTrue(ent.HasThird);
        Assert.AreEqual(43, ent.First);
        arena.Dispose();
        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(ent.HasThird);
        Assert.AreEqual(0, ent.First);
    }

    /// <summary>Verifies EntArena Dispose ErasesManyComponents.</summary>
    [TestMethod]
    public void EntArena_Dispose_ErasesManyComponents()
    {
        var arena = new EntArena();
        var ents = new List<EntPtr>();

        for (int i = 0; i < 35000; i++)
        {
            ents.Add(arena.Alloc());
            var ent = ents[i];
            ent.First = 434;
            ent.Third = "hihi";
        }

        foreach (var ent in ents)
        {
            Assert.IsTrue(ent.HasThird);
            Assert.IsTrue(ent.HasFirst);
            Assert.AreEqual(434, ent.First);
        }
        arena.Dispose();
        foreach (var ent in ents)
        {
            Assert.IsFalse(ent.HasThird);
            Assert.IsFalse(ent.HasFirst);
            Assert.AreEqual(0, ent.First);
        }
    }

    /// <summary>Verifies EntArena ManualDisposes AreCountedCorrectly.</summary>
    [TestMethod]
    public void EntArena_ManualDisposes_AreCountedCorrectly()
    {
        var arena = new EntArena();
        var ents = new List<EntPtr>();

        for (int i = 0; i < 10000; i++)
            ents.Add(arena.Alloc());

        Assert.AreEqual(ents.Count, arena.Allocated);

        foreach (var ent in ents)
            ent.Dispose();

        Assert.AreEqual(0, arena.Allocated);
    }

    /// <summary>Verifies EntArena ManualDisposes IsCommon.</summary>
    [TestMethod]
    public void EntArena_ManualDisposes_IsCommon()
    {
        var arena = new EntArena();
        var arena2 = arena;

        Assert.IsTrue(arena.IsAlive);
        Assert.IsTrue(arena2.IsAlive);

        arena.Dispose();

        Assert.IsFalse(arena.IsAlive);
        Assert.IsFalse(arena2.IsAlive);
    }

    /// <summary>Verifies EntArena UseAfterFree Throws.</summary>
    [TestMethod]
    public void EntArena_UseAfterFree_Throws()
    {
        var arena = new EntArena();
        arena.Dispose();
        Assert.ThrowsExactly<EntArenaDisposedException>(() => arena.Alloc());
    }

    /// <summary>Verifies EntArena ManualDisposes AreNotCountedTwice.</summary>
    [TestMethod]
    public void EntArena_ManualDisposes_AreNotCountedTwice()
    {
        var arena = new EntArena();
        var ents = new List<EntPtr>();

        for (int i = 0; i < 10000; i++)
            ents.Add(arena.Alloc());

        Assert.AreEqual(ents.Count, arena.Allocated);

        for (int i = 0; i < 3; i++)
        {
            foreach (var ent in ents)
                ent.Dispose();
        }

        Assert.AreEqual(0, arena.Allocated);
    }
}
