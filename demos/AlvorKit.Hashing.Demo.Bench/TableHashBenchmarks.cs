namespace AlvorKit;

/// <summary>Integer key shapes supported by <see cref="TableHash"/>.</summary>
public enum TableHashShape
{
    Int32,
    Int32Pair,
    Int64,
    UInt64,
    UInt64Int32,
}

/// <summary>Measures helper-call overhead against the same table mixing inlined in the benchmark.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class TableHashBenchmarks
{
    private const int OperationCount = 4_096;
    private const uint Key32Factor = 2_654_435_761u;
    private const ulong PairSalt = 0x9E3779B97F4A7C15UL;
    private const ulong PairFactor = 0xD6E8FEB86659FD93UL;
    private const ulong MixFirstFactor = 0xBF58476D1CE4E5B9UL;
    private const ulong MixSecondFactor = 0x94D049BB133111EBUL;

    private readonly int[] intKeys = new int[OperationCount];
    private readonly int[] secondIntKeys = new int[OperationCount];
    private readonly long[] longKeys = new long[OperationCount];
    private readonly ulong[] ulongKeys = new ulong[OperationCount];

    /// <summary>Gets or sets the key shape measured by the next invocation.</summary>
    [ParamsAllValues]
    public TableHashShape Shape;

    /// <summary>Gets or sets the capacity-minus-one mask measured by the next invocation.</summary>
    [Params(15, 31, 63)]
    public int TableMask;

    /// <summary>Builds deterministic input arrays outside measured work.</summary>
    [GlobalSetup]
    public void Setup()
    {
        for (var index = 0; index < OperationCount; index++)
        {
            intKeys[index] = (index * 7919) - 1_000_003;
            secondIntKeys[index] = (index * 104729) + 17;
            longKeys[index] = ((long)intKeys[index] << 32) | (uint)secondIntKeys[index];
            ulongKeys[index] = Unsafe.BitCast<long, ulong>(longKeys[index]);
        }
    }

    /// <summary>Runs the selected helper overload.</summary>
    [Benchmark]
    public ulong Helper() => Shape switch
    {
        TableHashShape.Int32 => RunHelperInt32(),
        TableHashShape.Int32Pair => RunHelperInt32Pair(),
        TableHashShape.Int64 => RunHelperInt64(),
        TableHashShape.UInt64 => RunHelperUInt64(),
        TableHashShape.UInt64Int32 => RunHelperUInt64Int32(),
        _ => throw new UnreachableException(),
    };

    /// <summary>Runs the same selected mixing directly inside this assembly.</summary>
    [Benchmark(Baseline = true)]
    public ulong Inline() => Shape switch
    {
        TableHashShape.Int32 => RunInlineInt32(),
        TableHashShape.Int32Pair => RunInlineInt32Pair(),
        TableHashShape.Int64 => RunInlineInt64(),
        TableHashShape.UInt64 => RunInlineUInt64(),
        TableHashShape.UInt64Int32 => RunInlineUInt64Int32(),
        _ => throw new UnreachableException(),
    };

    private ulong RunHelperInt32()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)TableHash.Index(intKeys[index], TableMask);
        return checksum;
    }

    private ulong RunHelperInt32Pair()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)TableHash.Index(intKeys[index], secondIntKeys[index], TableMask);
        return checksum;
    }

    private ulong RunHelperInt64()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)TableHash.Index(longKeys[index], TableMask);
        return checksum;
    }

    private ulong RunHelperUInt64()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)TableHash.Index(ulongKeys[index], TableMask);
        return checksum;
    }

    private ulong RunHelperUInt64Int32()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)TableHash.Index(ulongKeys[index], secondIntKeys[index], TableMask);
        return checksum;
    }

    private ulong RunInlineInt32()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)InlineIndex(intKeys[index], TableMask);
        return checksum;
    }

    private ulong RunInlineInt32Pair()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)InlineIndex(intKeys[index], secondIntKeys[index], TableMask);
        return checksum;
    }

    private ulong RunInlineInt64()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)InlineIndex(Unsafe.BitCast<long, ulong>(longKeys[index]), TableMask);
        return checksum;
    }

    private ulong RunInlineUInt64()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)InlineIndex(ulongKeys[index], TableMask);
        return checksum;
    }

    private ulong RunInlineUInt64Int32()
    {
        ulong checksum = 0;
        for (var index = 0; index < OperationCount; index++)
            checksum += (uint)InlineIndex(ulongKeys[index], secondIntKeys[index], TableMask);
        return checksum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InlineIndex(int key, int mask)
    {
        var bits = Unsafe.BitCast<int, uint>(key);
        var mixed = (uint)(((ulong)bits * Key32Factor) & uint.MaxValue);
        return (int)(mixed & (uint)mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InlineIndex(int first, int second, int mask)
    {
        var firstBits = Unsafe.BitCast<int, uint>(first);
        var secondBits = Unsafe.BitCast<int, uint>(second);
        return Mask(Mix(((ulong)firstBits << 32) | secondBits), mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InlineIndex(ulong key, int mask) => Mask(Mix(key), mask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InlineIndex(ulong first, int second, int mask)
    {
        var secondBits = Unsafe.BitCast<int, uint>(second);
        return Mask(Mix(first ^ MultiplyLow(PairSalt + secondBits, PairFactor)), mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ulong value)
    {
        value = MultiplyLow(value ^ (value >> 30), MixFirstFactor);
        value = MultiplyLow(value ^ (value >> 27), MixSecondFactor);
        return value ^ (value >> 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MultiplyLow(ulong left, ulong right) => (ulong)(((UInt128)left * right) & ulong.MaxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Mask(ulong value, int mask) => (int)(value & (uint)mask);
}
