namespace AlvorKit;

internal static class RaceFennecsMixedMeasurements
{
    internal static BenchResult ForEach(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var timer = BenchTimer.Start();
        RaceFennecsMixedForEach.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var timer = BenchTimer.Start();
        RaceFennecsMixedRaw.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
