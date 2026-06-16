namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests action execution state around handoff and resume.</summary>
[TestClass]
public sealed class ActionExecutorTest
{
    /// <summary>Frozen sessions queue active input instead of sending it immediately.</summary>
    [TestMethod]
    public async Task ExecuteAsync_FrozenInput_QueuesAction()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);
        context.Session!.Frozen = true;

        await new ActionExecutor().ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Key, Key = "A" });

        Assert.AreEqual(1, context.Session.QueuedActions.Count);
        Assert.AreEqual(0, platform.Calls.Count);
    }

    /// <summary>Resume unfreezes the target and drains queued input.</summary>
    [TestMethod]
    public async Task ExecuteAsync_Resume_DrainsQueuedActions()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);
        context.Session!.Frozen = true;
        context.Session.QueuedActions.Add(new() { Kind = AlvorEyeActionKind.Key, Key = "A" });

        await new ActionExecutor().ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Resume });

        CollectionAssert.AreEqual(new[] { "resume:123", "key:A:Press" }, platform.Calls.ToArray());
        Assert.IsFalse(context.Session.Frozen);
        Assert.AreEqual(0, context.Session.QueuedActions.Count);
    }

    /// <summary>Handoff captures and freezes the target.</summary>
    [TestMethod]
    public async Task ExecuteAsync_Handoff_CapturesAndFreezes()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);

        await new ActionExecutor().ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Handoff });

        CollectionAssert.AreEqual(new[] { "capture", "freeze:123", "capture" }, platform.Calls.ToArray());
        Assert.IsTrue(context.Session!.Frozen);
        Assert.AreEqual(2, context.Manifest.Frames.Count);
    }

    /// <summary>Handoff capture flags let scenarios freeze without taking extra frames.</summary>
    [TestMethod]
    public async Task ExecuteAsync_HandoffWithoutCaptures_FreezesOnly()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);

        await new ActionExecutor().ExecuteAsync(
            context,
            new AlvorEyeAction
            {
                Kind = AlvorEyeActionKind.Handoff,
                CaptureBeforeFreeze = false,
                CaptureAfterFreeze = false
            });

        CollectionAssert.AreEqual(new[] { "freeze:123" }, platform.Calls.ToArray());
        Assert.AreEqual(0, context.Manifest.Frames.Count);
    }

    /// <summary>Active actions dispatch to the platform and record captures.</summary>
    [TestMethod]
    public async Task ExecuteAsync_ActiveActions_DispatchToPlatform()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);
        var actions = new[]
        {
            new AlvorEyeAction { Kind = AlvorEyeActionKind.Wait, Delay = TimeSpan.Zero },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.Capture, Name = "start:frame" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.Key, Key = "Enter" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.KeyDown, Key = "Shift" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.KeyUp, Key = "Shift" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.Text, Text = "go" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.MouseMove, X = 10, Y = 20 },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.MouseClick, X = 10, Y = 20, Button = "left" },
            new AlvorEyeAction { Kind = AlvorEyeActionKind.MouseDrag, X = 1, Y = 2, ToX = 3, ToY = 4, Button = "right" }
        };

        await new ActionExecutor().ExecuteAsync(context, actions);

        CollectionAssert.AreEqual(
            new[]
            {
                "capture",
                "key:Enter:Press",
                "key:Shift:Down",
                "key:Shift:Up",
                "text:go",
                "move:10:20",
                "click:left",
                "drag:right"
            },
            platform.Calls.ToArray());
        Assert.AreEqual(1, context.Manifest.Frames.Count);
        Assert.IsTrue(File.Exists(context.LastFramePath));
        StringAssert.Contains(Path.GetFileName(context.LastFramePath), "start-frame");
    }

    /// <summary>Basic analysis writes a sidecar for the last captured frame.</summary>
    [TestMethod]
    public async Task ExecuteAsync_AnalyzeBasic_WritesSidecar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            Assert.Inconclusive("System.Drawing image analysis is Windows-only in v1.");

        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);
        var baseline = Path.Combine(workspace.Root, "baseline.png");
        var current = Path.Combine(workspace.Root, "current.png");
        TestPng.WriteBlack(baseline);
        TestPng.WriteRed(current);
        context.LastFramePath = current;

        await new ActionExecutor().ExecuteAsync(
            context,
            new AlvorEyeAction { Kind = AlvorEyeActionKind.AnalyzeBasic, CompareTo = baseline, Color = "#ff0000" });

        var path = Path.ChangeExtension(current, ".analysis.json");
        var result = JsonSerializer.Deserialize<BasicImageAnalysisResult>(File.ReadAllText(path), ScenarioJson.Options);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.NonBlank);
        Assert.IsTrue(result.ChangedPixels > 0);
        Assert.IsTrue(result.ColorHits > 0);
    }

    /// <summary>Analysis reports a clear error when no capture exists.</summary>
    [TestMethod]
    public async Task ExecuteAsync_AnalyzeBasicWithoutFrame_Throws()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            Assert.Inconclusive("System.Drawing image analysis is Windows-only in v1.");

        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => new ActionExecutor().ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.AnalyzeBasic }));
    }

    /// <summary>Resume can be used in one-shot timelines without persistent session state.</summary>
    [TestMethod]
    public async Task ExecuteAsync_ResumeWithoutSession_ResumesProcessOnly()
    {
        using var workspace = TempWorkspace.Create();
        var platform = new FakeAlvorEyePlatform();
        var context = Context(workspace.Root, platform);
        context.Session = null;

        await new ActionExecutor().ExecuteAsync(context, new AlvorEyeAction { Kind = AlvorEyeActionKind.Resume });

        CollectionAssert.AreEqual(new[] { "resume:123" }, platform.Calls.ToArray());
    }

    /// <summary>Builds a minimal session run context.</summary>
    private static RunContext Context(string root, FakeAlvorEyePlatform platform)
    {
        var scenario = new AlvorEyeScenario { Window = new() { Title = "window" }, Output = new() };
        var manifest = new RunManifest { RunId = "s1", SessionId = "s1", RunDirectory = root };
        var context = new RunContext(root, scenario, platform, new(root), manifest, Path.Combine(root, "frames"))
        {
            Target = new(1, "window", 123),
            Session = new()
            {
                SessionId = "s1",
                RepoRoot = root,
                RunDirectory = root,
                WindowTitle = "window",
                ProcessId = 123
            }
        };
        return context;
    }
}
