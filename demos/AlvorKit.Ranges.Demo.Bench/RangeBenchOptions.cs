namespace AlvorKit.Ranges.Demo.Bench;

/// <summary>Stores command-line options for the range allocator benchmark demo.</summary>
public sealed record RangeBenchOptions(int Operations, int Runs, int Warmups, int Window, string? JsonPath)
{
    /// <summary>Creates the command-line surface for the range allocator benchmark demo.</summary>
    public static RootCommand CreateRootCommand(Func<RangeBenchOptions, int> execute)
    {
        var quick = new Option<bool>("--quick") { Description = "Use a short agent-friendly sweep." };
        var operations = new Option<string>("--operations") { Description = "Operations per measured run." };
        var runs = new Option<string>("--runs") { Description = "Measured runs per benchmark." };
        var warmups = new Option<string>("--warmups") { Description = "Warmup runs per benchmark." };
        var window = new Option<string>("--window") { Description = "Live range count for rolling-window measurements." };
        var json = new Option<string>("--json") { Description = "JSON result file." };
        var command = new RootCommand("Range allocator benchmark demo.");
        command.Options.Add(quick);
        command.Options.Add(operations);
        command.Options.Add(runs);
        command.Options.Add(warmups);
        command.Options.Add(window);
        command.Options.Add(json);
        command.SetAction(parse => execute(CreateOptions(parse, quick, operations, runs, warmups, window, json)));
        return command;
    }

    /// <summary>Creates immutable benchmark options from parsed command-line values.</summary>
    private static RangeBenchOptions CreateOptions(
        ParseResult parse,
        Option<bool> quick,
        Option<string> operations,
        Option<string> runs,
        Option<string> warmups,
        Option<string> window,
        Option<string> json)
    {
        var shortSweep = parse.GetValue(quick);
        return new(
            ParsePositive(parse.GetValue(operations), "--operations", shortSweep ? 20_000 : 200_000),
            ParsePositive(parse.GetValue(runs), "--runs", shortSweep ? 3 : 7),
            ParseNonNegative(parse.GetValue(warmups), "--warmups", shortSweep ? 1 : 2),
            ParsePositive(parse.GetValue(window), "--window", shortSweep ? 256 : 2_048),
            parse.GetValue(json));
    }

    /// <summary>Parses an optional positive integer option.</summary>
    private static int ParsePositive(string? value, string option, int defaultValue)
    {
        var parsed = ParseNonNegative(value, option, defaultValue);
        return parsed > 0 ? parsed : throw new ArgumentOutOfRangeException(option, "Value must be positive.");
    }

    /// <summary>Parses an optional non-negative integer option.</summary>
    private static int ParseNonNegative(string? value, string option, int defaultValue)
    {
        if (value is null)
            return defaultValue;

        return int.TryParse(value, out var parsed) && parsed >= 0
            ? parsed
            : throw new ArgumentOutOfRangeException(option, "Value must be a non-negative integer.");
    }
}
