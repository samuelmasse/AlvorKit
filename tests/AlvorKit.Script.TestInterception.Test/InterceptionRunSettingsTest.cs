namespace AlvorKit.Script.TestInterception;

[TestClass]
public class InterceptionRunSettingsTest
{
    /// <summary>Scopes the profiler path and module allowlist to one VSTest run.</summary>
    [TestMethod]
    public void CreateScopesProfilerVariablesToRunConfiguration()
    {
        const string profiler = @"C:\native\interception.dll";

        var settings = InterceptionRunSettings.Create(
            profiler,
            ["Example.Test", "Example.Game", "Example.Test"],
            allocationProfiling: true);
        var environment = settings
            .Root!
            .Element("RunConfiguration")!
            .Element("EnvironmentVariables")!;

        Assert.AreEqual(
            "1",
            environment.Element("CORECLR_ENABLE_PROFILING")!.Value);
        Assert.AreEqual(
            InterceptionRunSettings.ProfilerClsid,
            environment.Element("CORECLR_PROFILER")!.Value);
        Assert.AreEqual(
            profiler,
            environment.Element("CORECLR_PROFILER_PATH")!.Value);
        Assert.AreEqual(
            profiler,
            environment.Element("CORECLR_PROFILER_PATH_64")!.Value);
        Assert.AreEqual(
            profiler,
            environment.Element("CORECLR_PROFILER_PATH_ARM64")!.Value);
        Assert.AreEqual(
            profiler,
            environment.Element(
                InterceptionProfilerAsset.PathVariable)!.Value);
        Assert.AreEqual(
            "Example.Test;Example.Game",
            environment.Element(
                InterceptionRunSettings.ModulesVariable)!.Value);
        Assert.AreEqual(
            "0",
            environment.Element("DOTNET_ReadyToRun")!.Value);
        Assert.AreEqual(
            "1",
            environment.Element(
                InterceptionRunSettings.AllocationProfilingVariable)!.Value);
    }

    /// <summary>Rejects a run configuration without an allowed module.</summary>
    [TestMethod]
    public void CreateRejectsEmptyAllowlist()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => InterceptionRunSettings.Create(
                @"C:\native\interception.dll",
                [],
                allocationProfiling: false));
    }
}
