namespace AlvorKit.Script.TestInterception;

[TestClass]
public sealed class InterceptionProfilerEnvironmentTest
{
    /// <summary>Reports only profiler variables whose values enable effective state.</summary>
    [TestMethod]
    public void ActiveVariablesReportsOnlyEffectiveProfilerState()
    {
        Dictionary<string, string?> environment = new(
            StringComparer.OrdinalIgnoreCase)
        {
            ["CORECLR_ENABLE_PROFILING"] = "0",
            ["CORECLR_PROFILER"] = string.Empty,
            ["COR_ENABLE_PROFILING"] = "true",
            ["DOTNET_PROFILER_PATH_64"] = @"C:\native\profiler.dll",
            ["DOTNET_EnableDiagnostics"] = "0",
            ["ALVORKIT_INTERCEPTION_PROFILER_PATH"] =
                @"C:\native\interception.dll"
        };

        CollectionAssert.AreEqual(
            new[] { "COR_ENABLE_PROFILING", "DOTNET_PROFILER_PATH_64" },
            InterceptionProfilerEnvironment.ActiveVariables(environment));
    }

    /// <summary>Clears profiler state without removing unrelated runtime configuration.</summary>
    [TestMethod]
    public void ClearRemovesProfilerVariablesFromChild()
    {
        ProcessStartInfo startInfo = new("dotnet");
        startInfo.Environment["CORECLR_ENABLE_PROFILING"] = "1";
        startInfo.Environment["CORECLR_PROFILER"] = "{profiler}";
        startInfo.Environment["DOTNET_ReadyToRun"] = "0";

        InterceptionProfilerEnvironment.Clear(startInfo);

        Assert.IsFalse(
            startInfo.Environment.ContainsKey("CORECLR_ENABLE_PROFILING"));
        Assert.IsFalse(
            startInfo.Environment.ContainsKey("CORECLR_PROFILER"));
        Assert.AreEqual("0", startInfo.Environment["DOTNET_ReadyToRun"]);
    }
}
