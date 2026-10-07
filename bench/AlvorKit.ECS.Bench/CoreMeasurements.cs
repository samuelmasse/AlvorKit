namespace AlvorKit;

[Bench]
public class CoreMeasurements
{
    private const int Count = 16384;
    private const int Passes = 1024;
    private const int VectorPasses = 8192;
    private const int CreationCount = 1048576;

    private int retained;

    public BenchResult SparseSet()
    {
        using var arena = new EntArena();
        var ents = Prepare(arena);
        var timer = BenchTimer.Start();
        CoreSparseSet.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "Ent");
        retained = ents[^1].First + ents[^1].SparseFirst;
        return result;
    }

    public BenchResult Handles()
    {
        using var arena = new EntArena();
        var ents = Prepare(arena);
        var timer = BenchTimer.Start();
        CoreHandles.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "Ent");
        retained = ents[^1].First + ents[^1].SparseFirst;
        return result;
    }

    public BenchResult Rows()
    {
        using var arena = new EntArena();
        var ents = Prepare(arena);
        var timer = BenchTimer.Start();
        CoreRows.Run(arena, Passes);
        var result = timer.Stop(Count * Passes, "Ent");
        retained = ents[^1].First + ents[^1].SparseFirst;
        return result;
    }

    public BenchResult Spans()
    {
        using var arena = new EntArena();
        var ents = Prepare(arena);
        var timer = BenchTimer.Start();
        CoreSpans.Run(arena, Passes);
        var result = timer.Stop(Count * Passes, "Ent");
        retained = ents[^1].First + ents[^1].SparseFirst;
        return result;
    }

    public BenchResult Simd()
    {
        using var arena = new EntArena();
        var ents = Prepare(arena);
        var timer = BenchTimer.Start();
        CoreSimd.Run(arena, VectorPasses);
        var result = timer.Stop(Count * VectorPasses, "Ent");
        retained = ents[^1].First + ents[^1].SparseFirst;
        return result;
    }

    public BenchResult CreateSparse()
    {
        using var arena = new EntArena();
        var timer = BenchTimer.Start();
        CoreCreateSparse.Run(arena, CreationCount);
        var result = timer.Stop(CreationCount, "Ent");
        retained = arena.Allocated;
        return result;
    }

    public BenchResult CreateSetters()
    {
        using var arena = new EntArena();
        var timer = BenchTimer.Start();
        CoreCreateSetters.Run(arena, CreationCount);
        var result = timer.Stop(CreationCount, "Ent");
        retained = arena.Allocated;
        return result;
    }

    public BenchResult CreateShape()
    {
        using var arena = new EntArena();
        var timer = BenchTimer.Start();
        CoreCreateShape.Run(arena, CreationCount);
        var result = timer.Stop(CreationCount, "Ent");
        retained = arena.Allocated;
        return result;
    }

    private static EntPtr[] Prepare(EntArena arena)
    {
        var ents = new EntPtr[Count];

        for (var i = 0; i < Count; i++)
        {
            var ent = arena.AllocArchetypal<CoreBenchComponents>()
                .With<int, CoreBenchComponents.First>(i)
                .With<int, CoreBenchComponents.Second>(2)
                .With<int, CoreBenchComponents.Third>(3)
                .Create();
            ent.SparseFirst = i;
            ents[i] = ent;
        }

        return ents;
    }
}
