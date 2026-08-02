namespace AlvorKit.Hashing.Demo.Bench;

/// <summary>Measures retained 32-bit epoch indexing against a retained dictionary.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EpochIndex32Benchmarks
{
    private EpochIndex32 epochIndex = null!;
    private Dictionary<int, int> dictionary = null!;
    private int[] keys = null!;

    /// <summary>Gets or sets the active mapping count.</summary>
    [Params(16, 256, 4_096)]
    public int Count;

    /// <summary>Builds retained collections and keys outside measured work.</summary>
    [GlobalSetup]
    public void Setup()
    {
        epochIndex = new(Count);
        dictionary = new(Count);
        keys = new int[Count];
        for (var index = 0; index < Count; index++)
            keys[index] = (index * 7919) - 1_000_003;
    }

    /// <summary>Begins, fills, and reads one epoch.</summary>
    [Benchmark]
    public long EpochIndex()
    {
        epochIndex.Begin();
        for (var index = 0; index < keys.Length; index++)
            epochIndex.GetOrAdd(keys[index], index, out _);

        long checksum = 0;
        for (var index = 0; index < keys.Length; index++)
        {
            epochIndex.TryGet(keys[index], out var slot);
            checksum += slot;
        }

        return checksum;
    }

    /// <summary>Clears, fills, and reads one retained dictionary.</summary>
    [Benchmark(Baseline = true)]
    public long Dictionary()
    {
        dictionary.Clear();
        for (var index = 0; index < keys.Length; index++)
            dictionary.Add(keys[index], index);

        long checksum = 0;
        for (var index = 0; index < keys.Length; index++)
            checksum += dictionary[keys[index]];
        return checksum;
    }
}

/// <summary>Measures retained 64-bit epoch indexing against a retained dictionary.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EpochIndex64Benchmarks
{
    private EpochIndex64 epochIndex = null!;
    private Dictionary<ulong, int> dictionary = null!;
    private ulong[] keys = null!;

    /// <summary>Gets or sets the active mapping count.</summary>
    [Params(16, 256, 4_096)]
    public int Count;

    /// <summary>Builds retained collections and keys outside measured work.</summary>
    [GlobalSetup]
    public void Setup()
    {
        epochIndex = new(Count);
        dictionary = new(Count);
        keys = new ulong[Count];
        for (var index = 0; index < Count; index++)
            keys[index] = ((ulong)(uint)(index * 7919) << 32) | (uint)(index * 104729);
    }

    /// <summary>Begins, fills, and reads one epoch.</summary>
    [Benchmark]
    public long EpochIndex()
    {
        epochIndex.Begin();
        for (var index = 0; index < keys.Length; index++)
            epochIndex.GetOrAdd(keys[index], index, out _);

        long checksum = 0;
        for (var index = 0; index < keys.Length; index++)
        {
            epochIndex.TryGet(keys[index], out var slot);
            checksum += slot;
        }

        return checksum;
    }

    /// <summary>Clears, fills, and reads one retained dictionary.</summary>
    [Benchmark(Baseline = true)]
    public long Dictionary()
    {
        dictionary.Clear();
        for (var index = 0; index < keys.Length; index++)
            dictionary.Add(keys[index], index);

        long checksum = 0;
        for (var index = 0; index < keys.Length; index++)
            checksum += dictionary[keys[index]];
        return checksum;
    }
}
