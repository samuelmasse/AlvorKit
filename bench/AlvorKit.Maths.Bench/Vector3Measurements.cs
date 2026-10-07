namespace AlvorKit;

[Bench]
public class Vector3Measurements
{
    private object? retained;

    public BenchResult AddAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3AddAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult SubtractAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3SubtractAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult PairMultiplyAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3PairMultiplyAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ScaleAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector3ScaleAlvor.Run(alvorLeft, alvorOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DivideAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector3DivideAlvor.Run(alvorLeft, alvorOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult LerpAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var Amount = fixture.Shared.Amount;
        var timer = BenchTimer.Start();
        Vector3LerpAlvor.Run(alvorLeft, alvorRight, alvorOutput, Amount, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult BoundsAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3BoundsAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var ScalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Vector3DotAlvor.Run(alvorLeft, alvorRight, ScalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult NormalizeAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3NormalizeAlvor.Run(alvorLeft, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ValueSemanticsAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var IntOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Vector3ValueSemanticsAlvor.Run(alvorLeft, alvorRight, IntOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult CrossAlvor()
    {
        var fixture = new Vector3Fixture();
        var alvorLeft = fixture.Alvor.AlvorLeft;
        var alvorRight = fixture.Alvor.AlvorRight;
        var alvorOutput = fixture.Alvor.AlvorOutput;
        var timer = BenchTimer.Start();
        Vector3CrossAlvor.Run(alvorLeft, alvorRight, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult AddNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3AddNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult SubtractNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3SubtractNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult PairMultiplyNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3PairMultiplyNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ScaleNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector3ScaleNumerics.Run(systemLeft, systemOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DivideNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Scalar = fixture.Shared.Scalar;
        var timer = BenchTimer.Start();
        Vector3DivideNumerics.Run(systemLeft, systemOutput, Scalar, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult LerpNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var Amount = fixture.Shared.Amount;
        var timer = BenchTimer.Start();
        Vector3LerpNumerics.Run(systemLeft, systemRight, systemOutput, Amount, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult BoundsNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3BoundsNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var ScalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Vector3DotNumerics.Run(systemLeft, systemRight, ScalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult NormalizeNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3NormalizeNumerics.Run(systemLeft, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult ValueSemanticsNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var IntOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Vector3ValueSemanticsNumerics.Run(systemLeft, systemRight, IntOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult CrossNumerics()
    {
        var fixture = new Vector3Fixture();
        var systemLeft = fixture.Numerics.SystemLeft;
        var systemRight = fixture.Numerics.SystemRight;
        var systemOutput = fixture.Numerics.SystemOutput;
        var timer = BenchTimer.Start();
        Vector3CrossNumerics.Run(systemLeft, systemRight, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
