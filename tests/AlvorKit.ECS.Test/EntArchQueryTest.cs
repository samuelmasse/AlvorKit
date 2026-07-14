namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchQueryTest
{
    /// <summary>Verifies multi-component selection yields aligned spans and span writes update point access.</summary>
    [TestMethod]
    public void SpanQuery_MultiComponentSelection_YieldsAlignedMutableSpans()
    {
        using var arena = new EntArena();
        EntMut firstOnly = arena.Alloc();
        EntMut pair = arena.Alloc();
        EntMut secondPair = arena.Alloc();
        EntMut triple = arena.Alloc();
        EntMut secondOnly = arena.Alloc();

        firstOnly.SetArchetypal<int, C0, QueryArch>(10);
        pair.SetArchetypal<int, C0, QueryArch>(20);
        pair.SetArchetypal<int, C1, QueryArch>(2);
        secondPair.SetArchetypal<int, C0, QueryArch>(25);
        secondPair.SetArchetypal<int, C1, QueryArch>(5);
        triple.SetArchetypal<int, C0, QueryArch>(30);
        triple.SetArchetypal<int, C1, QueryArch>(3);
        triple.SetArchetypal<int, C2, QueryArch>(300);
        secondOnly.SetArchetypal<int, C1, QueryArch>(4);

        int chunkCount = 0;
        int entCount = 0;
        int optionalCount = 0;
        var query = arena.QueryArchetypal<QueryArch>()
            .With<int, C0>()
            .With<int, C1>();

        foreach (var chunk in query)
        {
            chunkCount++;
            ReadOnlySpan<EntMut> ents = chunk.Ents;
            Span<int> first = chunk.Get<int, C0>();
            Span<int> second = chunk.Get<int, C1>();
            Span<int> optional = chunk.Get<int, C2>();

            Assert.AreEqual(ents.Length, first.Length);
            Assert.AreEqual(ents.Length, second.Length);
            optionalCount += optional.Length;

            for (int i = 0; i < ents.Length; i++)
            {
                Assert.AreEqual(ents[i].GetArchetypal<int, C0, QueryArch>(), first[i]);
                Assert.AreEqual(ents[i].GetArchetypal<int, C1, QueryArch>(), second[i]);
                first[i] += second[i];
                entCount++;
            }
        }

        Assert.AreEqual(2, chunkCount);
        Assert.AreEqual(3, entCount);
        Assert.AreEqual(1, optionalCount);
        Assert.AreEqual(10, firstOnly.GetArchetypal<int, C0, QueryArch>());
        Assert.AreEqual(22, pair.GetArchetypal<int, C0, QueryArch>());
        Assert.AreEqual(30, secondPair.GetArchetypal<int, C0, QueryArch>());
        Assert.AreEqual(33, triple.GetArchetypal<int, C0, QueryArch>());
    }

    /// <summary>Verifies an arbitrarily extended selection requires every selected component.</summary>
    [TestMethod]
    public void SpanQuery_ExtendedSelection_RequiresEveryComponent()
    {
        using var arena = new EntArena();
        EntMut pair = arena.Alloc();
        EntMut triple = arena.Alloc();

        pair.SetArchetypal<int, C0, ExtendedQueryArch>(10);
        pair.SetArchetypal<int, C1, ExtendedQueryArch>(20);
        triple.SetArchetypal<int, C0, ExtendedQueryArch>(30);
        triple.SetArchetypal<int, C1, ExtendedQueryArch>(40);
        triple.SetArchetypal<int, C2, ExtendedQueryArch>(50);

        int count = 0;
        var query = arena.QueryArchetypal<ExtendedQueryArch>()
            .With<int, C0>()
            .With<int, C1>()
            .With<int, C2>();

        foreach (var chunk in query)
        {
            count += chunk.Ents.Length;
            Assert.AreEqual(1, chunk.Get<int, C2>().Length);
            Assert.AreEqual(triple, chunk.Ents[0]);
        }

        Assert.AreEqual(1, count);
    }

    /// <summary>Verifies queries see only the rows owned by their arena's alloc.</summary>
    [TestMethod]
    public void SpanQuery_MultipleAllocs_RemainIsolated()
    {
        using var firstArena = new EntArena();
        using var secondArena = new EntArena();
        EntMut first = firstArena.Alloc();
        EntMut second = secondArena.Alloc();
        first.SetArchetypal<int, C0, AllocQueryArch>(11);
        second.SetArchetypal<int, C0, AllocQueryArch>(22);

        Assert.AreEqual(11, Sum(firstArena.QueryArchetypal<AllocQueryArch>().With<int, C0>()));
        Assert.AreEqual(22, Sum(secondArena.QueryArchetypal<AllocQueryArch>().With<int, C0>()));
    }

    /// <summary>Verifies an unused group is empty and a disposed arena rejects query creation.</summary>
    [TestMethod]
    public void SpanQuery_EmptyAndDisposedArena_UseArenaLifecycle()
    {
        var arena = new EntArena();
        int count = 0;
        foreach (var chunk in arena.QueryArchetypal<EmptyQueryArch>().With<int, C0>())
            count += chunk.Ents.Length;

        Assert.AreEqual(0, count);
        arena.Dispose();
        Assert.ThrowsExactly<EntArenaDisposedException>(() => arena.QueryArchetypal<EmptyQueryArch>());
    }

    private static int Sum<TSelect>(EntArchQuery<AllocQueryArch, TSelect> query)
        where TSelect : struct, IEntArchSelect<AllocQueryArch>
    {
        int sum = 0;
        foreach (var chunk in query)
        {
            foreach (int value in chunk.Get<int, C0>())
                sum += value;
        }

        return sum;
    }

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct QueryArch;
    private readonly record struct ExtendedQueryArch;
    private readonly record struct AllocQueryArch;
    private readonly record struct EmptyQueryArch;
}
