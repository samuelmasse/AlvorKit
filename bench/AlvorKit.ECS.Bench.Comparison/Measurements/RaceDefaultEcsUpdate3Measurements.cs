namespace AlvorKit;

internal static class RaceDefaultEcsUpdate3Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate3.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate3Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
