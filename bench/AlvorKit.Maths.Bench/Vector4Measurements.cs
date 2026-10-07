namespace AlvorKit;

[Bench]
public class Vector4Measurements
{
    private object? retained;

    public BenchResult AddAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector4AddAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult SubtractAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector4SubtractAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult PairMultiplyAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector4PairMultiplyAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ScaleAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector4ScaleAlvor.Run(alvorLeft, alvorOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DivideAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector4DivideAlvor.Run(alvorLeft, alvorOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult LerpAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Amount = fixture.Shared.Amount;
        var timer = BenchTimer.Start();
        Vector4LerpAlvor.Run(alvorLeft, alvorRight, alvorOutput, Amount, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult BoundsAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector4BoundsAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var ScalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Vector4DotAlvor.Run(alvorLeft, alvorRight, ScalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult NormalizeAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector4NormalizeAlvor.Run(alvorLeft, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ValueSemanticsAlvor()
    {
        var fixture = new Vector4Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var IntOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Vector4ValueSemanticsAlvor.Run(alvorLeft, alvorRight, IntOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult AddNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector4AddNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult SubtractNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector4SubtractNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult PairMultiplyNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector4PairMultiplyNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ScaleNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector4ScaleNumerics.Run(systemLeft, systemOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DivideNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector4DivideNumerics.Run(systemLeft, systemOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult LerpNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Amount = fixture.Shared.Amount;
        var timer = BenchTimer.Start();
        Vector4LerpNumerics.Run(systemLeft, systemRight, systemOutput, Amount, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult BoundsNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector4BoundsNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var ScalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Vector4DotNumerics.Run(systemLeft, systemRight, ScalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult NormalizeNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector4NormalizeNumerics.Run(systemLeft, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ValueSemanticsNumerics()
    {
        var fixture = new Vector4Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var IntOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Vector4ValueSemanticsNumerics.Run(systemLeft, systemRight, IntOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
