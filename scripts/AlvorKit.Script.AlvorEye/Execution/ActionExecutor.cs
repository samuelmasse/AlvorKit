namespace AlvorKit.Script.AlvorEye;

/// <summary>Executes scenario and session actions against a target window.</summary>
internal sealed class ActionExecutor
{
    /// <summary>Executes all actions in order.</summary>
    public async Task ExecuteAsync(RunContext context, IReadOnlyList<AlvorEyeAction> actions)
    {
        foreach (var action in actions)
            await ExecuteAsync(context, action);
    }

    /// <summary>Executes one action or queues it when a session is frozen.</summary>
    public async Task ExecuteAsync(RunContext context, AlvorEyeAction action)
    {
        if (ShouldQueue(context, action))
        {
            context.Session!.QueuedActions.Add(action);
            context.Manifest.Events.Add(Event(action, "queued"));
            context.SessionStore.Save(context.Session);
            return;
        }

        switch (action.Kind)
        {
            case AlvorEyeActionKind.Wait:
                await Task.Delay(action.Delay);
                break;
            case AlvorEyeActionKind.Capture:
                Capture(context, action.Name ?? "capture");
                break;
            case AlvorEyeActionKind.Key:
                context.Platform.SendKey(context.Target, action.Key!, KeyInputMode.Press);
                break;
            case AlvorEyeActionKind.KeyDown:
                context.Platform.SendKey(context.Target, action.Key!, KeyInputMode.Down);
                break;
            case AlvorEyeActionKind.KeyUp:
                context.Platform.SendKey(context.Target, action.Key!, KeyInputMode.Up);
                break;
            case AlvorEyeActionKind.Text:
                context.Platform.SendText(context.Target, action.Text!);
                break;
            case AlvorEyeActionKind.MouseMove:
                context.Platform.MoveMouse(context.Target, action.X, action.Y);
                break;
            case AlvorEyeActionKind.MouseClick:
                context.Platform.ClickMouse(context.Target, action.X, action.Y, action.Button!);
                break;
            case AlvorEyeActionKind.MouseDrag:
                context.Platform.DragMouse(context.Target, action.X, action.Y, action.ToX, action.ToY, action.Button!, action.Delay);
                break;
            case AlvorEyeActionKind.Handoff:
                Handoff(context, action);
                break;
            case AlvorEyeActionKind.Resume:
                await ResumeAsync(context);
                break;
            case AlvorEyeActionKind.AnalyzeBasic:
                Analyze(context, action);
                break;
        }

        context.Manifest.Events.Add(Event(action, "done"));
    }

    /// <summary>Captures a frame and records it in the manifest.</summary>
    private static string Capture(RunContext context, string name)
    {
        var frameName = $"frame-{context.FrameIndex++:000}-{SafeName(name)}.png";
        var path = Path.Combine(context.FramesDirectory, frameName);
        context.Platform.CaptureWindow(context.Target, path);
        context.LastFramePath = path;
        context.Manifest.Frames.Add(new(name, path));
        return path;
    }

    /// <summary>Freezes the target and captures handoff frames.</summary>
    private static void Handoff(RunContext context, AlvorEyeAction action)
    {
        if (action.CaptureBeforeFreeze)
            Capture(context, action.Name ?? "handoff-before");
        context.Platform.FreezeProcess(context.Target.ProcessId);
        if (context.Session is not null)
        {
            context.Session.Frozen = true;
            context.SessionStore.Save(context.Session);
        }

        if (action.CaptureAfterFreeze)
            Capture(context, action.Name ?? "handoff-after");
    }

    /// <summary>Resumes the target and drains queued session actions.</summary>
    private async Task ResumeAsync(RunContext context)
    {
        context.Platform.ResumeProcess(context.Target.ProcessId);
        if (context.Session is null)
            return;

        context.Session.Frozen = false;
        await Task.Delay(context.Scenario.Freeze.ResumeSettle);
        var queued = context.Session.QueuedActions.ToArray();
        context.Session.QueuedActions.Clear();
        context.SessionStore.Save(context.Session);
        foreach (var queuedAction in queued)
            await ExecuteAsync(context, queuedAction);
    }

    /// <summary>Runs basic image analysis and writes a JSON sidecar.</summary>
    private static void Analyze(RunContext context, AlvorEyeAction action)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            throw new PlatformNotSupportedException("AlvorEye image analysis v1 uses the Windows System.Drawing backend.");

        var framePath = context.LastFramePath ?? throw new InvalidOperationException("No frame has been captured yet.");
        var comparePath = action.CompareTo is null ? null : OutputPaths.Resolve(context.RepoRoot, action.CompareTo);
        var result = BasicImageAnalysis.Analyze(framePath, comparePath, action.Color);
        var path = Path.ChangeExtension(framePath, ".analysis.json");
        File.WriteAllText(path, JsonSerializer.Serialize(result, ScenarioJson.Options), Encoding.UTF8);
        context.Manifest.Events.Add(new(DateTimeOffset.UtcNow, "analyzeBasic", "done", path));
    }

    /// <summary>Determines whether an action should be queued during a frozen session.</summary>
    private static bool ShouldQueue(RunContext context, AlvorEyeAction action) =>
        context.Session?.Frozen == true &&
        action.Kind is not (AlvorEyeActionKind.Capture or AlvorEyeActionKind.AnalyzeBasic or AlvorEyeActionKind.Resume);

    /// <summary>Creates a manifest event for one action.</summary>
    private static ManifestEvent Event(AlvorEyeAction action, string status) =>
        new(DateTimeOffset.UtcNow, action.Kind.ToString(), status);

    /// <summary>Builds a file-safe stem for a frame name.</summary>
    private static string SafeName(string name) =>
        string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character));
}
