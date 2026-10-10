namespace AlvorKit;

internal static class RaceFennecsUpdate3Measurements
{
    internal static BenchResult ForEach(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate3ForEach.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate3Raw.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
