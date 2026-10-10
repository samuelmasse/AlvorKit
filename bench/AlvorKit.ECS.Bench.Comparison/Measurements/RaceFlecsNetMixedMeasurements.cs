namespace AlvorKit;

internal static class RaceFlecsNetMixedMeasurements
{
    internal static BenchResult Each(int count, int padding)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var timer = BenchTimer.Start();
        RaceFlecsNetMixedEach.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Iter(int count, int padding)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var timer = BenchTimer.Start();
        RaceFlecsNetMixedIter.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
