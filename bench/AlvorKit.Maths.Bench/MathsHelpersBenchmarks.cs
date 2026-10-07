namespace AlvorKit;

[Bench]
public class MathsHelpersBenchmarks(
    SwizzleBenchmarks swizzle,
    InliningHintBenchmarks inliningHint,
    VectorLeafBenchmarks vectorLeaf)
{
    public BenchNode[] Create() => [swizzle.Create(), inliningHint.Create(), vectorLeaf.Create()];
}
