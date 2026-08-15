namespace AlvorKit;

/// <summary>Writes temporary VSTest settings that profile only the testhost child.</summary>
internal static class InterceptionRunSettings
{
    /// <summary>Profiler COM class identifier supplied to CoreCLR.</summary>
    internal const string ProfilerClsid =
        "{3840ACF7-5AF1-49EA-BF94-5F7086C57F57}";
    /// <summary>Environment variable containing the explicit managed module allowlist.</summary>
    internal const string ModulesVariable =
        "ALVORKIT_INTERCEPTION_MODULES";
    /// <summary>Startup opt-in for allocation callbacks and managed stack snapshots.</summary>
    internal const string AllocationProfilingVariable =
        "ALVORKIT_INTERCEPTION_ALLOCATION_PROFILING";

    /// <summary>Writes one isolated runsettings file and returns its absolute path.</summary>
    internal static string Write(
        string directory,
        string profilerPath,
        IReadOnlyList<string> modules,
        bool allocationProfiling)
    {
        Directory.CreateDirectory(directory);
        var settings = Create(
            profilerPath,
            modules,
            allocationProfiling);
        var path = Path.Combine(directory, "interception.runsettings");
        settings.Save(path);
        return path;
    }

    /// <summary>Creates the settings document without changing process environment.</summary>
    internal static XDocument Create(
        string profilerPath,
        IReadOnlyList<string> modules,
        bool allocationProfiling)
    {
        var moduleList = string.Join(
            ";",
            modules.Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal));
        if (moduleList.Length == 0)
            throw new ArgumentException("At least one allowed module is required.", nameof(modules));

        var environment = new XElement(
            "EnvironmentVariables",
            Variable("CORECLR_ENABLE_PROFILING", "1"),
            Variable("CORECLR_PROFILER", ProfilerClsid),
            Variable("CORECLR_PROFILER_PATH", profilerPath),
            Variable("CORECLR_PROFILER_PATH_64", profilerPath),
            Variable("CORECLR_PROFILER_PATH_ARM64", profilerPath),
            Variable(
                InterceptionProfilerAsset.PathVariable,
                profilerPath),
            Variable("DOTNET_ReadyToRun", "0"),
            Variable(ModulesVariable, moduleList));
        if (allocationProfiling)
            environment.Add(Variable(AllocationProfilingVariable, "1"));

        return new(
            new XElement(
                "RunSettings",
                new XElement(
                    "RunConfiguration",
                    environment)));
    }

    private static XElement Variable(string name, string value) =>
        new(name, value);
}
