namespace AlvorKit;

internal static class RaceAlvorUpdate3Measurements
{
    internal static BenchResult DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3DenseHandles.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3Query.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3Rows.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3QuerySIMD.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
