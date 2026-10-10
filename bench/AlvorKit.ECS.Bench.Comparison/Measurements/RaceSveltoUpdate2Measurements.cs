namespace AlvorKit;

internal static class RaceSveltoUpdate2Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceSveltoUpdate2.SveltoECSContext(count, padding);
        var timer = BenchTimer.Start();
        RaceSveltoUpdate2Default.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
