namespace AlvorKit;

[TestClass]
public class BenchMeasurementTest
{
    /// <summary>Retaining scalar and larger value results adds no managed allocations to either measurement boundary.</summary>
    [TestMethod]
    public void ValueRetentionDoesNotAllocate()
    {
        MeasureValueRetention();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var result = MeasureValueRetention();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(0L, result.WorkloadAllocatedBytes);
        Assert.AreEqual(0L, allocated);
    }

    /// <summary>Small nonzero allocation measurements remain visible rather than rounding to zero.</summary>
    [TestMethod]
    [DoNotParallelize]
    public void ConsolePreservesSmallAllocationRates()
    {
        var original = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            var result = new BenchResult(TimeSpan.FromMilliseconds(1), 4000000, "op", "N0") { WorkloadAllocatedBytes = 8 };
            new BenchConsole().WriteMean(result, 1, 0, null, 0);
            StringAssert.Contains(output.ToString(), "2E-06 B/op");
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    /// <summary>The timed allocation counter excludes preparation and remains distinct from whole-measurement allocations.</summary>
    [TestMethod]
    public void WorkloadCounterExcludesFixtureAllocations()
    {
        var command = new BenchCommand(BenchCommandKind.Run, [], [], 1, 2, null, new(true, false, false));
        var runner = new BenchMeasurementRunner(command, new BenchConsole(), BenchAllocationTracking.Connect());
        var measurement = runner.Run(MeasureAllocation, 0, null);

        foreach (var sample in measurement.Samples)
        {
            Assert.IsTrue(sample.WorkloadAllocatedBytes >= 1024);
            Assert.IsTrue(sample.WorkloadAllocatedBytes < 2048);
            Assert.IsTrue(sample.AllocatedBytes >= 8192 + sample.WorkloadAllocatedBytes);
        }
    }

    /// <summary>Means include every sample's allocation count instead of reporting only the final sample.</summary>
    [TestMethod]
    public void MeanUsesAllAllocationSamples()
    {
        var command = new BenchCommand(BenchCommandKind.Run, [], [], 0, 3, null, new(true, false, false));
        var runner = new BenchMeasurementRunner(command, new BenchConsole(), BenchAllocationTracking.Connect());
        var count = 0;
        var measurement = runner.Run(() =>
            new(TimeSpan.FromMilliseconds(1), 1, "op", "N0") { WorkloadAllocatedBytes = ++count * 100 }, 0, null);
        Assert.AreEqual(200L, measurement.Summary.WorkloadAllocatedBytes);
    }

    /// <summary>Hierarchical globs distinguish one segment from recursive matches.</summary>
    [TestMethod]
    public void GlobsRespectPathSegments()
    {
        Assert.IsTrue(new BenchGlob("**/Set").Matches("Set"));
        Assert.IsTrue(new BenchGlob("**/Set").Matches("Sparse/Small/Set"));
        Assert.IsFalse(new BenchGlob("*/Set").Matches("Sparse/Small/Set"));
    }

    /// <summary>Mean memory trees preserve distinct siblings with identical labels.</summary>
    [TestMethod]
    public void MemoryMeansPreserveDuplicateLabels()
    {
        BenchMemoryUsage first = new("Root", 0, 192, 0, 192,
            [new("Map", 64, 64, 64, 64, []), new("Map", 128, 128, 128, 128, [])]);
        BenchMemoryUsage second = new("Root", 0, 192, 0, 192, [new("Map", 192, 192, 192, 192, [])]);
        var mean = BenchMemoryUsage.Mean([first, second]);
        Assert.AreEqual(2, mean.Children.Length);
        Assert.AreEqual(128d, mean.Children[0].TotalBytes);
        Assert.AreEqual(64d, mean.Children[1].TotalBytes);
    }

    private static BenchResult MeasureAllocation()
    {
        var fixture = new byte[8192];
        var timer = BenchTimer.Start();
        var payload = new byte[1024];
        var result = timer.Stop(1, "operation");
        GC.KeepAlive(fixture);
        GC.KeepAlive(payload);
        return result;
    }

    private static BenchResult MeasureValueRetention()
    {
        var timer = BenchTimer.Start();
        long retained = 0;

        for (var index = 0; index < 1000; index++)
            retained += index;

        var result = timer.Stop(1000, "operation");
        BenchRetain.Value(in retained);
        var larger = (retained, retained, retained, retained);
        BenchRetain.Value(in larger);
        return result;
    }
}
