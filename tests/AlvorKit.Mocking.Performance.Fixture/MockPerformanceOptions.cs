namespace AlvorKit;

/// <summary>Defines fixed measurement batches and the optional JSON artifact path.</summary>
internal sealed record MockPerformanceOptions(
    string? OutputPath,
    int Warmups,
    int Runs,
    int DispatchOperations,
    int OriginalOperations,
    int ContentionOperations,
    int SnapshotHistory,
    int Workers)
{
    /// <summary>Parses the fixture's deliberately small command surface.</summary>
    internal static MockPerformanceOptions Parse(string[] args)
    {
        string? outputPath = null;
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] != "--output" || index + 1 >= args.Length)
            {
                throw new ArgumentException(
                    "Usage: AlvorKit.Mocking.Performance.Fixture [--output <json-path>]");
            }

            outputPath = args[++index];
        }

        return new(
            outputPath,
            Warmups: 3,
            Runs: 9,
            DispatchOperations: 20_000,
            OriginalOperations: 1_000_000,
            ContentionOperations: 40_000,
            SnapshotHistory: 256,
            Workers: 8);
    }
}
