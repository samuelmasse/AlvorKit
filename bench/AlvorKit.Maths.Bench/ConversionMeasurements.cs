namespace AlvorKit;

[Bench]
public class ConversionMeasurements
{
    private object? retained;

    public BenchResult Vec3dToVec3CastVec3dToVec3()
    {
        var fixture = new ConversionFixture();
        var doubles = fixture.Doubles;
        var floatOutput = fixture.FloatOutput;
        var timer = BenchTimer.Start();
        ConversionVec3dToVec3CastVec3dToVec3.Run(doubles, floatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3dToVec3ManualVec3dToVec3()
    {
        var fixture = new ConversionFixture();
        var doubles = fixture.Doubles;
        var floatOutput = fixture.FloatOutput;
        var timer = BenchTimer.Start();
        ConversionVec3dToVec3ManualVec3dToVec3.Run(doubles, floatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3ToVec3dCastVec3ToVec3d()
    {
        var fixture = new ConversionFixture();
        var floats = fixture.Floats;
        var doubleOutput = fixture.DoubleOutput;
        var timer = BenchTimer.Start();
        ConversionVec3ToVec3dCastVec3ToVec3d.Run(floats, doubleOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3ToVec3dManualVec3ToVec3d()
    {
        var fixture = new ConversionFixture();
        var floats = fixture.Floats;
        var doubleOutput = fixture.DoubleOutput;
        var timer = BenchTimer.Start();
        ConversionVec3ToVec3dManualVec3ToVec3d.Run(floats, doubleOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2iToVec2uCastVec2iToVec2u()
    {
        var fixture = new ConversionFixture();
        var ints = fixture.Ints;
        var uintOutput = fixture.UintOutput;
        var timer = BenchTimer.Start();
        ConversionVec2iToVec2uCastVec2iToVec2u.Run(ints, uintOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2iToVec2uManualVec2iToVec2u()
    {
        var fixture = new ConversionFixture();
        var ints = fixture.Ints;
        var uintOutput = fixture.UintOutput;
        var timer = BenchTimer.Start();
        ConversionVec2iToVec2uManualVec2iToVec2u.Run(ints, uintOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
