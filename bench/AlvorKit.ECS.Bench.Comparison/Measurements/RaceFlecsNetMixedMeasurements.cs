namespace AlvorKit;

internal static class RaceFlecsNetMixedMeasurements
{
    internal static BenchResult Each(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var timer = BenchTimer.Start();
        RaceFlecsNetMixedEach.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Iter(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var timer = BenchTimer.Start();
        RaceFlecsNetMixedIter.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
