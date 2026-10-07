namespace AlvorKit;

[Bench]
public class ArchPointBenchmarks(ArchPointMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Point",
        "archetypal storage",
        [
            BenchNode.Group(
                "Get",
                "scalar access; varying signature width",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure($"Width{width}", "1,048,576 calls", () => measurements.ScalarGet(width)))
            ]),
            BenchNode.Group(
                "GetAbsent",
                "scalar access; varying signature width",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure($"Width{width}", "1,048,576 calls", () => measurements.ScalarGetAbsent(width)))
            ]),
            BenchNode.Group(
                "HasPresent",
                "scalar access; varying signature width",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure($"Width{width}", "1,048,576 calls", () => measurements.ScalarHasPresent(width)))
            ]),
            BenchNode.Group(
                "HasAbsent",
                "scalar access; varying signature width",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure($"Width{width}", "1,048,576 calls", () => measurements.ScalarHasAbsent(width)))
            ]),
            BenchNode.Group(
                "Set",
                "scalar access; varying signature width",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure($"Width{width}", "1,048,576 calls", () => measurements.ScalarSet(width)))
            ]),
            BenchNode.Measure("WideGet", "eight-column signature; 1,048,576 calls", measurements.WideGet),
            BenchNode.Measure("WideSet", "eight-column signature; 1,048,576 calls", measurements.WideSet),
            BenchNode.Measure("ReferenceGet", "eight-column signature; 1,048,576 calls", measurements.ReferenceGet),
            BenchNode.Measure("ReferenceSet", "eight-column signature; 1,048,576 calls", measurements.ReferenceSet),
            BenchNode.Measure("RefStructGet", "eight-column signature; 1,048,576 calls", measurements.RefStructGet),
            BenchNode.Measure("RefStructSet", "eight-column signature; 1,048,576 calls", measurements.RefStructSet)
    ]);
}
