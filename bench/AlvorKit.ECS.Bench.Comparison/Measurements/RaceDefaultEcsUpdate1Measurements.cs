namespace AlvorKit;

internal static class RaceDefaultEcsUpdate1Measurements
{
    internal static BenchResult ComponentSystemScalar(int count, int padding, int passes)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate1ComponentSystemScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult EntitySetSystemScalar(int count, int padding, int passes)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var timer = BenchTimer.Start();
        RaceDefaultEcsUpdate1EntitySetSystemScalar.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

}
