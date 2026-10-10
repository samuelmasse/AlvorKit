namespace AlvorKit;

internal static class RaceSveltoUpdate3Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceSveltoUpdate3.SveltoECSContext(count, padding);
        var timer = BenchTimer.Start();
        RaceSveltoUpdate3Default.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
