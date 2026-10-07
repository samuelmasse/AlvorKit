namespace AlvorKit;

[Bench]
public class VectorLeafMeasurements
{
    private object? retained;

    public BenchResult AbsAlvorAbs()
    {
        var fixture = new VectorLeafFixture();
        var alvorInput = fixture.AlvorInput;
        var alvorOutput = fixture.AlvorOutput;
        var timer = BenchTimer.Start();
        VectorLeafAbsAlvorAbs.Run(alvorInput, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult AbsNumericsAbs()
    {
        var fixture = new VectorLeafFixture();
        var systemInput = fixture.SystemInput;
        var systemOutput = fixture.SystemOutput;
        var timer = BenchTimer.Start();
        VectorLeafAbsNumericsAbs.Run(systemInput, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ClampAlvorClamp()
    {
        var fixture = new VectorLeafFixture();
        var alvorInput = fixture.AlvorInput;
        var alvorOutput = fixture.AlvorOutput;
        var timer = BenchTimer.Start();
        VectorLeafClampAlvorClamp.Run(alvorInput, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ClampNumericsClamp()
    {
        var fixture = new VectorLeafFixture();
        var systemInput = fixture.SystemInput;
        var systemOutput = fixture.SystemOutput;
        var timer = BenchTimer.Start();
        VectorLeafClampNumericsClamp.Run(systemInput, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
