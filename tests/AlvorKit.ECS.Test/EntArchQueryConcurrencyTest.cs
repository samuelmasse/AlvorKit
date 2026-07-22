namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchQueryConcurrencyTest
{
    private const int FieldCount = 11;

    /// <summary>Verifies concurrent alloc owners observe a completely published cold active-side membership cache.</summary>
    [TestMethod]
    public void ArchetypalQuery_ConcurrentColdActiveSideBits_PublishCompleteSnapshot()
    {
        const int ownerCount = 8;
        MaterializeArchs();

        var counts = new int[ownerCount];
        using var barrier = new Barrier(ownerCount);
        Parallel.For(
            0,
            ownerCount,
            new ParallelOptions { MaxDegreeOfParallelism = ownerCount },
            owner => QueryOneActiveRow(owner, barrier, counts));

        CollectionAssert.AreEqual(Enumerable.Repeat(1, ownerCount).ToArray(), counts);
        Assert.AreEqual(
            1 << (FieldCount - 1),
            EntArchQueryCache<ConcurrentBitArch, EntArchSelect<int, C0, ConcurrentBitArch>>.MatchingArchCount);
    }

    private static void MaterializeArchs()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        int previousMask = 0;
        int archCount = (1 << FieldCount) - 1;
        for (int index = 1; index <= archCount; index++)
        {
            int mask = index ^ (index >> 1);
            int changed = previousMask ^ mask;
            int bit = System.Numerics.BitOperations.TrailingZeroCount((uint)changed);
            SetBit(ent, bit, (mask & changed) != 0);
            previousMask = mask;
        }

        for (int bit = 0; bit < FieldCount; bit++)
        {
            if ((previousMask & (1 << bit)) != 0)
                SetBit(ent, bit, false);
        }
    }

    private static void QueryOneActiveRow(int owner, Barrier barrier, int[] counts)
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ent.SetArchetypal<int, C0, ConcurrentBitArch>(owner);
        barrier.SignalAndWait();

        int count = 0;
        foreach (var chunk in arena.QueryArchetypal<ConcurrentBitArch>().With<int, C0>())
            count += chunk.Ents.Length;
        counts[owner] = count;
    }

    private static void SetBit(EntMut ent, int bit, bool value)
    {
        switch (bit)
        {
            case 0: Set<C0>(ent, value); break;
            case 1: Set<C1>(ent, value); break;
            case 2: Set<C2>(ent, value); break;
            case 3: Set<C3>(ent, value); break;
            case 4: Set<C4>(ent, value); break;
            case 5: Set<C5>(ent, value); break;
            case 6: Set<C6>(ent, value); break;
            case 7: Set<C7>(ent, value); break;
            case 8: Set<C8>(ent, value); break;
            case 9: Set<C9>(ent, value); break;
            case 10: Set<C10>(ent, value); break;
        }
    }

    private static void Set<N>(EntMut ent, bool value)
    {
        if (value)
            ent.SetArchetypal<int, N, ConcurrentBitArch>(0);
        else ent.UnsetArchetypal<int, N, ConcurrentBitArch>();
    }

    private readonly record struct ConcurrentBitArch;
    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct C3;
    private readonly record struct C4;
    private readonly record struct C5;
    private readonly record struct C6;
    private readonly record struct C7;
    private readonly record struct C8;
    private readonly record struct C9;
    private readonly record struct C10;
}
