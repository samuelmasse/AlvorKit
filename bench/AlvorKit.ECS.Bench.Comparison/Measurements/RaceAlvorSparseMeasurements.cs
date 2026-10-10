namespace AlvorKit;

internal static class RaceAlvorSparseMeasurements
{
    internal static BenchResult Update1(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorSparseContext(count, padding, 1, mixed: false);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate1SparseHandles.Run(fixture, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Update2(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorSparseContext(count, padding, 2, mixed: false);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate2SparseHandles.Run(fixture, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Update3(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorSparseContext(count, padding, 3, mixed: false);
        var timer = BenchTimer.Start();
        RaceAlvorUpdate3SparseHandles.Run(fixture, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Mixed(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorSparseContext(count, padding, 2, mixed: true);
        var timer = BenchTimer.Start();
        RaceAlvorMixedSparseHandles.Run(fixture, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
