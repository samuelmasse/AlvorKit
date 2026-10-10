namespace AlvorKit;

internal static class RaceSveltoMixedMeasurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceSveltoMixed.SveltoECSContext(count);
        var timer = BenchTimer.Start();
        RaceSveltoMixedDefault.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
