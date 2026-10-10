namespace AlvorKit;

internal static class RaceFlecsNetUpdate2Measurements
{
    internal static BenchResult Each(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate2.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate2Each.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Iter(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetUpdate2.FlecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFlecsNetUpdate2Iter.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
