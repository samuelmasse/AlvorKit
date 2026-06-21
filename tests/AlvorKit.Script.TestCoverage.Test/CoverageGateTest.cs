namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for coverage gate pass and fail decisions.</summary>
[TestClass]
public sealed class CoverageGateTest
{
    /// <summary>A zero threshold permits unmeasured modules for exploratory report generation.</summary>
    [TestMethod]
    public void Passes_ZeroThreshold_IgnoresUnmeasuredModules()
    {
        var summary = Summary(unmeasuredModules: ["Tool"]);

        Assert.IsTrue(CoverageGate.Passes([PassedProject()], summary, CoverageThresholds.All(0)));
    }

    /// <summary>An enabled threshold fails when any source module was not measured.</summary>
    [TestMethod]
    public void Passes_EnabledThresholdFails_WhenModuleUnmeasured()
    {
        var summary = Summary(unmeasuredModules: ["Tool"]);

        Assert.IsFalse(CoverageGate.Passes([PassedProject()], summary, CoverageThresholds.Default));
    }

    /// <summary>Each coverage metric is checked against its own threshold.</summary>
    [TestMethod]
    public void Passes_MetricSpecificThresholds_ChecksEachMetric()
    {
        var summary = new CoverageSummary(
            new(new(95, 100), new(84, 100), new(95, 100)),
            [new("Tool", new(95, 100), new(84, 100), new(95, 100))],
            [],
            []);

        Assert.IsFalse(CoverageGate.Passes([PassedProject()], summary, CoverageThresholds.Default));
        Assert.IsTrue(CoverageGate.Passes([PassedProject()], summary, new(95, 84, 95)));
    }

    /// <summary>Failed test projects fail the gate even if coverage metrics meet the threshold.</summary>
    [TestMethod]
    public void Passes_FailedTestProject_ReturnsFalse()
    {
        var failedProject = PassedProject() with { ExitCode = 1 };

        Assert.IsFalse(CoverageGate.Passes([failedProject], Summary([]), CoverageThresholds.All(0)));
    }

    /// <summary>Slow test timings fail the gate after tests and coverage otherwise pass.</summary>
    [TestMethod]
    public void Passes_SlowTiming_ReturnsFalse()
    {
        var timing = Timing(warnOnly: false, isComplete: true, slowResults: [SlowTest()]);

        Assert.IsFalse(CoverageGate.Passes([PassedProject()], Summary([]), CoverageThresholds.All(0), timing));
    }

    /// <summary>Warn-only timing keeps exploratory coverage reports from failing on known slow tests.</summary>
    [TestMethod]
    public void Passes_WarnOnlySlowTiming_ReturnsTrue()
    {
        var timing = Timing(warnOnly: true, isComplete: true, slowResults: [SlowTest()]);

        Assert.IsTrue(CoverageGate.Passes([PassedProject()], Summary([]), CoverageThresholds.All(0), timing));
    }

    /// <summary>Missing timing data fails the gate because the coverage run did not prove the test budget.</summary>
    [TestMethod]
    public void Passes_MissingTiming_ReturnsFalse()
    {
        var timing = Timing(warnOnly: false, isComplete: false, slowResults: []);

        Assert.IsFalse(CoverageGate.Passes([PassedProject()], Summary([]), CoverageThresholds.All(0), timing));
    }

    /// <summary>Creates a passing test project result.</summary>
    private static TestProjectResult PassedProject() =>
        new("Tool.Test", "tests/Tool.Test/Tool.Test.csproj", 0, TimeSpan.Zero, "log", "coverage.json", "coverage.xml", "coverage.info");

    /// <summary>Creates a timing summary for gate tests.</summary>
    private static TestTimingSummary Timing(bool warnOnly, bool isComplete, IReadOnlyList<TestTimingResult> slowResults) =>
        new(TimeSpan.FromMilliseconds(100), warnOnly, isComplete, 1, slowResults, "timing.md", "timing.csv");

    /// <summary>Creates a slow timing result for gate tests.</summary>
    private static TestTimingResult SlowTest() =>
        new("Slow", "Passed", TimeSpan.FromMilliseconds(150), "run.trx");

    /// <summary>Creates a fully covered summary with configurable unmeasured modules.</summary>
    private static CoverageSummary Summary(IReadOnlyList<string> unmeasuredModules) =>
        new(
            new(new(1, 1), new(1, 1), new(1, 1)),
            [new("Tool", new(1, 1), new(1, 1), new(1, 1))],
            unmeasuredModules,
            []);
}
