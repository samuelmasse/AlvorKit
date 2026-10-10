namespace AlvorKit;

internal static class RaceFrifloMixedMeasurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var timer = BenchTimer.Start();
        RaceFrifloMixedScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult SIMDScalar(int count, int padding, int passes)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var timer = BenchTimer.Start();
        RaceFrifloMixedSIMDScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
