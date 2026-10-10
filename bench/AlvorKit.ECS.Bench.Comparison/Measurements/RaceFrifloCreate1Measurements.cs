namespace AlvorKit;

internal static class RaceFrifloCreate1Measurements
{
    internal static BenchResult Default(int count, int padding)
    {
        var fixture = new RaceFrifloCreateContext();
        var timer = BenchTimer.Start();
        RaceFrifloCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
