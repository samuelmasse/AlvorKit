namespace AlvorKit;

[Bench]
public class ArchRowsMeasurements
{
    private object? retained;

    public BenchResult HotRowGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, true);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotRowGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotRowSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, true);
        var timer = BenchTimer.Start();
        AlvorKit.HotRowSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }
}
