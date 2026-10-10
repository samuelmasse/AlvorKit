namespace AlvorKit;

internal static class RaceAlvorUpdate3Measurements
{
    internal static BenchResult DenseHandles(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3DenseHandles.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Query(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3Query.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Rows(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3Rows.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QuerySIMD(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3QuerySIMD.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
