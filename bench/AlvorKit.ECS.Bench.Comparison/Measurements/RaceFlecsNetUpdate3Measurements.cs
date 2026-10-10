namespace AlvorKit;

internal static class RaceFlecsNetUpdate3Measurements
{
    internal static BenchResult Each(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate3.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate3Each.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Iter(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate3.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate3Iter.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
