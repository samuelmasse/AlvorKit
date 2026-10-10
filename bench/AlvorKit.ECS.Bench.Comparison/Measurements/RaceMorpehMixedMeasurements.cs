namespace AlvorKit;

internal static class RaceMorpehMixedMeasurements
{
    internal static BenchResult Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var timer = BenchTimer.Start();
        RaceMorpehMixedDirect.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var timer = BenchTimer.Start();
        RaceMorpehMixedStash.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
