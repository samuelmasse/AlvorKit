namespace AlvorKit;

internal static class RaceFennecsMixedMeasurements
{
    internal static BenchResult ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var timer = BenchTimer.Start();
        RaceFennecsMixedForEach.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var timer = BenchTimer.Start();
        RaceFennecsMixedRaw.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
