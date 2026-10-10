namespace AlvorKit;

internal static class RaceDefaultEcsUpdate2Measurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        using var fixture = new RaceDefaultEcsUpdate2.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate2Scalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
