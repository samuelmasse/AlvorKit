namespace AlvorKit;

internal static class RaceMorpehUpdate1Measurements
{
    internal static BenchResult Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate1Direct.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var timer = BenchTimer.Start();
        RaceMorpehUpdate1Stash.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
