namespace AlvorKit;

[Bench]
public class EqualityComparerMeasurements
{
    private object? retained;

    public BenchResult Vec2iDirectVec2i()
    {
        var fixture = new EqualityComparerFixture();
        var left2 = fixture.Left2;
        var right2 = fixture.Right2;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        EqualityComparerVec2iDirectVec2i.Run(left2, right2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2iComparerVec2i()
    {
        var fixture = new EqualityComparerFixture();
        var left2 = fixture.Left2;
        var right2 = fixture.Right2;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        EqualityComparerVec2iComparerVec2i.Run(left2, right2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3iDirectVec3i()
    {
        var fixture = new EqualityComparerFixture();
        var left3 = fixture.Left3;
        var right3 = fixture.Right3;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        EqualityComparerVec3iDirectVec3i.Run(left3, right3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3iComparerVec3i()
    {
        var fixture = new EqualityComparerFixture();
        var left3 = fixture.Left3;
        var right3 = fixture.Right3;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        EqualityComparerVec3iComparerVec3i.Run(left3, right3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
