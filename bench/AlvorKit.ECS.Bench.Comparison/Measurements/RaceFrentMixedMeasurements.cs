namespace AlvorKit;

internal static class RaceFrentMixedMeasurements
{
    internal static BenchResult QueryInline(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var timer = BenchTimer.Start();
        RaceFrentMixedQueryInline.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Simd(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var timer = BenchTimer.Start();
        RaceFrentMixedSimd.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
