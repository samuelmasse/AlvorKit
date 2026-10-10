namespace AlvorKit;

internal static class RaceFrentCreate2Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentBaseContext();
        var timer = BenchTimer.Start();
        RaceFrentCreate2Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
