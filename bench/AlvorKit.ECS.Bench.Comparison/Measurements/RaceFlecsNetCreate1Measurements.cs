namespace AlvorKit;

internal static class RaceFlecsNetCreate1Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetBaseContext();
        var timer = BenchTimer.Start();
        RaceFlecsNetCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
