namespace AlvorKit;

/// <summary>Resolved profiler settings shared by coverage testhost children.</summary>
internal sealed record CoverageInterceptionSettings(
    string ProfilerPath,
    IReadOnlyList<string> Modules)
{
    /// <summary>Resolves the profiler and default measured source/test allowlist.</summary>
    internal static CoverageInterceptionSettings Resolve(
        string repoRoot,
        IReadOnlyList<string> sourceModules,
        IReadOnlyList<string> testProjects) =>
        new(
            InterceptionProfilerAsset.Resolve(repoRoot, configuredPath: null),
            DefaultModules(sourceModules, testProjects));

    /// <summary>Builds a deterministic allowlist from measured source and test assemblies.</summary>
    internal static IReadOnlyList<string> DefaultModules(
        IReadOnlyList<string> sourceModules,
        IReadOnlyList<string> testProjects) =>
        [
            .. sourceModules
                .Concat(
                    testProjects.Select(
                        static project =>
                            Path.GetFileNameWithoutExtension(project)))
                .Where(static module => !string.IsNullOrWhiteSpace(module))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
        ];

    /// <summary>Runs one test child with a temporary settings file and then removes it.</summary>
    internal async Task<ProcessResult> RunTestAsync(
        Func<string, Task<ProcessResult>> run)
    {
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "AlvorKit",
            "TestCoverage",
            "Interception",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            var settingsPath = InterceptionRunSettings.Write(
                temporaryRoot,
                ProfilerPath,
                Modules,
                allocationProfiling: false);
            return await run(settingsPath);
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
                Directory.Delete(temporaryRoot, recursive: true);
        }
    }
}
