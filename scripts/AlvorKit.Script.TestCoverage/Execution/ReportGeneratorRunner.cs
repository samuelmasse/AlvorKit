namespace AlvorKit.Script.TestCoverage;

/// <summary>Runs the standard ReportGenerator dotnet tool over collected Cobertura reports.</summary>
internal static class ReportGeneratorRunner
{
    /// <summary>Generates the browser-readable HTML report and writes a tool log.</summary>
    public static async Task<bool> RunAsync(
        string repoRoot,
        CoverageOutputPaths output,
        IReadOnlyList<TestProjectResult> testResults)
    {
        var reports = testResults
            .Select(result => Path.Combine(repoRoot, result.CoverageCoberturaPath))
            .Where(File.Exists)
            .ToArray();

        if (reports.Length == 0)
        {
            await File.WriteAllTextAsync(output.ReportGeneratorLog, "No Cobertura reports found.");
            return false;
        }

        ResetDirectory(output.HtmlReportDirectory);
        var restore = await DotNetProcess.RunAsync(repoRoot, ["tool", "restore"]);
        var generator = restore.ExitCode == 0
            ? await RunReportGeneratorAsync(repoRoot, output, reports)
            : new ProcessResult(restore.ExitCode, "");

        await File.WriteAllTextAsync(output.ReportGeneratorLog, restore.Output + generator.Output);
        return restore.ExitCode == 0 && generator.ExitCode == 0 && File.Exists(output.HtmlIndexReport);
    }

    /// <summary>Runs ReportGenerator against the collected coverage files.</summary>
    private static Task<ProcessResult> RunReportGeneratorAsync(
        string repoRoot,
        CoverageOutputPaths output,
        IReadOnlyList<string> reports) =>
        DotNetProcess.RunAsync(
            repoRoot,
            [
                "tool",
                "run",
                "reportgenerator",
                "--",
                "-reports:" + string.Join(";", reports),
                "-targetdir:" + output.HtmlReportDirectory,
                "-sourcedirs:" + string.Join(";", SourceDirectories(repoRoot)),
                "-reporttypes:Html",
                "-title:AlvorKit Coverage",
            ]);

    /// <summary>Returns source roots that undo the repository PathMap used by script and source projects.</summary>
    private static string[] SourceDirectories(string repoRoot) =>
    [
        Path.Combine(repoRoot, "src"),
        Path.Combine(repoRoot, "scripts"),
    ];

    /// <summary>Recreates a directory so stale generated HTML cannot survive.</summary>
    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }
}
