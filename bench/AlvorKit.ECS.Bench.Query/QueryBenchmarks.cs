namespace AlvorKit;

[Bench]
public class QueryBenchmarks(QueryMeasurements measurements) : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.ECS.Bench.Query",
    [
        BenchNode.Group("Traversal", "16,384 Ents; 256 passes; counts are Ents, including wide reads",
        [
            BenchNode.Measure("Sparse", "allocation order", measurements.Sparse),
            BenchNode.Measure("SparseShuffled", "deterministic shuffled order", measurements.SparseShuffled),
            BenchNode.Measure("Archetypal", "allocation order", measurements.Archetypal),
            BenchNode.Measure("ArchetypalShuffled", "deterministic shuffled order", measurements.ArchetypalShuffled),
            BenchNode.Measure("Spans", "one column", measurements.Spans),
            BenchNode.Measure("Rows", "one column", measurements.Rows),
            BenchNode.Measure("WideSpans2", "two columns", measurements.WideSpans2),
            BenchNode.Measure("WideRows2", "two columns", measurements.WideRows2),
            BenchNode.Measure("WideSpans8", "eight columns", measurements.WideSpans8),
            BenchNode.Measure("WideRows8", "eight columns", measurements.WideRows8),
        ]),
        BenchNode.Group("Discovery", "prepared catalog; 200,000 queries",
        [
            BenchNode.Measure("ManyArch", "2,047 materialized archs; 128 matches; one active match", measurements.ManyArch),
        ]),
    ]);
}
