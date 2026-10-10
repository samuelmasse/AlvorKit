namespace AlvorKit;

internal static class RaceMorpehUpdate1Measurements
{
    internal static BenchResult Direct(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate1Direct.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate1Stash.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
