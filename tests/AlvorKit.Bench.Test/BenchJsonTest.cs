namespace AlvorKit;

/// <summary>Reads exported JSON through a separate parser to verify the public result schema.</summary>
[TestClass]
[DoNotParallelize]
public class BenchJsonTest
{
    /// <summary>JSON stdout contains only structured output, with complete samples, allocation scopes, and comparison signs.</summary>
    [TestMethod]
    public void StdoutExportPreservesMeasurementsAndComparisons()
    {
        using var output = new BenchOutputCapture();
        Assert.AreEqual(0, BenchHost.Run<BenchTestSuite>(["run", "--warmup", "1", "--samples", "2", "--json", "-"]));
        using var document = System.Text.Json.JsonDocument.Parse(output.Text);
        var root = document.RootElement;
        Assert.AreEqual("Contract suite", root.GetProperty("suite").GetString());
        var cases = root.GetProperty("benchmarks");
        Assert.AreEqual(4, cases.GetArrayLength());
        Assert.AreEqual("baseline", cases[0].GetProperty("comparison").GetProperty("role").GetString());
        var candidate = cases[1];
        Assert.AreEqual(1, candidate.GetProperty("warmups").GetArrayLength());
        Assert.AreEqual(2, candidate.GetProperty("samples").GetArrayLength());
        Assert.AreEqual(2, candidate.GetProperty("samples")[0].GetProperty("number").GetInt32());
        var summary = candidate.GetProperty("summary");
        Assert.AreEqual(-50d, summary.GetProperty("baselinePercentDifference").GetDouble());
        Assert.AreEqual(10d, summary.GetProperty("meanAllocations").GetProperty("workloadBytes").GetDouble());
        Assert.AreEqual(System.Text.Json.JsonValueKind.Null, cases[3].GetProperty("comparison").ValueKind);
    }

    /// <summary>File export creates its directory and reports the absolute output path only in human-output mode.</summary>
    [TestMethod]
    public void FileExportWritesReadableDocument()
    {
        using var output = new BenchOutputCapture();
        using var workspace = TempWorkspace.Create();
        var path = Path.Combine(workspace.Root, "export", "result.json");
        Assert.AreEqual(0, BenchHost.Run<BenchTestSuite>(["run", "Plain", "--warmup", "0", "--samples", "1", "--json", path]));
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
        Assert.AreEqual(1, document.RootElement.GetProperty("benchmarks").GetArrayLength());
        StringAssert.Contains(output.Text, Path.GetFullPath(path));
    }
}
