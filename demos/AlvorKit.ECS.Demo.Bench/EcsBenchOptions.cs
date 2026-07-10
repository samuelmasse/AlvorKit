namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores command-line options for the ECS benchmark demo.</summary>
public sealed record EcsBenchOptions(
    int Operations,
    int Runs,
    int Warmups,
    string? JsonPath,
    string Suite,
    string[] Cases,
    int[] Widths,
    int Arches,
    int Rows,
    int Allocs,
    string Label,
    bool List,
    string? WorkerCase,
    int SampleIndex,
    string? WorkerJsonPath)
{
    internal const string CoreSuite = "core";
    internal const string ArchetypalSuite = "archetypal";
    internal const string AllSuite = "all";

    /// <summary>Creates the original core-only option shape.</summary>
    public EcsBenchOptions(int operations, int runs, int warmups, string? jsonPath)
        : this(
            operations,
            runs,
            warmups,
            jsonPath,
            CoreSuite,
            [],
            [1, 4, 8, 16, 32],
            2_048,
            4_096,
            4,
            "unlabelled",
            false,
            null,
            0,
            null)
    {
    }

    /// <summary>Deconstructs the original core-only option shape.</summary>
    public void Deconstruct(out int operations, out int runs, out int warmups, out string? jsonPath)
    {
        operations = Operations;
        runs = Runs;
        warmups = Warmups;
        jsonPath = JsonPath;
    }

    /// <summary>Creates the command-line surface for the ECS benchmark demo.</summary>
    /// <param name="execute">Action that runs the benchmark with parsed options.</param>
    public static RootCommand CreateRootCommand(Func<EcsBenchOptions, int> execute)
    {
        var quick = new Option<bool>("--quick") { Description = "Use a short agent-friendly sweep." };
        var operations = new Option<string>("--operations") { Description = "Operations per measured run." };
        var runs = new Option<string>("--runs") { Description = "Measured runs per benchmark." };
        var warmups = new Option<string>("--warmups") { Description = "Warmup runs per benchmark." };
        var json = new Option<string>("--json") { Description = "JSON result file." };
        var suite = new Option<string>("--suite") { Description = "Benchmark suite: core, archetypal, or all." };
        var cases = new Option<string>("--cases") { Description = "Comma-separated archetypal case IDs." };
        var widths = new Option<string>("--widths") { Description = "Comma-separated arch widths." };
        var arches = new Option<string>("--arches") { Description = "Unique arch target for catalog and footprint cases." };
        var rows = new Option<string>("--rows") { Description = "Rows used by structural and footprint cases." };
        var allocs = new Option<string>("--allocs") { Description = "Concurrent alloc owners." };
        var label = new Option<string>("--label") { Description = "Label stored in the versioned report." };
        var list = new Option<bool>("--list") { Description = "List archetypal case IDs without running them." };
        var workerCase = new Option<string>("--worker-case") { Description = "Internal isolated worker case." };
        var sampleIndex = new Option<string>("--sample-index") { Description = "Internal isolated sample index." };
        var workerJson = new Option<string>("--worker-json") { Description = "Internal isolated sample output." };
        var command = new RootCommand("ECS benchmark demo.");

        command.Options.Add(quick);
        command.Options.Add(operations);
        command.Options.Add(runs);
        command.Options.Add(warmups);
        command.Options.Add(json);
        command.Options.Add(suite);
        command.Options.Add(cases);
        command.Options.Add(widths);
        command.Options.Add(arches);
        command.Options.Add(rows);
        command.Options.Add(allocs);
        command.Options.Add(label);
        command.Options.Add(list);
        command.Options.Add(workerCase);
        command.Options.Add(sampleIndex);
        command.Options.Add(workerJson);
        command.SetAction(parse => execute(CreateOptions(
            parse,
            quick,
            operations,
            runs,
            warmups,
            json,
            suite,
            cases,
            widths,
            arches,
            rows,
            allocs,
            label,
            list,
            workerCase,
            sampleIndex,
            workerJson)));
        return command;
    }

    /// <summary>Parses benchmark options without running a benchmark.</summary>
    public static EcsBenchOptions Parse(string[] args)
    {
        EcsBenchOptions? options = null;
        var result = CreateRootCommand(value =>
        {
            options = value;
            return 0;
        }).Parse(args);

        ThrowIfErrors(result);
        result.Invoke(new() { EnableDefaultExceptionHandler = false });
        return options!;
    }

    private static EcsBenchOptions CreateOptions(
        ParseResult parse,
        Option<bool> quick,
        Option<string> operations,
        Option<string> runs,
        Option<string> warmups,
        Option<string> json,
        Option<string> suite,
        Option<string> cases,
        Option<string> widths,
        Option<string> arches,
        Option<string> rows,
        Option<string> allocs,
        Option<string> label,
        Option<bool> list,
        Option<string> workerCase,
        Option<string> sampleIndex,
        Option<string> workerJson)
    {
        bool shortSweep = parse.GetValue(quick);
        string suiteName = ParseSuite(parse.GetValue(suite));

        return new(
            ParsePositive(parse.GetValue(operations), "--operations", shortSweep ? 1_000_000 : 5_000_000),
            ParsePositive(parse.GetValue(runs), "--runs", shortSweep ? 3 : 7),
            ParseNonNegative(parse.GetValue(warmups), "--warmups", shortSweep ? 1 : 2),
            parse.GetValue(json),
            suiteName,
            ParseList(parse.GetValue(cases)),
            ParseWidths(parse.GetValue(widths)),
            ParsePositive(parse.GetValue(arches), "--arches", shortSweep ? 128 : 2_048),
            ParsePositive(parse.GetValue(rows), "--rows", shortSweep ? 256 : 4_096),
            ParsePositive(parse.GetValue(allocs), "--allocs", 4),
            parse.GetValue(label) ?? "unlabelled",
            parse.GetValue(list),
            parse.GetValue(workerCase),
            ParseNonNegative(parse.GetValue(sampleIndex), "--sample-index", 0),
            parse.GetValue(workerJson));
    }

    private static string ParseSuite(string? value) => value?.ToLowerInvariant() switch
    {
        null or CoreSuite => CoreSuite,
        ArchetypalSuite => ArchetypalSuite,
        AllSuite => AllSuite,
        _ => throw new ArgumentOutOfRangeException("--suite", "Suite must be core, archetypal, or all."),
    };

    private static string[] ParseList(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static int[] ParseWidths(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [1, 4, 8, 16, 32];

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out int width) || width is not (1 or 4 or 8 or 16 or 32))
                throw new ArgumentOutOfRangeException("--widths", "Widths must be selected from 1, 4, 8, 16, and 32.");
            result[i] = width;
        }

        return result;
    }

    private static int ParsePositive(string? value, string option, int defaultValue)
    {
        int parsed = ParseNonNegative(value, option, defaultValue);
        return parsed > 0 ? parsed : throw new ArgumentOutOfRangeException(option, "Value must be positive.");
    }

    private static int ParseNonNegative(string? value, string option, int defaultValue)
    {
        if (value is null)
            return defaultValue;

        return int.TryParse(value, out int parsed) && parsed >= 0
            ? parsed
            : throw new ArgumentOutOfRangeException(option, "Value must be a non-negative integer.");
    }

    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}
