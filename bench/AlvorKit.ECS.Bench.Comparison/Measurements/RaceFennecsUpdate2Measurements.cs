namespace AlvorKit;

internal static class RaceFennecsUpdate2Measurements
{
    internal static BenchResult ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate2ForEach.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceFennecsUpdate2Raw.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
