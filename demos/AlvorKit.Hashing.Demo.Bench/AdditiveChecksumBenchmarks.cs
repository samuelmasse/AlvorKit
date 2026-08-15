namespace AlvorKit;

/// <summary>Measures additive checksum helper overhead against direct addition.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AdditiveChecksumBenchmarks
{
    private const int OperationCount = 4_096;

    private readonly ulong[] values = new ulong[OperationCount];

    /// <summary>Builds deterministic input values outside measured work.</summary>
    [GlobalSetup]
    public void Setup()
    {
        for (var index = 0; index < values.Length; index++)
            values[index] = (ulong)(index * 17);
    }

    /// <summary>Accumulates through <see cref="AdditiveChecksum64"/>.</summary>
    [Benchmark]
    public ulong Helper()
    {
        AdditiveChecksum64 checksum = default;
        for (var index = 0; index < values.Length; index++)
            checksum.Add(values[index]);
        return checksum.Value;
    }

    /// <summary>Accumulates with direct addition.</summary>
    [Benchmark(Baseline = true)]
    public ulong Inline()
    {
        ulong checksum = 0;
        for (var index = 0; index < values.Length; index++)
            checksum += values[index];
        return checksum;
    }
}
