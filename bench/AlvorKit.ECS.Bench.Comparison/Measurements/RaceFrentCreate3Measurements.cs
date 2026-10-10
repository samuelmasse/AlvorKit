namespace AlvorKit;

internal static class RaceFrentCreate3Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentBaseContext();
        var timer = BenchTimer.Start();
        RaceFrentCreate3Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
