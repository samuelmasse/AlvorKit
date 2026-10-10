namespace AlvorKit;

internal static class RaceFennecsUpdate3Measurements
{
    internal static BenchResult ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate3ForEach.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate3Raw.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
