namespace AlvorKit;

internal static class RaceFrentUpdate3Measurements
{
    internal static BenchResult QueryInline(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3QueryInline.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QueryDelegate(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3QueryDelegate.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Simd(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate3Simd.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
