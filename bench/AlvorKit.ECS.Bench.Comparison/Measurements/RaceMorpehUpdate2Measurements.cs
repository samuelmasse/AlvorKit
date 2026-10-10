namespace AlvorKit;

internal static class RaceMorpehUpdate2Measurements
{
    internal static BenchResult Direct(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate2.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate2Direct.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehUpdate2.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate2Stash.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
