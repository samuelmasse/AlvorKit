namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests scenario and action JSON parsing.</summary>
[TestClass]
public sealed class AlvorEyeScenarioParserTest
{
    /// <summary>Parses scenario sections and supported actions.</summary>
    [TestMethod]
    public void Parse_FullScenario_ReturnsTypedModel()
    {
        var scenario = AlvorEyeScenarioParser.Parse("""
            {
              "run": { "executable": "dotnet", "args": ["run"], "environment": { "A": "B" } },
              "window": { "title": "AlvorKit testcube", "exact": true, "timeoutSeconds": 3, "width": 640 },
              "output": { "runId": "case-a", "directory": "out/custom" },
              "freeze": { "strategy": "processSuspend", "resumeSettleMilliseconds": 50 },
              "timeline": [
                { "action": "wait", "milliseconds": 25 },
                { "action": "capture", "name": "start" },
                { "action": "key", "key": "Space" },
                { "action": "keyDown", "key": "Shift" },
                { "action": "keyUp", "key": "Shift" },
                { "action": "text", "text": "go" },
                { "action": "mouseMove", "x": 1, "y": 2 },
                { "action": "mouseClick", "x": 3, "y": 4, "button": "right" },
                { "action": "mouseDrag", "x": 1, "y": 2, "toX": 3, "toY": 4 },
                { "action": "handoff" },
                { "action": "resume" },
                { "action": "analyzeBasic", "color": "#ff0000" }
              ]
            }
            """);

        Assert.AreEqual("dotnet", scenario.Run!.Executable);
        Assert.AreEqual("B", scenario.Run.Environment["A"]);
        Assert.AreEqual("AlvorKit testcube", scenario.Window.Title);
        Assert.IsTrue(scenario.Window.Exact);
        Assert.AreEqual("case-a", scenario.Output.RunId);
        Assert.AreEqual(TimeSpan.FromMilliseconds(50), scenario.Freeze.ResumeSettle);
        CollectionAssert.AreEqual(
            new[]
            {
                AlvorEyeActionKind.Wait,
                AlvorEyeActionKind.Capture,
                AlvorEyeActionKind.Key,
                AlvorEyeActionKind.KeyDown,
                AlvorEyeActionKind.KeyUp,
                AlvorEyeActionKind.Text,
                AlvorEyeActionKind.MouseMove,
                AlvorEyeActionKind.MouseClick,
                AlvorEyeActionKind.MouseDrag,
                AlvorEyeActionKind.Handoff,
                AlvorEyeActionKind.Resume,
                AlvorEyeActionKind.AnalyzeBasic
            },
            scenario.Timeline.Select(action => action.Kind).ToArray());
    }

    /// <summary>ParseFile resolves relative run working directories from the scenario file location.</summary>
    [TestMethod]
    public void ParseFile_RelativeWorkingDirectory_UsesScenarioDirectory()
    {
        using var workspace = TempWorkspace.Create();
        var scenarioDirectory = Path.Combine(workspace.Root, "scenarios");
        Directory.CreateDirectory(scenarioDirectory);
        var path = Path.Combine(scenarioDirectory, "case.json");
        File.WriteAllText(
            path,
            """
            {
              "run": { "executable": "dotnet", "workingDirectory": "game", "environment": [] },
              "window": { "title": "x" }
            }
            """);

        var scenario = AlvorEyeScenarioParser.ParseFile(path);

        Assert.AreEqual(Path.Combine(scenarioDirectory, "game"), scenario.Run!.WorkingDirectory);
        Assert.AreEqual(0, scenario.Run.Environment.Count);
    }

    /// <summary>Rejects scenarios missing required sections or action fields.</summary>
    [TestMethod]
    public void Parse_InvalidScenario_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("{}"));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("""{ "run": {}, "window": { "title": "x" } }"""));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("""{ "window": {} }"""));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("""{ "window": { "title": "x" }, "timeline": [{ "action": "key" }] }"""));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("""{ "window": { "title": "x" }, "timeline": [{ "kind": "text" }] }"""));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeScenarioParser.Parse("""{ "window": { "title": "x" }, "timeline": [{ "action": "wat" }] }"""));
    }

    /// <summary>Parses JSONL batches for session mode.</summary>
    [TestMethod]
    public void ParseJsonLine_ActionBatch_ReturnsActions()
    {
        var actions = AlvorEyeActionParser.ParseJsonLine("""{ "actions": [{ "action": "text", "text": "go" }, { "action": "capture" }] }""");

        Assert.AreEqual(2, actions.Count);
        Assert.AreEqual(AlvorEyeActionKind.Text, actions[0].Kind);
        Assert.AreEqual(AlvorEyeActionKind.Capture, actions[1].Kind);
    }

    /// <summary>Parses single JSONL actions and rejects malformed action arrays.</summary>
    [TestMethod]
    public void ParseJsonLine_SingleAction_ReturnsAction()
    {
        var actions = AlvorEyeActionParser.ParseJsonLine(
            """{ "kind": "handoff", "captureBeforeFreeze": false, "captureAfterFreeze": false }""");

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(AlvorEyeActionKind.Handoff, actions[0].Kind);
        Assert.IsFalse(actions[0].CaptureBeforeFreeze);
        Assert.IsFalse(actions[0].CaptureAfterFreeze);
        Assert.ThrowsExactly<ArgumentException>(() =>
            AlvorEyeActionParser.ParseActions(JsonDocument.Parse("""{}""").RootElement));
        Assert.ThrowsExactly<ArgumentException>(() => AlvorEyeActionParser.ParseJsonLine("""{}"""));
    }
}
