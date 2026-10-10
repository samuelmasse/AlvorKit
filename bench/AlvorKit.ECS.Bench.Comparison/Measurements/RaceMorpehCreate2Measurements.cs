namespace AlvorKit;

internal static class RaceMorpehCreate2Measurements
{
    internal static BenchResult Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        var timer = BenchTimer.Start();
        RaceMorpehCreate2Direct.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        var timer = BenchTimer.Start();
        RaceMorpehCreate2Stash.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
