namespace AlvorKit.Script.TestTiming.Test;

/// <summary>Tests timing report output.</summary>
[TestClass]
public sealed class TestTimingReportWriterTest
{
    /// <summary>Reports include slow-test counts, escaped names, and all results in CSV form.</summary>
    [TestMethod]
    public void Write_WithSlowTests_WritesMarkdownAndCsv()
    {
        using var workspace = TempWorkspace.Create();
        var results = new[]
        {
            new TestTimingResult("Fast", "Passed", TimeSpan.FromMilliseconds(10), "fast.trx"),
            new TestTimingResult("Slow|Name", "Passed", TimeSpan.FromMilliseconds(150), "slow.trx")
        };

        var report = new TestTimingReportWriter().Write(results, TimeSpan.FromMilliseconds(100), workspace.Root);
        var markdown = File.ReadAllText(report.MarkdownPath);
        var csv = File.ReadAllText(report.CsvPath);

        Assert.AreEqual(2, report.TotalCount);
        Assert.AreEqual(1, report.SlowResults.Count);
        StringAssert.Contains(markdown, "- Tests over threshold: 1");
        StringAssert.Contains(markdown, "Slow\\|Name");
        StringAssert.Contains(csv, "\"Slow|Name\"");
        StringAssert.Contains(csv, "\"slow.trx\"");
    }

    /// <summary>Reports explicitly say when no tests exceeded the timing budget.</summary>
    [TestMethod]
    public void Write_WithoutSlowTests_WritesNoneSection()
    {
        using var workspace = TempWorkspace.Create();
        var results = new[] { new TestTimingResult("Fast", "Passed", TimeSpan.FromMilliseconds(10), "fast.trx") };

        var report = new TestTimingReportWriter().Write(results, TimeSpan.FromMilliseconds(100), workspace.Root);
        var markdown = File.ReadAllText(report.MarkdownPath);

        Assert.AreEqual(0, report.SlowResults.Count);
        StringAssert.Contains(markdown, "## Slow Tests");
        StringAssert.Contains(markdown, "None.");
    }
}
