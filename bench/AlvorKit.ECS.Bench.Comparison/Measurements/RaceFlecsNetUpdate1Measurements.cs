namespace AlvorKit;

internal static class RaceFlecsNetUpdate1Measurements
{
    internal static BenchResult Each(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate1.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate1Each.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Iter(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate1.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate1Iter.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
