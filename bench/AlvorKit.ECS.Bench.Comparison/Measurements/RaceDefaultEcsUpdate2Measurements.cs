namespace AlvorKit;

internal static class RaceDefaultEcsUpdate2Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate2.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate2Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
