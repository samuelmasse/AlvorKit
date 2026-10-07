namespace AlvorKit;

[Bench]
public class FloatValueSemanticsMeasurements
{
    private object? retained;

    public BenchResult Vec2EqualsAlvorVec2Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft2 = fixture.Alvor.AlvorLeft2;
        var alvorRight2 = fixture.Alvor.AlvorRight2;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec2EqualsAlvorVec2Equals.Run(alvorLeft2, alvorRight2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2EqualsNumericsVec2Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft2 = fixture.Numerics.SystemLeft2;
        var systemRight2 = fixture.Numerics.SystemRight2;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec2EqualsNumericsVec2Equals.Run(systemLeft2, systemRight2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2HashAlvorVec2Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft2 = fixture.Alvor.AlvorLeft2;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec2HashAlvorVec2Hash.Run(alvorLeft2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec2HashNumericsVec2Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft2 = fixture.Numerics.SystemLeft2;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec2HashNumericsVec2Hash.Run(systemLeft2, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3EqualsAlvorVec3Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft3 = fixture.Alvor.AlvorLeft3;
        var alvorRight3 = fixture.Alvor.AlvorRight3;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec3EqualsAlvorVec3Equals.Run(alvorLeft3, alvorRight3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3EqualsNumericsVec3Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft3 = fixture.Numerics.SystemLeft3;
        var systemRight3 = fixture.Numerics.SystemRight3;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec3EqualsNumericsVec3Equals.Run(systemLeft3, systemRight3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3HashAlvorVec3Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft3 = fixture.Alvor.AlvorLeft3;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec3HashAlvorVec3Hash.Run(alvorLeft3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3HashNumericsVec3Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft3 = fixture.Numerics.SystemLeft3;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec3HashNumericsVec3Hash.Run(systemLeft3, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4EqualsAlvorVec4Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft4 = fixture.Alvor.AlvorLeft4;
        var alvorRight4 = fixture.Alvor.AlvorRight4;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec4EqualsAlvorVec4Equals.Run(alvorLeft4, alvorRight4, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4EqualsNumericsVec4Equals()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft4 = fixture.Numerics.SystemLeft4;
        var systemRight4 = fixture.Numerics.SystemRight4;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec4EqualsNumericsVec4Equals.Run(systemLeft4, systemRight4, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4HashAlvorVec4Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var alvorLeft4 = fixture.Alvor.AlvorLeft4;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec4HashAlvorVec4Hash.Run(alvorLeft4, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4HashNumericsVec4Hash()
    {
        var fixture = new FloatValueSemanticsFixture();
        var systemLeft4 = fixture.Numerics.SystemLeft4;
        var output = fixture.Shared.Output;
        var timer = BenchTimer.Start();
        FloatValueSemanticsVec4HashNumericsVec4Hash.Run(systemLeft4, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
