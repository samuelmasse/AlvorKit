const int EntCount = 16_384;
const int PassCount = 256;
const int SampleCount = 7;

if (args.Length == 1 && args[0] == "--many-arch")
    return AlvorKit.ECS.Demo.QueryBench.ManyArchQueryBench.Run();

#if DEBUG
Console.Error.WriteLine("This benchmark must run in Release mode: dotnet run -c Release");
return 1;
#else
using var state = new QueryBenchState(EntCount);
state.WarmUp();

long expectedSum = (long)EntCount * (EntCount + 1) / 2 * PassCount;
QueryBenchResult[] results =
[
    Measure("Sparse Get ordered", state.IterateSparse, expectedSum),
    Measure("Sparse Get shuffled", state.IterateSparseShuffled, expectedSum),
    Measure("Arch Get ordered", state.IterateArchetypal, expectedSum),
    Measure("Arch Get shuffled", state.IterateArchetypalShuffled, expectedSum),
    Measure("Archetypal spans", state.IterateSpans, expectedSum),
    Measure("Archetypal rows", state.IterateRows, expectedSum),
    Measure("Archetypal spans x2", state.IterateWideSpans2, expectedSum * 2),
    Measure("Archetypal rows x2", state.IterateWideRows2, expectedSum * 2),
    Measure("Archetypal spans x8", state.IterateWideSpans8, expectedSum * 8),
    Measure("Archetypal rows x8", state.IterateWideRows8, expectedSum * 8),
];

double sparseNs = results[0].NanosecondsPerEnt;
Console.WriteLine("AlvorKit ECS iteration comparison");
Console.WriteLine($"{EntCount:N0} Ents, {PassCount} passes, median of {SampleCount} short samples");
Console.WriteLine();
Console.WriteLine($"{"Path",-22} {"ns / Ent",12} {"vs sparse",12} {"loop alloc",12}");
foreach (QueryBenchResult result in results)
{
    Console.WriteLine(
        $"{result.Name,-22} {result.NanosecondsPerEnt,12:F3} {result.NanosecondsPerEnt / sparseNs,11:F2}x {result.AllocatedBytes,10} B");
}

Console.WriteLine();
Console.WriteLine("Shuffled cases use one deterministic Ent order created before measurement. Span and row paths retain dense arch row order.");
return 0;

// Measure one iteration path with short samples and validate its accumulated result.
QueryBenchResult Measure(string name, Func<int, long> body, long expected)
{
    var ticks = new long[SampleCount];
    long allocatedBytes = 0;

    for (int sample = 0; sample < SampleCount; sample++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        long started = Stopwatch.GetTimestamp();
        long sum = body(PassCount);
        ticks[sample] = Stopwatch.GetTimestamp() - started;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        if (sum != expected)
            throw new InvalidOperationException($"{name} produced {sum}, expected {expected}.");

        allocatedBytes = Math.Max(allocatedBytes, allocated);
    }

    Array.Sort(ticks);
    long operations = (long)EntCount * PassCount;
    double nanosecondsPerEnt = ticks[SampleCount / 2] * 1_000_000_000d / Stopwatch.Frequency / operations;
    return new(name, nanosecondsPerEnt, allocatedBytes);
}
#endif

/// <summary>Owns equivalent sparse and archetypal data sets for the iteration comparison.</summary>
internal sealed class QueryBenchState : IDisposable
{
    /// <summary>The alloc whose rows and columns are exercised by every benchmark path.</summary>
    private readonly EntArena arena = new();

    /// <summary>The stable allocation order used by the ordered point-access paths.</summary>
    private readonly EntMut[] ents;

    /// <summary>A deterministic shuffled view of the same Ents used by randomized point access.</summary>
    private readonly EntMut[] shuffledEnts;

    /// <summary>Creates matching sparse and archetypal values for the requested Ent count.</summary>
    internal QueryBenchState(int entCount)
    {
        ents = new EntMut[entCount];
        for (int i = 0; i < ents.Length; i++)
        {
            EntMut ent = arena.Alloc();
            int value = i + 1;
            ent.Set<int, SparseValue>(value);
            ent.SetArchetypal<int, ArchValue, QueryArch>(value);
            ent.SetArchetypal<int, W0, WideQueryArch>(value);
            ent.SetArchetypal<int, W1, WideQueryArch>(value);
            ent.SetArchetypal<int, W2, WideQueryArch>(value);
            ent.SetArchetypal<int, W3, WideQueryArch>(value);
            ent.SetArchetypal<int, W4, WideQueryArch>(value);
            ent.SetArchetypal<int, W5, WideQueryArch>(value);
            ent.SetArchetypal<int, W6, WideQueryArch>(value);
            ent.SetArchetypal<int, W7, WideQueryArch>(value);
            ents[i] = ent;
        }

        shuffledEnts = (EntMut[])ents.Clone();
        var random = new Random(0x5EED);
        random.Shuffle(shuffledEnts);
    }

    /// <summary>Runs every path enough times to JIT its steady-state loop before measurement.</summary>
    internal void WarmUp()
    {
        for (int i = 0; i < 32; i++)
        {
            IterateSparse(1);
            IterateSparseShuffled(1);
            IterateArchetypal(1);
            IterateArchetypalShuffled(1);
            IterateSpans(1);
            IterateRows(1);
            IterateWideSpans2(1);
            IterateWideRows2(1);
            IterateWideSpans8(1);
            IterateWideRows8(1);
        }
    }

    /// <summary>Reads sparse values in allocation order.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateSparse(int passes)
    {
        long sum = 0;
        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < ents.Length; i++)
                sum += ents[i].Get<int, SparseValue>();
        }

        return sum;
    }

    /// <summary>Reads sparse values in deterministic shuffled order.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateSparseShuffled(int passes)
    {
        long sum = 0;
        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < shuffledEnts.Length; i++)
                sum += shuffledEnts[i].Get<int, SparseValue>();
        }

        return sum;
    }

    /// <summary>Reads archetypal values through per-Ent point access in allocation order.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateArchetypal(int passes)
    {
        long sum = 0;
        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < ents.Length; i++)
                sum += ents[i].GetArchetypal<int, ArchValue, QueryArch>();
        }

        return sum;
    }

    /// <summary>Reads archetypal values through per-Ent point access in deterministic shuffled order.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateArchetypalShuffled(int passes)
    {
        long sum = 0;
        for (int pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < shuffledEnts.Length; i++)
                sum += shuffledEnts[i].GetArchetypal<int, ArchValue, QueryArch>();
        }

        return sum;
    }

    /// <summary>Reads archetypal values through aligned dense component spans.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateSpans(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<QueryArch>().With<int, ArchValue>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> values = chunk.Get<int, ArchValue>();
                for (int i = 0; i < values.Length; i++)
                    sum += values[i];
            }
        }

        return sum;
    }

    /// <summary>Reads archetypal values through generated one-Ent-at-a-time rows.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateRows(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<QueryArch>().With<int, ArchValue>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var row in query.Rows())
                sum += row.ArchValue;
        }

        return sum;
    }

    /// <summary>Reads two aligned archetypal columns through direct spans.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateWideSpans2(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>()
            .With<int, W0>()
            .With<int, W1>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> first = chunk.Get<int, W0>();
                Span<int> second = chunk.Get<int, W1>();
                for (int i = 0; i < first.Length; i++)
                    sum += first[i] + second[i];
            }
        }

        return sum;
    }

    /// <summary>Reads two aligned archetypal columns through generated rows.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateWideRows2(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>()
            .With<int, W0>()
            .With<int, W1>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var row in query.Rows())
                sum += row.W0 + row.W1;
        }

        return sum;
    }

    /// <summary>Reads eight aligned archetypal columns through direct spans.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateWideSpans8(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>()
            .With<int, W0>()
            .With<int, W1>()
            .With<int, W2>()
            .With<int, W3>()
            .With<int, W4>()
            .With<int, W5>()
            .With<int, W6>()
            .With<int, W7>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
            {
                Span<int> c0 = chunk.Get<int, W0>();
                Span<int> c1 = chunk.Get<int, W1>();
                Span<int> c2 = chunk.Get<int, W2>();
                Span<int> c3 = chunk.Get<int, W3>();
                Span<int> c4 = chunk.Get<int, W4>();
                Span<int> c5 = chunk.Get<int, W5>();
                Span<int> c6 = chunk.Get<int, W6>();
                Span<int> c7 = chunk.Get<int, W7>();
                for (int i = 0; i < c0.Length; i++)
                    sum += c0[i] + c1[i] + c2[i] + c3[i] + c4[i] + c5[i] + c6[i] + c7[i];
            }
        }

        return sum;
    }

    /// <summary>Reads eight aligned archetypal columns through generated rows.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal long IterateWideRows8(int passes)
    {
        long sum = 0;
        var query = arena.QueryArchetypal<WideQueryArch>()
            .With<int, W0>()
            .With<int, W1>()
            .With<int, W2>()
            .With<int, W3>()
            .With<int, W4>()
            .With<int, W5>()
            .With<int, W6>()
            .With<int, W7>();
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var row in query.Rows())
            {
                sum += row.W0 + row.W1 + row.W2 + row.W3;
                sum += row.W4 + row.W5 + row.W6 + row.W7;
            }
        }

        return sum;
    }

    /// <summary>Releases the benchmark alloc and its archetypal storage.</summary>
    public void Dispose() => arena.Dispose();

}

/// <summary>Reports the median cost and maximum measured loop allocation for one iteration path.</summary>
internal readonly record struct QueryBenchResult(string Name, double NanosecondsPerEnt, long AllocatedBytes);

/// <summary>Names the sparse value component.</summary>
internal readonly record struct SparseValue;

/// <summary>Names the archetypal value component.</summary>
internal readonly record struct ArchValue;

/// <summary>Identifies the archetypal component group.</summary>
internal readonly record struct QueryArch;

/// <summary>Identifies the wide archetypal component group.</summary>
internal readonly record struct WideQueryArch;

/// <summary>Names wide query column zero.</summary>
internal readonly record struct W0;

/// <summary>Names wide query column one.</summary>
internal readonly record struct W1;

/// <summary>Names wide query column two.</summary>
internal readonly record struct W2;

/// <summary>Names wide query column three.</summary>
internal readonly record struct W3;

/// <summary>Names wide query column four.</summary>
internal readonly record struct W4;

/// <summary>Names wide query column five.</summary>
internal readonly record struct W5;

/// <summary>Names wide query column six.</summary>
internal readonly record struct W6;

/// <summary>Names wide query column seven.</summary>
internal readonly record struct W7;
