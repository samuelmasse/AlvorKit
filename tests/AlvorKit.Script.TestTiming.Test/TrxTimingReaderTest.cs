namespace AlvorKit.Script.TestTiming.Test;

/// <summary>Tests MSTest TRX timing parsing.</summary>
[TestClass]
public sealed class TrxTimingReaderTest
{
    /// <summary>TRX unit test result elements are parsed and ordered by duration.</summary>
    [TestMethod]
    public void ReadFiles_ParsesUnitTestResults()
    {
        using var workspace = TempWorkspace.Create();
        var trxPath = Path.Combine(workspace.Root, "run.trx");
        File.WriteAllText(
            trxPath,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
              <Results>
                <UnitTestResult testName="FastTest" outcome="Passed" duration="00:00:00.0100000" />
                <UnitTestResult testName="Slow|Test" outcome="Failed" duration="00:00:00.2500000" />
                <UnitTestResult testName="NoDuration" outcome="Passed" />
              </Results>
            </TestRun>
            """);

        var results = new TrxTimingReader().ReadFiles([trxPath]);

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual("Slow|Test", results[0].TestName);
        Assert.AreEqual("Failed", results[0].Outcome);
        Assert.AreEqual(TimeSpan.FromMilliseconds(250), results[0].Duration);
        Assert.AreEqual(trxPath, results[0].SourcePath);
        Assert.AreEqual("FastTest", results[1].TestName);
    }

    /// <summary>Malformed TRX result elements are skipped without blocking valid timings.</summary>
    [TestMethod]
    public void ReadFiles_SkipsMalformedResults()
    {
        using var workspace = TempWorkspace.Create();
        var trxPath = Path.Combine(workspace.Root, "run.trx");
        File.WriteAllText(
            trxPath,
            """
            <TestRun>
              <Results>
                <UnitTestResult outcome="Passed" duration="00:00:00.0100000" />
                <UnitTestResult testName="BadDuration" outcome="Passed" duration="later" />
                <UnitTestResult testName="Valid" outcome="Passed" duration="00:00:00.0200000" />
              </Results>
            </TestRun>
            """);

        var results = new TrxTimingReader().ReadFiles([trxPath]);

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Valid", results[0].TestName);
    }
}
