namespace AlvorKit;

internal static class RaceArchMixedMeasurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var timer = BenchTimer.Start();
        RaceArchMixedDefault.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var timer = BenchTimer.Start();
        RaceArchMixedScalarSourceGenerated.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
