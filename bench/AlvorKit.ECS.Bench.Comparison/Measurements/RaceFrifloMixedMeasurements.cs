namespace AlvorKit;

internal static class RaceFrifloMixedMeasurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var timer = BenchTimer.Start();
        RaceFrifloMixedScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var timer = BenchTimer.Start();
        RaceFrifloMixedSIMDScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
