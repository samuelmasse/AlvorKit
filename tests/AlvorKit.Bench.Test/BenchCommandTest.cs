namespace AlvorKit;

/// <summary>Exercises the public command parser and hierarchical benchmark selection.</summary>
[TestClass]
[DoNotParallelize]
public class BenchCommandTest
{
    /// <summary>Bare invocation and explicit commands preserve defaults, selection, and output options.</summary>
    [TestMethod]
    public void CommandsPreserveRequestedOptions()
    {
        BenchCommand? captured = null;
        var parser = new BenchCommandLine(command => { captured = command; return 17; });
        Assert.AreEqual(17, parser.Invoke([]));
        Assert.AreEqual(BenchCommandKind.Run, captured!.Kind);
        Assert.AreEqual(11, captured.WarmupCount);
        Assert.AreEqual(5, captured.SampleCount);
        Assert.IsFalse(captured.Display.Quiet);
        Assert.AreEqual(17, parser.Invoke(["list", "**/A", "--exclude", "Slow/**"]));
        Assert.AreEqual(BenchCommandKind.List, captured.Kind);
        CollectionAssert.AreEqual(new[] { "**/A" }, captured.IncludePatterns);
        CollectionAssert.AreEqual(new[] { "Slow/**" }, captured.ExcludePatterns);
        Assert.AreEqual(17, parser.Invoke(["run", "A", "B", "--warmup", "0", "--samples", "2",
            "--json", "-", "--show-samples", "--memory-tree", "--exclude", "C", "--exclude", "D"]));
        Assert.AreEqual(0, captured.WarmupCount);
        Assert.AreEqual(2, captured.SampleCount);
        Assert.IsTrue(captured.Display.Quiet && captured.Display.ShowSamples && captured.Display.ShowMemoryTree);
        Assert.AreEqual("-", captured.JsonPath);
        CollectionAssert.AreEqual(new[] { "A", "B" }, captured.IncludePatterns);
        CollectionAssert.AreEqual(new[] { "C", "D" }, captured.ExcludePatterns);
        Assert.AreEqual(17, parser.Invoke(["run", "--quiet"]));
        Assert.IsTrue(captured.Display.Quiet);
        Assert.AreEqual(17, parser.Invoke(["run"]));
        Assert.IsFalse(captured.Display.Quiet);
        Assert.IsFalse(captured.Display.ShowSamples);
    }

    /// <summary>Invalid counts and unknown options fail before any benchmark can execute.</summary>
    [TestMethod]
    public void InvalidCommandsNeverExecute()
    {
        using var output = new BenchOutputCapture();
        var calls = 0;
        var parser = new BenchCommandLine(command => ++calls);
        Assert.AreNotEqual(0, parser.Invoke(["run", "--samples", "0"]));
        Assert.AreNotEqual(0, parser.Invoke(["run", "--warmup", "-1"]));
        Assert.AreNotEqual(0, parser.Invoke(["--unknown"]));
        Assert.AreEqual(0, parser.Invoke(["--help"]));
        Assert.AreEqual(0, calls);
        StringAssert.Contains(output.Error, "Sample count must be positive");
        StringAssert.Contains(output.Error, "Warmup count must be non-negative");
    }

    /// <summary>Include globs are unioned, exclusions win, and regular expression punctuation remains literal.</summary>
    [TestMethod]
    public void SelectionRetainsCatalogOrderAndHonorsExclusions()
    {
        var catalog = new BenchCatalogBuilder().Create(new BenchTestSuite().Create());
        var selector = new BenchSelector();
        Assert.AreEqual(catalog.Cases.Length, selector.Select(catalog, [], []).Length);
        var selected = selector.Select(catalog, ["Group/**", "Plain"], ["**/Candidate"]);
        CollectionAssert.AreEqual(new[] { "Group/Compare/Base", "Group/Read", "Plain" }, selected.Select(c => c.Id).ToArray());
        Assert.AreEqual(0, selector.Select(catalog, ["missing"], []).Length);
        Assert.IsTrue(new BenchGlob("A.?/*").Matches("A.x/Value"));
        Assert.IsFalse(new BenchGlob("A.?/*").Matches("ABx/Value"));
        Assert.IsFalse(new BenchGlob("A/?").Matches("A//"));
        Assert.IsTrue(new BenchGlob("**").Matches("A/B/C"));
    }
}
