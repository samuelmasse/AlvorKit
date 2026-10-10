namespace AlvorKit;

internal static class RaceDefaultEcsMixedMeasurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        using var fixture = new RaceDefaultEcsMixed.DefaultEcsContext(count);
        var timer = BenchTimer.Start();
        RaceDefaultEcsMixedScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
