namespace AlvorKit;

internal static class RaceSveltoUpdate3Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceSveltoUpdate3.SveltoECSContext(count, padding);
        var timer = BenchTimer.Start();
        RaceSveltoUpdate3Default.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
