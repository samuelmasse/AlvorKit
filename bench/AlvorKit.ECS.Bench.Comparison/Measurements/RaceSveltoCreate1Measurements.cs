namespace AlvorKit;

internal static class RaceSveltoCreate1Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceSveltoECSBaseContext();
        var timer = BenchTimer.Start();
        RaceSveltoCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
