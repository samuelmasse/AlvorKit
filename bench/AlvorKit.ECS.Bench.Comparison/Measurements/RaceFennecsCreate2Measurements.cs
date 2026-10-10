namespace AlvorKit;

internal static class RaceFennecsCreate2Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        using var fixture = new RaceFennecsBaseContext();
        var timer = BenchTimer.Start();
        RaceFennecsCreate2Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
