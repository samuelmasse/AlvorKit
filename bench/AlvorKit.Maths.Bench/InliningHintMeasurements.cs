namespace AlvorKit;

[Bench]
public class InliningHintMeasurements
{
    private object? retained;

    public BenchResult InliningHintDefaultJit()
    {
        var fixture = new InliningHintFixture();
        var left = fixture.Left;
        var right = fixture.Right;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        InliningHintInliningHintDefaultJit.Run(left, right, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult InliningHintAggressiveInlining()
    {
        var fixture = new InliningHintFixture();
        var left = fixture.Left;
        var right = fixture.Right;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        InliningHintInliningHintAggressiveInlining.Run(left, right, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult InliningHintAggressiveOptimization()
    {
        var fixture = new InliningHintFixture();
        var left = fixture.Left;
        var right = fixture.Right;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        InliningHintInliningHintAggressiveOptimization.Run(left, right, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult InliningHintBothHints()
    {
        var fixture = new InliningHintFixture();
        var left = fixture.Left;
        var right = fixture.Right;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        InliningHintInliningHintBothHints.Run(left, right, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult InliningHintNoInliningControl()
    {
        var fixture = new InliningHintFixture();
        var left = fixture.Left;
        var right = fixture.Right;
        var output = fixture.Output;
        var timer = BenchTimer.Start();
        InliningHintInliningHintNoInliningControl.Run(left, right, output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
