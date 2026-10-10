namespace AlvorKit;

internal static class RaceFrentUpdate3Measurements
{
    internal static BenchResult QueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3QueryInline.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QueryDelegate(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3QueryDelegate.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Simd(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3Simd.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
