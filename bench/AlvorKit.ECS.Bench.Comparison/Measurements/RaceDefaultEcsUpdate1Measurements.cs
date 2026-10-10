namespace AlvorKit;

internal static class RaceDefaultEcsUpdate1Measurements
{
    internal static BenchResult ComponentSystemScalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate1ComponentSystemScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult EntitySetSystemScalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate1EntitySetSystemScalar.Run(fixture, count, 64);
        var result = timer.Stop(count * 64, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
