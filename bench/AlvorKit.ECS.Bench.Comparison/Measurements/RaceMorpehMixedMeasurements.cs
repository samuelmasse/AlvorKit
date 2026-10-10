namespace AlvorKit;

internal static class RaceMorpehMixedMeasurements
{
    internal static BenchResult Direct(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var timer = BenchTimer.Start();
        RaceMorpehMixedDirect.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding, int passes)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var timer = BenchTimer.Start();
        RaceMorpehMixedStash.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
