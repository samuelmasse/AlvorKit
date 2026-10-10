namespace AlvorKit;

internal static class RaceFennecsCreate3Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsBaseContext();
        var timer = BenchTimer.Start();
        RaceFennecsCreate3Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
