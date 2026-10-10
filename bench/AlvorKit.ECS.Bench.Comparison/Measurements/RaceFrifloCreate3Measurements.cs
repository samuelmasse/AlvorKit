namespace AlvorKit;

internal static class RaceFrifloCreate3Measurements
{
    internal static BenchResult Default(int count, int padding, int passes)
    {
        var fixture = new RaceFrifloCreateContext();
        var timer = BenchTimer.Start();
        RaceFrifloCreate3Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
