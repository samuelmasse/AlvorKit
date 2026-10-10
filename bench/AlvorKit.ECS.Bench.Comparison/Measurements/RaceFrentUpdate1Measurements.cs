namespace AlvorKit;

internal static class RaceFrentUpdate1Measurements
{
    internal static BenchResult QueryInline(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate1QueryInline.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult QueryDelegate(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate1QueryDelegate.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Simd(int count, int padding, int passes)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFrentUpdate1Simd.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
