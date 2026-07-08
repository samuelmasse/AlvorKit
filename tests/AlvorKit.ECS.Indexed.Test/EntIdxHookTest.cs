namespace AlvorKit.ECS.Indexed.Test;

[TestClass]
public class EntIdxHookTest
{
    /// <summary>Verifies pre and post hooks run in registration order and observe the expected component state.</summary>
    [TestMethod]
    public void EntIdxHooks_Set_RunsInOrderAndObservesState()
    {
        var context = new EntIdxContextBuilder();
        var events = new List<string>();

        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"pre1 old={ent.Value} new={value}"));
        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"pre2 old={ent.Value} new={value}"));
        context.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"post1 current={ent.Value}"));
        context.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"post2 current={ent.Value}"));

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.Value = 10;
        events.Clear();
        entity.Value = 20;

        CollectionAssert.AreEqual(
            new[]
            {
                "pre1 old=10 new=20",
                "pre2 old=10 new=20",
                "post1 current=20",
                "post2 current=20",
            },
            events);
    }

    /// <summary>Verifies unsetting an absent component is a no-op with no hook invocations.</summary>
    [TestMethod]
    public void EntIdxHooks_UnsetAbsent_IsNoOp()
    {
        var context = new EntIdxContextBuilder();
        var events = new List<string>();

        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"pre has={ent.HasValue} value={value}"));
        context.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"post has={ent.HasValue}"));

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        Assert.IsFalse(entity.UnsetValue());
        Assert.IsFalse(entity.HasValue);
        Assert.AreEqual(0, events.Count);
    }

    /// <summary>Verifies unsetting a present component fires hooks around the actual removal.</summary>
    [TestMethod]
    public void EntIdxHooks_UnsetPresent_ObservesOldThenAbsentState()
    {
        var context = new EntIdxContextBuilder();
        var events = new List<string>();

        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"pre old={ent.Value} incoming={value} has={ent.HasValue}"));
        context.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"post value={ent.Value} has={ent.HasValue}"));

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.Value = 12;
        events.Clear();

        Assert.IsTrue(entity.UnsetValue());

        CollectionAssert.AreEqual(
            new[]
            {
                "pre old=12 incoming=0 has=True",
                "post value=0 has=False",
            },
            events);
    }

    /// <summary>Verifies mutations through dead indexed handles fire no hooks and do not alter indexed state.</summary>
    [TestMethod]
    public void EntIdxHooks_DeadHandleMutation_IsInert()
    {
        var context = new EntIdxContextBuilder();
        var events = new List<string>();
        var index = new Dictionary<Guid, EntMutIdx>();

        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"value {value}"));
        context.AddPre<Guid, EntIdxTestComponents.Id>(
            (ent, in value) =>
            {
                if (value != default)
                    index.Add(value, ent);
            });

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.Value = 3;
        entity.Dispose();
        events.Clear();
        var id = Guid.NewGuid();

        entity.Value = 4;
        entity.Id = id;
        Assert.IsFalse(entity.UnsetValue());

        Assert.AreEqual(0, events.Count);
        Assert.IsFalse(index.ContainsKey(id));
        Assert.AreEqual(0, entity.Value);
    }

    /// <summary>Verifies per-entity dispose is idempotent and clears components through the indexed unset pipeline.</summary>
    [TestMethod]
    public void EntIdxHooks_Dispose_IsIdempotentAndClearsThroughHooks()
    {
        var context = new EntIdxContextBuilder();
        var events = new List<string>();

        context.AddPreDispose(ent => events.Add($"dispose value={ent.Value} has={ent.HasValue}"));
        context.AddPre<int, EntIdxTestComponents.Value>(
            (ent, in value) => events.Add($"pre value={value} old={ent.Value}"));
        context.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"post has={ent.HasValue}"));

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();

        entity.Value = 33;
        events.Clear();

        entity.Dispose();
        entity.Dispose();

        CollectionAssert.AreEqual(
            new[]
            {
                "dispose value=33 has=True",
                "pre value=0 old=33",
                "post has=False",
            },
            events);
        Assert.IsFalse(entity.IsAlive);
        Assert.AreEqual(0, arena.Allocated);
    }

    /// <summary>Verifies hook lists are isolated by context entity.</summary>
    [TestMethod]
    public void EntIdxHooks_Contexts_DoNotShareHooks()
    {
        var firstContext = new EntIdxContextBuilder();
        var secondContext = new EntIdxContextBuilder();
        var events = new List<string>();

        firstContext.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"first {ent.Value}"));
        secondContext.AddPost<int, EntIdxTestComponents.Value>(
            ent => events.Add($"second {ent.Value}"));

        using var firstArena = new EntIdxArena(firstContext.Ent);
        using var secondArena = new EntIdxArena(secondContext.Ent);

        var firstEntity = firstArena.Alloc();
        var secondEntity = secondArena.Alloc();

        firstEntity.Value = 1;
        secondEntity.Value = 2;

        CollectionAssert.AreEqual(new[] { "first 1", "second 2" }, events);
    }

    /// <summary>Verifies a pre hook can maintain a key index across set, reassignment, unset, and dispose.</summary>
    [TestMethod]
    public void EntIdxHooks_KeyIndex_TracksSetUnsetAndDispose()
    {
        var context = new EntIdxContextBuilder();
        var index = new Dictionary<Guid, EntMutIdx>();

        context.AddPre<Guid, EntIdxTestComponents.Id>(
            (ent, in value) =>
            {
                if (ent.Id == value)
                    return;

                if (ent.Id != default)
                    index.Remove(ent.Id);

                if (value != default)
                    index.Add(value, ent);
            });

        using var arena = new EntIdxArena(context.Ent);
        var entity = arena.Alloc();
        EntMutIdx mut = entity;
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        entity.Id = first;
        Assert.AreEqual(mut, index[first]);

        entity.Id = second;
        Assert.IsFalse(index.ContainsKey(first));
        Assert.AreEqual(mut, index[second]);

        Assert.IsTrue(entity.UnsetId());
        Assert.AreEqual(0, index.Count);

        entity.Id = first;
        entity.Dispose();
        Assert.AreEqual(0, index.Count);
    }
}
