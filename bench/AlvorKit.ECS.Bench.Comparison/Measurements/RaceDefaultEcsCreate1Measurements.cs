namespace AlvorKit;

internal static class RaceDefaultEcsCreate1Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsBaseContext();
        var timer = BenchTimer.Start();
        RaceDefaultEcsCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
