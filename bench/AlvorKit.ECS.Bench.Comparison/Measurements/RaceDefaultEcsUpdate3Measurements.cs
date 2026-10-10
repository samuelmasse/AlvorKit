namespace AlvorKit;

internal static class RaceDefaultEcsUpdate3Measurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        using var fixture = new RaceDefaultEcsUpdate3.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate3Scalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
