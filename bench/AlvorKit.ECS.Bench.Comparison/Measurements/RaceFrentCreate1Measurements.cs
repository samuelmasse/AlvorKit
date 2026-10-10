namespace AlvorKit;

internal static class RaceFrentCreate1Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceFrentBaseContext();
        var timer = BenchTimer.Start();
        RaceFrentCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
