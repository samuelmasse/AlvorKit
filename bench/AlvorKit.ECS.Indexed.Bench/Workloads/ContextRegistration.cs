namespace AlvorKit;

/// <summary>Includes context, two bags, observer registration, and disposal; intentionally measures cold allocations.</summary>
public static class IndexedContextRegistration
{
    public static void Run(int count)
    {
        for (var i = 0; i < count; i++)
        {
            using var context = new EntIdxContext();
            context.AddBag(new EntIdxBag<BenchDirty>());
            context.AddGatedBag(new EntIdxGatedBag<BenchActive, BenchGate>());
            context.OnChange<int, BenchScalar>(Track);
        }
    }

    private static void Track(EntMutIdx ent, in EntChange<int> write)
    {
        if (write.Before != write.After)
            ent.Set<bool, BenchDirty>(true);
    }
}
