using System.Text.Json;

namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for aggregating raw Coverlet JSON into agent-readable summaries.</summary>
[TestClass]
public sealed class CoverageAccumulatorTest
{
    /// <summary>Repeated reports for the same item are merged before percentages are calculated.</summary>
    [TestMethod]
    public void BuildSummary_DuplicateCoverageItems_AggregatesHits()
    {
        using var workspace = TempWorkspace.Create();
        var document = workspace.PathFor("scripts", "Tool", "Source.cs");
        var accumulator = new CoverageAccumulator();

        accumulator.AddCoverletJson(workspace.Write("first.json", ReportJson(document, line10Hits: 1, line11Hits: 0, branchHits: 0)), workspace.Root);
        accumulator.AddCoverletJson(workspace.Write("second.json", ReportJson(document, line10Hits: 0, line11Hits: 3, branchHits: 2)), workspace.Root);
        var summary = accumulator.BuildSummary(["Tool"]);

        Assert.AreEqual(2, summary.Totals.Line.Total);
        Assert.AreEqual(2, summary.Totals.Line.Covered);
        Assert.AreEqual(1, summary.Totals.Branch.Total);
        Assert.AreEqual(1, summary.Totals.Branch.Covered);
        Assert.AreEqual(1, summary.Totals.Method.Total);
        Assert.AreEqual(1, summary.Totals.Method.Covered);
    }

    /// <summary>Missing lines and source projects with no measured assembly are reported explicitly.</summary>
    [TestMethod]
    public void BuildSummary_MissingCoverage_ReportsDetails()
    {
        using var workspace = TempWorkspace.Create();
        var document = workspace.PathFor("scripts", "Tool", "Source.cs");
        var accumulator = new CoverageAccumulator();

        accumulator.AddCoverletJson(workspace.Write("coverage.json", ReportJson(document, line10Hits: 1, line11Hits: 0, branchHits: 0)), workspace.Root);
        var summary = accumulator.BuildSummary(["MissingTool", "Tool"]);
        var file = summary.Files.Single();

        CollectionAssert.AreEqual(new[] { "MissingTool" }, summary.UnmeasuredModules.ToArray());
        Assert.AreEqual("scripts/Tool/Source.cs", file.Path);
        CollectionAssert.AreEqual(new[] { 11 }, file.MissingLineNumbers.ToArray());
        CollectionAssert.AreEqual(new[] { 10 }, file.MissingBranchLineNumbers.ToArray());
    }

    /// <summary>Creates a minimal Coverlet JSON document with configurable hit counts.</summary>
    private static string ReportJson(string document, int line10Hits, int line11Hits, int branchHits)
    {
        var documentName = JsonSerializer.Serialize(document);

        return $$"""
        {
          "Tool.dll": {
            {{documentName}}: {
              "Tool.Type": {
                "System.Void Tool.Type::Run()": {
                  "Lines": {
                    "10": {{line10Hits}},
                    "11": {{line11Hits}}
                  },
                  "Branches": [
                    {
                      "Line": 10,
                      "Offset": 1,
                      "EndOffset": 2,
                      "Path": 0,
                      "Ordinal": 0,
                      "Hits": {{branchHits}}
                    }
                  ]
                }
              }
            }
          }
        }
        """;
    }
}
