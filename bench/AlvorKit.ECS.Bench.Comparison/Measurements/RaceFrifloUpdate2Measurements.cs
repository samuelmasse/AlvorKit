namespace AlvorKit;

internal static class RaceFrifloUpdate2Measurements
{
    internal static BenchResult Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate2Scalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate2SIMDScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
