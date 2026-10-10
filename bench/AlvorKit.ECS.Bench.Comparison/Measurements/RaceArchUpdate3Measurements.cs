namespace AlvorKit;

internal static class RaceArchUpdate3Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        using var fixture = new RaceArchUpdate3.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate3Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchUpdate3.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate3ScalarSourceGenerated.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
