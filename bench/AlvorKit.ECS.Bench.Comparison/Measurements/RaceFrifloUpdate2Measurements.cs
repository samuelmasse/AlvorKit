namespace AlvorKit;

internal static class RaceFrifloUpdate2Measurements
{
    internal static BenchResult Scalar(int count, int padding, int passes)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate2Scalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult SIMDScalar(int count, int padding, int passes)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrifloUpdate2SIMDScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
