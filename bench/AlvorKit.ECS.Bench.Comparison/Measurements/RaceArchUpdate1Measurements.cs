namespace AlvorKit;

internal static class RaceArchUpdate1Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        using var fixture = new RaceArchUpdate1.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate1Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchUpdate1.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate1ScalarSourceGenerated.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
