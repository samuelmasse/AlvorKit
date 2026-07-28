namespace AlvorKit.Script.TestInterception;

/// <summary>Prevents inherited CLR-profiler state from escaping launcher control.</summary>
internal static class InterceptionProfilerEnvironment
{
    /// <summary>Returns active profiler variables in stable diagnostic order.</summary>
    internal static string[] ActiveVariables(
        IReadOnlyDictionary<string, string?> environment) =>
        [.. environment
            .Where(static pair =>
                IsProfilerVariable(pair.Key) &&
                IsActive(pair.Key, pair.Value))
            .Select(static pair => pair.Key)
            .Order(StringComparer.OrdinalIgnoreCase)];

    /// <summary>Removes inherited profiler variables before configuring a child.</summary>
    internal static void Clear(ProcessStartInfo startInfo)
    {
        string[] names =
            [.. startInfo.Environment.Keys.Where(IsProfilerVariable)];
        foreach (var name in names)
            startInfo.Environment.Remove(name);
    }

    private static bool IsProfilerVariable(string name)
    {
        var runtimeVariable =
            name.StartsWith("CORECLR_", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("COR_", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase);
        return runtimeVariable &&
            (name.Contains("PROFILER", StringComparison.OrdinalIgnoreCase) ||
             name.Contains(
                 "ENABLE_PROFILING",
                 StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsActive(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!name.Contains(
                "ENABLE_PROFILING",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
