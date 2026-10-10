namespace AlvorKit;

internal static class RaceSveltoMixedMeasurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceSveltoMixed.SveltoECSContext(count);
        var timer = BenchTimer.Start();
        RaceSveltoMixedDefault.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
