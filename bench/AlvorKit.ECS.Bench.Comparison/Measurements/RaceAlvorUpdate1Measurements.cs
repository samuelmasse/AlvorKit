namespace AlvorKit;

internal static class RaceAlvorUpdate1Measurements
{
    internal static BenchResult DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate1DenseHandles.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate1Query.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate1Rows.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate1QuerySIMD.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
