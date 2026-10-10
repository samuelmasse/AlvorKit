namespace AlvorKit;

internal static class RaceMorpehUpdate3Measurements
{
    internal static BenchResult Direct(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate3.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate3Direct.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate3.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate3Stash.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
