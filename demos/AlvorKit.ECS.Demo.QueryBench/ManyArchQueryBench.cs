namespace AlvorKit.ECS.Demo.QueryBench;

/// <summary>Measures repeated query discovery after a group has materialized many sparsely active archs.</summary>
internal static class ManyArchQueryBench
{
    private const int FieldCount = 11;
    private const int ArchCount = (1 << FieldCount) - 1;
    private const int PassCount = 20_000;
    private const int SampleCount = 7;

    /// <summary>Runs the opt-in many-arch query benchmark.</summary>
    internal static int Run()
    {
#if DEBUG
        Console.Error.WriteLine("This benchmark must run in Release mode: dotnet run -c Release -- --many-arch");
        return 1;
#else
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        MaterializeArchs(ent);
        EnterMeasuredArch(ent);

        var query = arena.QueryArchetypal<ManyArch>()
            .With<int, C0>()
            .With<int, C1>()
            .With<int, C2>()
            .With<int, C3>();

        _ = CountRows(query, 1);
        var ticks = new long[SampleCount];
        long maxAllocatedBytes = 0;
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            long started = Stopwatch.GetTimestamp();
            int count = CountRows(query, PassCount);
            ticks[sample] = Stopwatch.GetTimestamp() - started;
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            maxAllocatedBytes = Math.Max(maxAllocatedBytes, allocatedBytes);

            if (count != PassCount)
                throw new InvalidOperationException($"Unexpected query count {count}; expected {PassCount}.");
        }

        Array.Sort(ticks);
        double nanoseconds = ticks[SampleCount / 2] * (1_000_000_000d / Stopwatch.Frequency);
        Console.WriteLine("AlvorKit ECS many-arch query discovery");
        Console.WriteLine($"{ArchCount:N0} materialized archs, 128 signature matches, 1 active match");
        Console.WriteLine($"{nanoseconds / PassCount:F2} ns / query, {maxAllocatedBytes} B loop allocation");
        return 0;
#endif
    }

    private static int CountRows<TSelect>(EntArchQuery<ManyArch, TSelect> query, int passes)
        where TSelect : struct, IEntArchSelect<ManyArch>
    {
        int count = 0;
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (var chunk in query)
                count += chunk.Ents.Length;
        }

        return count;
    }

    private static void MaterializeArchs(EntMut ent)
    {
        int previousMask = 0;
        for (int index = 1; index <= ArchCount; index++)
        {
            int mask = index ^ (index >> 1);
            int changed = previousMask ^ mask;
            int bit = BitOperations.TrailingZeroCount((uint)changed);
            SetBit(ent, bit, (mask & changed) != 0);
            previousMask = mask;
        }

        for (int bit = 0; bit < FieldCount; bit++)
        {
            if ((previousMask & (1 << bit)) != 0)
                SetBit(ent, bit, false);
        }
    }

    private static void EnterMeasuredArch(EntMut ent)
    {
        ent.SetArchetypal<int, C0, ManyArch>(1);
        ent.SetArchetypal<int, C1, ManyArch>(1);
        ent.SetArchetypal<int, C2, ManyArch>(1);
        ent.SetArchetypal<int, C3, ManyArch>(1);
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
            ent.SetArchetypal<int, N, ManyArch>(0);
        else ent.UnsetArchetypal<int, N, ManyArch>();
    }

    private readonly record struct ManyArch;
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
