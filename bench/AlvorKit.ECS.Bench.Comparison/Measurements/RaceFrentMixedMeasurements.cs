namespace AlvorKit;

internal static class RaceFrentMixedMeasurements
{
    internal static BenchResult QueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var timer = BenchTimer.Start();
        RaceFrentMixedQueryInline.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Simd(int count, int padding)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var timer = BenchTimer.Start();
        RaceFrentMixedSimd.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
