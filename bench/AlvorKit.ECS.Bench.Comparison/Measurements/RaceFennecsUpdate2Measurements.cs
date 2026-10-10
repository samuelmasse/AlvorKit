namespace AlvorKit;

internal static class RaceFennecsUpdate2Measurements
{
    internal static BenchResult ForEach(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate2ForEach.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding, int passes)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate2Raw.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
