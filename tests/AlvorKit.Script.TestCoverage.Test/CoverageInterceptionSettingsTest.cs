namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests coverage-specific Interception settings and child argument routing.</summary>
[TestClass]
public sealed class CoverageInterceptionSettingsTest
{
    /// <summary>The default allowlist includes measured sources and selected test assemblies.</summary>
    [TestMethod]
    public void DefaultModules_IncludesMeasuredSourceAndTestModules()
    {
        var modules = CoverageInterceptionSettings.DefaultModules(
            ["Game", "Core", "Game"],
            [
                @"C:\repo\tests\Game.Test\Game.Test.csproj",
                @"C:\repo\tests\Core.Test\Core.Test.csproj"
            ]);

        CollectionAssert.AreEqual(
            new[] { "Core", "Core.Test", "Game", "Game.Test" },
            modules.ToArray());
    }

    /// <summary>The settings file exists only while the profiled child delegate is active.</summary>
    [TestMethod]
    public async Task RunTestAsync_WritesSettingsAndCleansTemporaryDirectory()
    {
        var settings = new CoverageInterceptionSettings(
            @"C:\native\AlvorKit.Interception.Profiler.Native.dll",
            ["Game", "Game.Test"]);
        string? capturedPath = null;

        var result = await settings.RunTestAsync(
            path =>
            {
                capturedPath = path;
                Assert.IsTrue(File.Exists(path));
                var document = XDocument.Load(path);
                var environment = document
                    .Root!
                    .Element("RunConfiguration")!
                    .Element("EnvironmentVariables")!;
                Assert.AreEqual(
                    "Game;Game.Test",
                    environment
                        .Element("ALVORKIT_INTERCEPTION_MODULES")!
                        .Value);
                return Task.FromResult(new ProcessResult(7, "child-output"));
            });

        Assert.AreEqual(7, result.ExitCode);
        Assert.AreEqual("child-output", result.Output);
        Assert.IsNotNull(capturedPath);
        Assert.IsFalse(File.Exists(capturedPath));
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(capturedPath)));
    }

    /// <summary>Only dotnet test arguments receive the temporary VSTest settings path.</summary>
    [TestMethod]
    public void TestProjectArguments_SettingsAppearOnlyOnTestCommand()
    {
        var runner = new TestProjectRunner(
            @"C:\repo",
            CoverageOptions.Parse(["--interception"]),
            ["Game"]);
        var build = runner.BuildBuildArguments(@"C:\repo\tests\Game.Test.csproj");
        var test = runner.BuildTestArguments(
            @"C:\repo\tests\Game.Test.csproj",
            @"C:\out\coverage",
            @"C:\out",
            noBuild: false,
            settingsPath: @"C:\temp\interception.runsettings");

        CollectionAssert.DoesNotContain(build.ToArray(), "--settings");
        var settingsIndex = test.ToList().IndexOf("--settings");
        Assert.IsTrue(settingsIndex >= 0);
        Assert.AreEqual(
            @"C:\temp\interception.runsettings",
            test[settingsIndex + 1]);
    }
}
