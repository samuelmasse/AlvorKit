namespace AlvorKit;

internal static class RaceFlecsNetCreate3Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFlecsNetBaseContext();
        var timer = BenchTimer.Start();
        RaceFlecsNetCreate3Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
