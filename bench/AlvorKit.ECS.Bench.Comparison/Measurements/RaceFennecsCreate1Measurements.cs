namespace AlvorKit;

internal static class RaceFennecsCreate1Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsBaseContext();
        var timer = BenchTimer.Start();
        RaceFennecsCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
