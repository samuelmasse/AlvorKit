namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests small persistence, path, and result helpers.</summary>
[TestClass]
public sealed class CoreUtilitiesTest
{
    /// <summary>Output paths honor custom directories and create artifact folders.</summary>
    [TestMethod]
    public void OutputPaths_Create_ResolvesAndCreatesDirectories()
    {
        using var workspace = TempWorkspace.Create();

        var (runId, runDirectory, framesDirectory, logsDirectory) =
            OutputPaths.Create(workspace.Root, new() { RunId = "r1", Directory = "out/custom" });
        var absolute = OutputPaths.Resolve(workspace.Root, runDirectory);

        Assert.AreEqual("r1", runId);
        Assert.AreEqual(Path.Combine(workspace.Root, "out", "custom"), runDirectory);
        Assert.AreEqual(runDirectory, absolute);
        Assert.IsTrue(Directory.Exists(framesDirectory));
        Assert.IsTrue(Directory.Exists(logsDirectory));
    }

    /// <summary>Output paths fall back to the standard run directory when no custom output is provided.</summary>
    [TestMethod]
    public void OutputPaths_Create_DefaultsToAlvorEyeRuns()
    {
        using var workspace = TempWorkspace.Create();

        var (runId, runDirectory, _, _) = OutputPaths.Create(workspace.Root, new());

        StringAssert.StartsWith(runDirectory, Path.Combine(workspace.Root, "out", "alvoreye", "runs"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(runId));
    }

    /// <summary>Session store round-trips queued actions and reports missing sessions clearly.</summary>
    [TestMethod]
    public void SessionStore_SaveLoad_RoundTripsState()
    {
        using var workspace = TempWorkspace.Create();
        var store = new SessionStore(workspace.Root);
        var state = new SessionState
        {
            SessionId = "s1",
            RepoRoot = workspace.Root,
            RunDirectory = workspace.Root,
            WindowTitle = "window",
            ProcessId = 5,
            Frozen = true
        };
        state.QueuedActions.Add(new() { Kind = AlvorEyeActionKind.Key, Key = "Space" });

        store.Save(state);
        var loaded = store.Load("s1");

        Assert.AreEqual("window", loaded.WindowTitle);
        Assert.IsTrue(loaded.Frozen);
        Assert.AreEqual(1, loaded.QueuedActions.Count);
        Assert.ThrowsExactly<FileNotFoundException>(() => store.Load("missing"));

        File.WriteAllText(Path.Combine(store.SessionsDirectory, "null.json"), "null");
        Assert.ThrowsExactly<InvalidOperationException>(() => store.Load("null"));
    }

    /// <summary>Result and capability helpers expose their values.</summary>
    [TestMethod]
    public void ResultAndCapabilities_ExposeValues()
    {
        var result = AlvorEyeResult.Success("one", "two");
        var capabilities = new PlatformCapabilities
        {
            WindowDiscovery = true,
            WindowPlacement = true,
            Capture = true,
            Keyboard = true,
            Mouse = true,
            ProcessFreeze = true
        };
        var analysis = new BasicImageAnalysisResult(3, 4, true, 5, 6, 1, 2, 7, 8);
        using var process = Process.GetCurrentProcess();
        var context = new RunContext(
            "",
            new() { Window = new() { Title = "" }, Output = new() },
            new FakeAlvorEyePlatform(),
            new(""),
            new() { RunId = "", RunDirectory = "" },
            "")
        {
            Process = process
        };

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.AreEqual(new[] { "one", "two" }, result.Lines.ToArray());
        Assert.IsTrue(capabilities.WindowDiscovery);
        Assert.IsTrue(capabilities.WindowPlacement);
        Assert.IsTrue(capabilities.Capture);
        Assert.IsTrue(capabilities.Keyboard);
        Assert.IsTrue(capabilities.Mouse);
        Assert.IsTrue(capabilities.ProcessFreeze);
        Assert.AreEqual(3, analysis.Width);
        Assert.AreEqual(4, analysis.Height);
        Assert.AreEqual(5, analysis.ChangedPixels);
        Assert.AreSame(process, context.Process);
    }
}
