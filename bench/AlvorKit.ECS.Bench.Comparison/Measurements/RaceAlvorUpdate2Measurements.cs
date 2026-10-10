namespace AlvorKit;

internal static class RaceAlvorUpdate2Measurements
{
    internal static BenchResult DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2DenseHandles.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2Query.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QueryUnrolled4(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2QueryUnrolled4.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QueryUnrolled8(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2QueryUnrolled8.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2Rows.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2QuerySIMD.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
