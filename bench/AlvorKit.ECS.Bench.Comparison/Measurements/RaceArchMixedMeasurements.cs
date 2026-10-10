namespace AlvorKit;

internal static class RaceArchMixedMeasurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var timer = BenchTimer.Start();
        RaceArchMixedDefault.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ScalarSourceGenerated(int count, int padding, int passes)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var timer = BenchTimer.Start();
        RaceArchMixedScalarSourceGenerated.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
