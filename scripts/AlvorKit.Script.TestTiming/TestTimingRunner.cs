namespace AlvorKit.Script.TestTiming;

/// <summary>Runs <c>dotnet test</c>, reads TRX output, and reports test timing budget violations.</summary>
[ExcludeFromCodeCoverage(Justification = "Coordinates external dotnet test processes and filesystem artifacts.")]
internal sealed class TestTimingRunner
{
    /// <summary>Runs the timing guard and returns a process exit code.</summary>
    /// <param name="options">Options for the guarded test timing run.</param>
    public int Run(TestTimingOptions options)
    {
        var testExitCode = 0;
        if (options.TrxPath is null)
        {
            Directory.CreateDirectory(options.ResultsDirectory);
            testExitCode = RunDotNetTest(options);
        }

        var trxPaths = FindTrxPaths(options);
        if (trxPaths.Length == 0)
        {
            Console.Error.WriteLine($"No TRX files found under {options.ResultsDirectory}.");
            return 1;
        }

        var results = new TrxTimingReader().ReadFiles(trxPaths);
        var report = new TestTimingReportWriter().Write(results, options.MaxDuration, options.ResultsDirectory);
        WriteConsoleSummary(report, options);
        if (testExitCode != 0)
            return testExitCode;

        return report.SlowResults.Count == 0 || options.WarnOnly ? 0 : 1;
    }

    /// <summary>Runs the underlying <c>dotnet test</c> command with a TRX logger.</summary>
    private static int RunDotNetTest(TestTimingOptions options)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = options.RepoRoot,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("test");
        foreach (var arg in DotNetTestArguments(options))
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet test.");
        process.WaitForExit();
        return process.ExitCode;
    }

    /// <summary>Returns forwarded <c>dotnet test</c> arguments plus the required TRX logger options.</summary>
    private static IEnumerable<string> DotNetTestArguments(TestTimingOptions options)
    {
        foreach (var arg in options.DotNetTestArguments)
            yield return arg;

        yield return "--logger";
        yield return "trx;LogFilePrefix=test-timing";
        yield return "--results-directory";
        yield return options.ResultsDirectory;
    }

    /// <summary>Finds the TRX files to inspect for a run or parse-only invocation.</summary>
    private static string[] FindTrxPaths(TestTimingOptions options) =>
        options.TrxPath is not null
            ? [options.TrxPath]
            : Directory.GetFiles(options.ResultsDirectory, "*.trx", SearchOption.AllDirectories);

    /// <summary>Writes a concise console summary and slow-test warnings.</summary>
    private static void WriteConsoleSummary(TestTimingReport report, TestTimingOptions options)
    {
        Console.WriteLine(
            $"Test timing: {report.TotalCount} tests, {report.SlowResults.Count} over {Seconds(options.MaxDuration)}s.");
        foreach (var result in report.SlowResults.Take(10))
            Console.WriteLine($"WARNING AVKTESTTIMING: {Seconds(result.Duration)}s {result.TestName}");

        if (report.SlowResults.Count > 10)
            Console.WriteLine($"WARNING AVKTESTTIMING: {report.SlowResults.Count - 10} more slow tests in the report.");

        Console.WriteLine($"Timing report: {report.MarkdownPath}");
        Console.WriteLine($"Timing CSV: {report.CsvPath}");
    }

    /// <summary>Formats a duration in seconds for console output.</summary>
    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
}
