namespace AlvorKit;

/// <summary>Protects readable allocation reports and sparse memory-tree aggregation.</summary>
[TestClass]
[DoNotParallelize]
public class BenchPresentationTest
{
    /// <summary>Memory trees show each sibling and distinguish payload from allocator overhead across byte scales.</summary>
    [TestMethod]
    public void MemoryOutputPreservesHierarchyAndUnits()
    {
        using var output = new BenchOutputCapture();
        BenchMemoryUsage root = new("Root", 120, 1200, 100, 1000,
            [new("Small", 100, 100, 80, 80, []), new("Large", 1000, 1000, 800, 800, [])]);
        new BenchConsole().WriteMemoryTree(root, 0, false);
        StringAssert.Contains(output.Text, "├─ Small");
        StringAssert.Contains(output.Text, "└─ Large");
        StringAssert.Contains(output.Text, "payload");
        StringAssert.Contains(output.Text, "overhead");
        Assert.AreEqual(20d, root.SelfOverheadBytes);
        Assert.AreEqual(200d, root.TotalOverheadBytes);
        Assert.AreEqual("1.0 KB", BenchFormatting.FormatBytes(1024));
        Assert.AreEqual("1.0 MB", BenchFormatting.FormatBytes(1024 * 1024));
        Assert.AreEqual("1.0 GB", BenchFormatting.FormatBytes(1024L * 1024 * 1024));
        Assert.AreEqual("1.0 TB", BenchFormatting.FormatBytes(1024L * 1024 * 1024 * 1024));
    }

    /// <summary>An absent subtree contributes zero to every descendant instead of dropping it from the average.</summary>
    [TestMethod]
    public void MissingMemorySubtreesContributeZero()
    {
        BenchMemoryUsage first = new("Root", 10, 110, 5, 55,
            [new("Branch", 20, 100, 10, 50, [new("Leaf", 80, 80, 40, 40, [])])]);
        BenchMemoryUsage second = new("Root", 10, 10, 5, 5, []);
        var mean = BenchMemoryUsage.Mean([first, second]);
        Assert.AreEqual(60d, mean.TotalBytes);
        Assert.AreEqual(50d, mean.Children[0].TotalBytes);
        Assert.AreEqual(40d, mean.Children[0].Children[0].TotalBytes);
        Assert.AreEqual(20d, mean.Children[0].Children[0].TotalRequestedBytes);
    }

    /// <summary>Final samples erase longer transient lines without losing allocation values or comparison direction.</summary>
    [TestMethod]
    public void FinalSamplesReplaceTransientOutput()
    {
        using var output = new BenchOutputCapture();
        var console = new BenchConsole();
        var result = BenchTestSuite.Result(10);
        console.WriteRetained(result, 1, 1, 0, 200);
        console.WriteMean(result, 1, 0, 2, 1);
        StringAssert.Contains(output.Text, "1 B/op");
        StringAssert.Contains(output.Text, "100.00% slower");
        console.WriteMean(result with { WorkloadAllocatedBytes = null }, 1, 0, null, 0);
        var benchmark = new BenchCase("Parent/Leaf", "leaf description", () => result, BenchCaseRole.Measurement, null);
        var depth = console.WriteCase(benchmark, [], out var segments);
        Assert.AreEqual(1, depth);
        CollectionAssert.AreEqual(new[] { "Parent", "Leaf" }, segments);
        StringAssert.Contains(output.Text, "Leaf: leaf description");
    }
}
