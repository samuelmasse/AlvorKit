namespace AlvorKit;

[Bench]
public class QueryMeasurements
{
    private long retained;

    public BenchResult Sparse()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QuerySparse.Run(fixture.Ents, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult SparseShuffled()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QuerySparseShuffled.Run(fixture.ShuffledEnts, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult Archetypal()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryArchetypal.Run(fixture.Ents, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult ArchetypalShuffled()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryArchetypalShuffled.Run(fixture.ShuffledEnts, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult Spans()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QuerySpans.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult Rows()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryRows.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult WideSpans2()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryWideSpans2.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult WideRows2()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryWideRows2.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult WideSpans8()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryWideSpans8.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult WideRows8()
    {
        using var fixture = new QueryFixture(16384);
        var timer = BenchTimer.Start();
        var sum = QueryWideRows8.Run(fixture.Arena, 256);
        var result = timer.Stop(16384 * 256, "Ent");
        retained = sum;
        return result;
    }

    public BenchResult ManyArch()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        ManyArchFixture.MaterializeArchs(ent);
        ManyArchFixture.EnterMeasuredArch(ent);
        var timer = BenchTimer.Start();
        var count = ManyArchDiscovery.Run(arena, 200000);
        var result = timer.Stop(200000, "query");
        retained = count;
        return result;
    }
}
