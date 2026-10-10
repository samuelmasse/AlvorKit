namespace AlvorKit;

internal static class RaceDefaultEcsMixedMeasurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsMixed.DefaultEcsContext(count);
        var timer = BenchTimer.Start();
        RaceDefaultEcsMixedScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
