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

    /// <summary>Verifies the cache extends for new signatures and retained matches survive empty row storage.</summary>
    [TestMethod]
    public void SpanQuery_CacheExtendsForNewSignaturesAndSurvivesEmptyRows()
    {
        using var arena = new EntArena();
        var query = arena.QueryArchetypal<CacheQueryArch>()
            .With<int, C0>()
            .With<int, C1>();

        Assert.AreEqual(0, Count(query));
        Assert.AreEqual(0, CacheMatchingArchCount);
        Assert.AreEqual(EntArchGraph<CacheQueryArch>.PublishedArchEnd, CacheScannedArchEnd);

        EntMut pair = arena.Alloc();
        pair.SetArchetypal<int, C0, CacheQueryArch>(10);
        Assert.AreEqual(0, Count(query));
        Assert.AreEqual(0, CacheMatchingArchCount);

        pair.SetArchetypal<int, C1, CacheQueryArch>(20);
        Assert.AreEqual(1, Count(query));
        Assert.AreEqual(1, CacheMatchingArchCount);
        Assert.AreEqual(EntArchGraph<CacheQueryArch>.PublishedArchEnd, CacheScannedArchEnd);
        int pairArchEnd = CacheScannedArchEnd;

        Assert.IsTrue(pair.UnsetArchetypal<int, C1, CacheQueryArch>());
        Assert.AreEqual(0, Count(query));
        Assert.AreEqual(1, CacheMatchingArchCount);
        Assert.AreEqual(pairArchEnd, CacheScannedArchEnd);

        pair.SetArchetypal<int, C1, CacheQueryArch>(21);
        Assert.AreEqual(1, Count(query));
        Assert.AreEqual(pairArchEnd, CacheScannedArchEnd);

        EntMut triple = arena.Alloc();
        triple.SetArchetypal<int, C0, CacheQueryArch>(30);
        triple.SetArchetypal<int, C1, CacheQueryArch>(40);
        triple.SetArchetypal<int, C2, CacheQueryArch>(50);

        Assert.AreEqual(2, Count(query));
        Assert.AreEqual(2, CacheMatchingArchCount);
        Assert.AreEqual(EntArchGraph<CacheQueryArch>.PublishedArchEnd, CacheScannedArchEnd);
    }

    /// <summary>Verifies captured arch IDs remain valid across cache growth and warm enumeration allocates nothing.</summary>
    [TestMethod]
    public void SpanQuery_CapturedCacheSurvivesGrowthAndWarmEnumerationAllocatesNothing()
    {
        using var firstArena = new EntArena();
        SetGrowthArch(firstArena.Alloc());
        SetGrowthArch<C1>(firstArena.Alloc());
        SetGrowthArch<C2>(firstArena.Alloc());
        SetGrowthArch<C3>(firstArena.Alloc());

        var firstQuery = firstArena.QueryArchetypal<CacheGrowthArch>().With<int, C0>();
        Assert.AreEqual(4, Count(firstQuery));
        Assert.AreEqual(4, GrowthCacheMatchingArchCount);
        var captured = firstQuery.GetEnumerator();

        using var secondArena = new EntArena();
        SetGrowthArch<C4>(secondArena.Alloc());
        var secondQuery = secondArena.QueryArchetypal<CacheGrowthArch>().With<int, C0>();
        Assert.AreEqual(1, Count(secondQuery));
        Assert.AreEqual(5, GrowthCacheMatchingArchCount);

        int capturedCount = 0;
        while (captured.MoveNext())
            capturedCount += captured.Current.Ents.Length;
        Assert.AreEqual(4, capturedCount);

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int warmCount = 0;
        for (int iteration = 0; iteration < 1_000; iteration++)
            warmCount += Count(firstQuery);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        Assert.AreEqual(4_000, warmCount);
        Assert.AreEqual(0, allocated);
    }

    /// <summary>Verifies lazy query bits grow across newly published nonmatching archs before active-side membership reads.</summary>
    [TestMethod]
    public void SpanQuery_ActiveSideBits_GrowAcrossNonmatchingArchs()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        var query = arena.QueryArchetypal<BitGrowthArch>().With<int, C0>();

        ent.SetArchetypal<int, C0, BitGrowthArch>(0);
        ent.SetArchetypal<int, B1, BitGrowthArch>(1);
        Assert.AreEqual(1, Count(query));
        Assert.IsTrue(ent.UnsetArchetypal<int, C0, BitGrowthArch>());

        int previousMask = 1;
        for (int index = 2; index < 128; index++)
        {
            int mask = index ^ (index >> 1);
            int changed = previousMask ^ mask;
            bool set = (mask & changed) != 0;
            int field = 0;
            while ((changed & 1) == 0)
            {
                changed >>= 1;
                field++;
            }
            ToggleBitGrowth(ent, field, set);
            previousMask = mask;
        }

        Assert.AreEqual(0, Count(query));
        Assert.IsTrue(EntArchDiagnostics<BitGrowthArch>.Capture().MaterializedArchCount > 64);
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

    private static int Count<A, TSelect>(EntArchQuery<A, TSelect> query)
        where TSelect : struct, IEntArchSelect<A>
    {
        int count = 0;
        foreach (var chunk in query)
            count += chunk.Ents.Length;
        return count;
    }

    private static void SetGrowthArch(EntMut ent) =>
        ent.SetArchetypal<int, C0, CacheGrowthArch>(0);

    private static void SetGrowthArch<N>(EntMut ent)
    {
        ent.SetArchetypal<int, C0, CacheGrowthArch>(0);
        ent.SetArchetypal<int, N, CacheGrowthArch>(1);
    }

    private static void ToggleBitGrowth(EntMut ent, int field, bool set)
    {
        switch (field)
        {
            case 0: ToggleBitGrowth<B1>(ent, set); break;
            case 1: ToggleBitGrowth<B2>(ent, set); break;
            case 2: ToggleBitGrowth<B3>(ent, set); break;
            case 3: ToggleBitGrowth<B4>(ent, set); break;
            case 4: ToggleBitGrowth<B5>(ent, set); break;
            case 5: ToggleBitGrowth<B6>(ent, set); break;
            case 6: ToggleBitGrowth<B7>(ent, set); break;
        }
    }

    private static void ToggleBitGrowth<N>(EntMut ent, bool set)
    {
        if (set)
            ent.SetArchetypal<int, N, BitGrowthArch>(0);
        else ent.UnsetArchetypal<int, N, BitGrowthArch>();
    }

    private static int CacheMatchingArchCount =>
        EntArchQueryCache<
            CacheQueryArch,
            EntArchSelect<int, C1, CacheQueryArch, EntArchSelect<int, C0, CacheQueryArch>>>
        .MatchingArchCount;

    private static int CacheScannedArchEnd =>
        EntArchQueryCache<
            CacheQueryArch,
            EntArchSelect<int, C1, CacheQueryArch, EntArchSelect<int, C0, CacheQueryArch>>>
        .ScannedArchEnd;

    private static int GrowthCacheMatchingArchCount =>
        EntArchQueryCache<CacheGrowthArch, EntArchSelect<int, C0, CacheGrowthArch>>
            .MatchingArchCount;

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct QueryArch;
    private readonly record struct ExtendedQueryArch;
    private readonly record struct AllocQueryArch;
    private readonly record struct EmptyQueryArch;
    private readonly record struct CacheQueryArch;
    private readonly record struct CacheGrowthArch;
    private readonly record struct C3;
    private readonly record struct C4;
    private readonly record struct B1;
    private readonly record struct B2;
    private readonly record struct B3;
    private readonly record struct B4;
    private readonly record struct B5;
    private readonly record struct B6;
    private readonly record struct B7;
    private readonly record struct BitGrowthArch;
}
