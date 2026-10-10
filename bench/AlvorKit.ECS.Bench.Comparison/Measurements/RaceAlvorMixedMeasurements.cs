namespace AlvorKit;

internal static class RaceAlvorMixedMeasurements
{
    internal static BenchResult DenseHandles(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var timer = BenchTimer.Start();
        RaceAlvorMixedDenseHandles.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Query(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var timer = BenchTimer.Start();
        RaceAlvorMixedQuery.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Rows(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var timer = BenchTimer.Start();
        RaceAlvorMixedRows.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QuerySIMD(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var timer = BenchTimer.Start();
        RaceAlvorMixedQuerySIMD.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
