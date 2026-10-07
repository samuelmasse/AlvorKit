namespace AlvorKit;

[Bench]
public class ArchMembershipBenchmarks(ArchMembershipMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Membership",
        "archetypal storage",
        [
            BenchNode.Group(
                "PresentFirst",
                "membership kernel; ideal direct uses a precomputed table",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Compare(
                            $"Width{width}",
                            "1,048,576 lookups",
                            new(
                                new("IndexOf", () => measurements.IndexOfPresentFirst(width)),
                                [
                                    new("Binary", () => measurements.BinaryPresentFirst(width)),
                                    new("OrdinalHash", () => measurements.OrdinalHashPresentFirst(width)),
                                    new("IdealDirect", () => measurements.IdealDirectPresentFirst(width))
                    ])))
            ]),
            BenchNode.Group(
                "PresentRotating",
                "membership kernel; ideal direct uses a precomputed table",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Compare(
                            $"Width{width}",
                            "1,048,576 lookups",
                            new(
                                new("IndexOf", () => measurements.IndexOfPresentRotating(width)),
                                [
                                    new("Binary", () => measurements.BinaryPresentRotating(width)),
                                    new("OrdinalHash", () => measurements.OrdinalHashPresentRotating(width)),
                                    new("IdealDirect", () => measurements.IdealDirectPresentRotating(width))
                    ])))
            ]),
            BenchNode.Group(
                "AbsentInterior",
                "membership kernel; ideal direct uses a precomputed table",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Compare(
                            $"Width{width}",
                            "1,048,576 lookups",
                            new(
                                new("IndexOf", () => measurements.IndexOfAbsentInterior(width)),
                                [
                                    new("Binary", () => measurements.BinaryAbsentInterior(width)),
                                    new("OrdinalHash", () => measurements.OrdinalHashAbsentInterior(width)),
                                    new("IdealDirect", () => measurements.IdealDirectAbsentInterior(width))
                    ])))
            ]),
            BenchNode.Group(
                "AbsentHigh",
                "membership kernel; ideal direct uses a precomputed table",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Compare(
                            $"Width{width}",
                            "1,048,576 lookups",
                            new(
                                new("IndexOf", () => measurements.IndexOfAbsentHigh(width)),
                                [
                                    new("Binary", () => measurements.BinaryAbsentHigh(width)),
                                    new("OrdinalHash", () => measurements.OrdinalHashAbsentHigh(width)),
                                    new("IdealDirect", () => measurements.IdealDirectAbsentHigh(width))
                    ])))
            ])
    ]);
}
