namespace AlvorKit;

/// <summary>Verifies sampling, ordering, output modes, and the executable suite contract.</summary>
[TestClass]
[DoNotParallelize]
public class BenchExecutionTest
{
    /// <summary>Catalog flattening preserves inherited descriptions and the baseline precedes each candidate.</summary>
    [TestMethod]
    public void RunnerUsesOnlyRetainedSamplesForComparisons()
    {
        using var output = new BenchOutputCapture();
        var command = new BenchCommand(BenchCommandKind.Run, [], [], 1, 2, null, new(false, true, false));
        var console = new BenchConsole();
        var runner = new BenchRunner(command, console, new(command, console, BenchAllocationTracking.Connect()));
        var catalog = new BenchCatalogBuilder().Create(new BenchTestSuite().Create());
        Assert.AreEqual("parent; child", catalog.Cases[0].Description);
        Assert.AreEqual("parent", catalog.Cases[2].Description);
        Assert.AreEqual("Group/Compare/Base", catalog.Cases[1].BaselineId);
        var execution = runner.Run(catalog, catalog.Cases);
        Assert.AreEqual(4, execution.Benchmarks.Length);
        Assert.AreEqual(1, execution.Benchmarks[0].Measurement.Warmups.Length);
        Assert.AreEqual(2, execution.Benchmarks[0].Measurement.Samples.Length);
        Assert.AreEqual(0.5, execution.Benchmarks[1].Measurement.BaselineMultiplier);
        Assert.IsNull(execution.Benchmarks[0].Measurement.BaselineMultiplier);
        StringAssert.Contains(output.Text, "50.00% faster");
        var candidateOnly = runner.Run(catalog, [catalog.Cases[1]]);
        Assert.IsNull(candidateOnly.Benchmarks[0].Measurement.BaselineMultiplier);
    }

    /// <summary>Warmups never affect the mean, while retained memory trees and workload allocation totals do.</summary>
    [TestMethod]
    public void SamplingSeparatesWarmupsAndAveragesMemory()
    {
        using var output = new BenchOutputCapture();
        var command = new BenchCommand(BenchCommandKind.Run, [], [], 1, 2, null, new(false, true, true));
        var measurement = 0;
        var runner = new BenchMeasurementRunner(command, new(), BenchAllocationTracking.Connect());
        var result = runner.Run(() =>
        {
            measurement++;
            return BenchTestSuite.Result(measurement == 1 ? 1000 : measurement * 10)
                .WithUnmanagedMemory(new("Root", measurement * 100, measurement * 100, 50, 50, []));
        }, 0, TimeSpan.FromTicks(10));
        Assert.AreEqual(3, measurement);
        Assert.AreEqual(25L, result.Summary.Elapsed.Ticks);
        Assert.AreEqual(25L, result.Summary.WorkloadAllocatedBytes);
        Assert.AreEqual(250d, result.Summary.UnmanagedMemory!.TotalBytes);
        Assert.AreEqual(2.5, result.BaselineMultiplier);
        StringAssert.Contains(output.Text, "mean memory");
        StringAssert.Contains(output.Text, "slower");
        var meanCommand = command with { Display = command.Display with { ShowSamples = false } };
        var meanOnly = new BenchMeasurementRunner(meanCommand, new(), BenchAllocationTracking.Connect());
        meanOnly.Run(() => BenchTestSuite.Result(10), 0, null);
        Assert.AreEqual(1000d, new BenchResult(TimeSpan.FromSeconds(1), 1000, "op", "N0").OperationsPerSecond);
    }

    /// <summary>The host composes the migrated services, lists without running, and fails an empty selection clearly.</summary>
    [TestMethod]
    public void HostListsRunsAndRejectsMissingSelection()
    {
        using var output = new BenchOutputCapture();
        Assert.AreEqual(0, BenchHost.Run<BenchTestSuite>(["list"]));
        StringAssert.Contains(output.Text, "comparison candidate; baseline: Group/Compare/Base");
        Assert.AreEqual(0, BenchHost.Run<BenchTestSuite>(["list", "Plain"]));
        Assert.AreEqual(2, BenchHost.Run<BenchTestSuite>(["list", "Missing"]));
        StringAssert.Contains(output.Error, "No benchmarks matched");
        Assert.AreEqual(0, BenchHost.Run<BenchTestSuite>(["run", "Plain", "--quiet", "--warmup", "0", "--samples", "1"]));
    }
}
