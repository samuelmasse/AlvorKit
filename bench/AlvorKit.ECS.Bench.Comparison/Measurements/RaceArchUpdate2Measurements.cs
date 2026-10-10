namespace AlvorKit;

internal static class RaceArchUpdate2Measurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        using var fixture = new RaceArchUpdate2.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate2Scalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ScalarSourceGenerated(int count, int padding, int passes)
    {
        using var fixture = new RaceArchUpdate2.ArchContext(count, padding);
        var timer = BenchTimer.Start();
        RaceArchUpdate2ScalarSourceGenerated.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
