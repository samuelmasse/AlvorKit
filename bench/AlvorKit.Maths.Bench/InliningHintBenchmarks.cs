namespace AlvorKit;

[Bench]
public class InliningHintBenchmarks(InliningHintMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "InliningHint",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "InliningHint",
                "4,096 inputs; 256 passes",
                new(
                    new("DefaultJit", measurements.InliningHintDefaultJit),
                    [
                        new("AggressiveInlining", measurements.InliningHintAggressiveInlining),
                        new("AggressiveOptimization", measurements.InliningHintAggressiveOptimization),
                        new("BothHints", measurements.InliningHintBothHints),
                        new("NoInliningControl", measurements.InliningHintNoInliningControl)
            ]))
    ]);
}
