namespace AlvorKit;

[Bench]
public class MathsValuesBenchmarks(
    ConversionBenchmarks conversion,
    EqualityComparerBenchmarks equalityComparer,
    FloatValueSemanticsBenchmarks floatValueSemantics)
{
    public BenchNode[] Create() => [conversion.Create(), equalityComparer.Create(), floatValueSemantics.Create()];
}
