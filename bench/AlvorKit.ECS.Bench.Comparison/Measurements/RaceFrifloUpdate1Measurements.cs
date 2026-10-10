namespace AlvorKit;

internal static class RaceFrifloUpdate1Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate1.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate1Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate1.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate1SIMDScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
