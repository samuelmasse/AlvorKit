namespace AlvorKit;

/// <summary>Prepared sparse data and observers; allocations here are excluded from workload counters.</summary>
public class IndexedBenchFixture : IDisposable
{
    private readonly EntIdxContext context = new();
    private readonly EntIdxArena arena;
    private readonly EntPtrIdx[] ents;
    private readonly EntIdxBag<BenchDirty> dirty = new();
    private readonly EntIdxGatedBag<BenchActive, BenchGate> active = new();
    private readonly Dictionary<int, EntMutIdx> keys = [];
    private readonly int[] array = [1, 2, 3, 4];
    private long observed;

    public ReadOnlySpan<EntPtrIdx> Ents => ents;
    public int[] Array => array;
    public EntIdxArena Arena => arena;
    public long Observed => observed;

    public IndexedBenchFixture(int count, string observers)
    {
        Register(observers);
        arena = new(context);
        ents = new EntPtrIdx[count];

        for (var i = 0; i < count; i++)
        {
            var ent = arena.Alloc();
            ent.Set<int, BenchPlain>(1);
            ent.Set<int, BenchScalar>(1);
            ent.Set<int[], BenchArray>(array);
            ent.Set<bool, BenchGate>(true);
            ent.Set<bool, BenchActive>(true);
            ent.Set<int, BenchKey>(i + 1);
            ent.Set<BenchWideValue, BenchWide>(new(1, 2, 3, 4, 5, 6, 7, 8));
            ents[i] = ent;
        }
    }

    private void Register(string observers)
    {
        if (observers == "dirty")
        {
            context.AddBag(dirty);
            context.OnChange<int, BenchScalar>(TrackScalar);
            context.OnWrite<int[], BenchArray>(TrackArray);
        }

        if (observers == "bags")
        {
            context.AddBag(dirty);
            context.AddGatedBag(active);
        }

        if (observers == "key")
            context.AddIndex(RemoveKey).OnChange<int, BenchKey>(UpdateKey);

        if (observers == "wide")
            context.OnWrite<BenchWideValue, BenchWide>(ObserveWide);

        if (observers == "wide-change")
            context.OnChange<BenchWideValue, BenchWide>(ObserveWideChange);
    }

    private void TrackScalar(EntMutIdx ent, in EntChange<int> write)
    {
        ent.Set<bool, BenchDirty>(true);
    }

    private void TrackArray(EntMutIdx ent) => ent.Set<bool, BenchDirty>(true);

    private void UpdateKey(EntMutIdx ent, in EntChange<int> write)
    {
        if (write.Before == write.After)
            return;

        if (write.Before != 0)
            keys.Remove(write.Before);

        if (write.After != 0)
            keys.Add(write.After, ent);
    }

    private void RemoveKey(EntMutIdx ent) => keys.Remove(ent.Get<int, BenchKey>());

    private void ObserveWide(EntMutIdx ent) => observed += ent.Get<BenchWideValue, BenchWide>().H;

    private void ObserveWideChange(EntMutIdx ent, in EntChange<BenchWideValue> change) =>
        observed += change.After.H - change.Before.H;

    public void Dispose()
    {
        arena.Dispose();
        context.Dispose();
    }
}
