namespace AlvorKit.Script.TestInterception;

/// <summary>Writes temporary VSTest settings that profile only the testhost child.</summary>
internal static class InterceptionRunSettings
{
    internal const string ProfilerClsid =
        "{3840ACF7-5AF1-49EA-BF94-5F7086C57F57}";
    internal const string ModulesVariable =
        "ALVORKIT_INTERCEPTION_MODULES";

    /// <summary>Writes one isolated runsettings file and returns its absolute path.</summary>
    internal static string Write(
        string directory,
        string profilerPath,
        IReadOnlyList<string> modules)
    {
        Directory.CreateDirectory(directory);
        var settings = Create(profilerPath, modules);
        var path = Path.Combine(directory, "interception.runsettings");
        settings.Save(path);
        return path;
    }

    /// <summary>Creates the settings document without changing process environment.</summary>
    internal static XDocument Create(
        string profilerPath,
        IReadOnlyList<string> modules)
    {
        var moduleList = string.Join(
            ";",
            modules.Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal));
        if (moduleList.Length == 0)
            throw new ArgumentException("At least one allowed module is required.", nameof(modules));

        return new(
            new XElement(
                "RunSettings",
                new XElement(
                    "RunConfiguration",
                    new XElement(
                        "EnvironmentVariables",
                        Variable("CORECLR_ENABLE_PROFILING", "1"),
                        Variable("CORECLR_PROFILER", ProfilerClsid),
                        Variable("CORECLR_PROFILER_PATH", profilerPath),
                        Variable("CORECLR_PROFILER_PATH_64", profilerPath),
                        Variable(
                            InterceptionProfilerAsset.PathVariable,
                            profilerPath),
                        Variable("DOTNET_ReadyToRun", "0"),
                        Variable(ModulesVariable, moduleList)))));
    }

    private static XElement Variable(string name, string value) =>
        new(name, value);
}
