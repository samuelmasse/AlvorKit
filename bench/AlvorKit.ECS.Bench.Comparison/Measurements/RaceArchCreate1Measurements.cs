namespace AlvorKit;

internal static class RaceArchCreate1Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceArchBaseContext();
        var timer = BenchTimer.Start();
        RaceArchCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
