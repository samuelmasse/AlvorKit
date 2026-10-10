namespace AlvorKit;

internal static class RaceSveltoUpdate1Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceSveltoUpdate1.SveltoECSContext(count, padding);
        var timer = BenchTimer.Start();
        RaceSveltoUpdate1Default.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
