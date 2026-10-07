namespace AlvorKit;

[Bench]
public class HashBenchmarks(HashMeasurements measurements) : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.Hashing.Bench",
    [
        BenchNode.Group("Table", "4,096 keys; 256 passes; same approved algorithm in both assemblies",
        [
            BenchNode.Group("Int32", "key shape",
            [
                ..new[] { 15, 31, 63 }.Select(mask => BenchNode.Compare($"Mask{mask}", "table mask", new(
                    new("Inline", () => measurements.TableInt32Inline(mask)),
                    [new("Helper", () => measurements.TableInt32Helper(mask))])))
            ]),
            BenchNode.Group("Int32Pair", "key shape",
            [
                ..new[] { 15, 31, 63 }.Select(mask => BenchNode.Compare($"Mask{mask}", "table mask", new(
                    new("Inline", () => measurements.TableInt32PairInline(mask)),
                    [new("Helper", () => measurements.TableInt32PairHelper(mask))])))
            ]),
            BenchNode.Group("Int64", "key shape",
            [
                ..new[] { 15, 31, 63 }.Select(mask => BenchNode.Compare($"Mask{mask}", "table mask", new(
                    new("Inline", () => measurements.TableInt64Inline(mask)),
                    [new("Helper", () => measurements.TableInt64Helper(mask))])))
            ]),
            BenchNode.Group("UInt64", "key shape",
            [
                ..new[] { 15, 31, 63 }.Select(mask => BenchNode.Compare($"Mask{mask}", "table mask", new(
                    new("Inline", () => measurements.TableUInt64Inline(mask)),
                    [new("Helper", () => measurements.TableUInt64Helper(mask))])))
            ]),
            BenchNode.Group("UInt64Int32", "key shape",
            [
                ..new[] { 15, 31, 63 }.Select(mask => BenchNode.Compare($"Mask{mask}", "table mask", new(
                    new("Inline", () => measurements.TableUInt64Int32Inline(mask)),
                    [new("Helper", () => measurements.TableUInt64Int32Helper(mask))])))
            ]),
        ]),
        BenchNode.Group("Epoch32", "retained collections; one operation clears, fills, and reads all keys",
        [
            ..new[] { 16, 256, 4096 }.Select(count => BenchNode.Compare($"Count{count}", $"{count} keys per cycle", new(
                new("Dictionary", () => measurements.Epoch32Dictionary(count)),
                [new("EpochIndex", () => measurements.Epoch32EpochIndex(count))])))
        ]),
        BenchNode.Group("Epoch64", "retained collections; one operation clears, fills, and reads all keys",
        [
            ..new[] { 16, 256, 4096 }.Select(count => BenchNode.Compare($"Count{count}", $"{count} keys per cycle", new(
                new("Dictionary", () => measurements.Epoch64Dictionary(count)),
                [new("EpochIndex", () => measurements.Epoch64EpochIndex(count))])))
        ]),
        BenchNode.Compare("Checksum", "4,096 values; 256 passes", new(
            new("Inline", measurements.ChecksumInline),
            [new("Helper", measurements.ChecksumHelper)]))
    ]);
}
