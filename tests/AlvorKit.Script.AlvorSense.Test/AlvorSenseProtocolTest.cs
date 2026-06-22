namespace AlvorKit.Script.AlvorSense.Test;

/// <summary>Tests the JSON protocol and filesystem mailbox used by AlvorSense sessions.</summary>
[TestClass]
public sealed class AlvorSenseProtocolTest
{
    /// <summary>Path helpers keep all session files underneath the shared session root.</summary>
    [TestMethod]
    public void Paths_ReturnExpectedSessionFiles()
    {
        var sessionDir = AlvorSensePaths.SessionDir("game-1");

        Assert.AreEqual(Path.Combine("out", "alvorsense-sessions", "game-1"), sessionDir);
        Assert.AreEqual(Path.Combine(sessionDir, "session.json"), AlvorSensePaths.Manifest(sessionDir));
        Assert.AreEqual(Path.Combine(sessionDir, "ready.json"), AlvorSensePaths.Ready(sessionDir));
        Assert.AreEqual(Path.Combine(sessionDir, "host-runtime"), AlvorSensePaths.HostRuntime(sessionDir));
        Assert.AreEqual(Path.Combine(sessionDir, "requests", "abc.tmp"), AlvorSensePaths.RequestTemp(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "requests", "abc.json"), AlvorSensePaths.Request(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "responses", "abc.json"), AlvorSensePaths.Response(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "stdout.log"), AlvorSensePaths.Stdout(sessionDir));
        Assert.AreEqual(Path.Combine(sessionDir, "stderr.log"), AlvorSensePaths.Stderr(sessionDir));
    }

    /// <summary>Start results serialize the foreground session details in camel-case JSON.</summary>
    [TestMethod]
    public void StartResult_ToJson_WritesSessionDetails()
    {
        var json = AlvorSenseJson.ToJson(new AlvorSenseStartResult("game-1", "out/session", Ready: true, ProcessId: 123));

        StringAssert.Contains(json, "\"id\": \"game-1\"");
        StringAssert.Contains(json, "\"sessionDir\": \"out/session\"");
        StringAssert.Contains(json, "\"ready\": true");
        StringAssert.Contains(json, "\"processId\": 123");
    }

    /// <summary>Foreground send results omit stderr tails unless diagnostics were requested.</summary>
    [TestMethod]
    public void SendResult_ToJson_OnlyWritesRequestedStderrTail()
    {
        var response = new AlvorSenseResponse(
            "abc",
            Ok: false,
            CommandCount: 1,
            StateLine: null,
            OutputLines: [],
            ProcessExited: true,
            ExitCode: 2,
            Error: "Target exited.");

        var withoutTail = AlvorSenseJson.ToJson(AlvorSenseSendResult.From(response, null));
        var withTail = AlvorSenseJson.ToJson(AlvorSenseSendResult.From(response, ["first", "second"]));

        Assert.IsFalse(withoutTail.Contains("stderrTail", StringComparison.Ordinal));
        StringAssert.Contains(withTail, "\"stderrTail\": [");
        StringAssert.Contains(withTail, "\"second\"");
    }

    /// <summary>Foreground send responses include requested stderr tails only for failed sends whose target exited.</summary>
    [TestMethod]
    public void ForegroundResponses_WriteSend_AddsRequestedExitedTargetStderrTail()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        Directory.CreateDirectory(sessionDir);
        File.WriteAllText(AlvorSensePaths.Stderr(sessionDir), "one\ntwo\nthree\n", Encoding.UTF8);
        using var output = new StringWriter();
        var response = new AlvorSenseResponse(
            "abc",
            Ok: false,
            CommandCount: 1,
            StateLine: null,
            OutputLines: [],
            ProcessExited: true,
            ExitCode: 1,
            Error: "Target exited.");
        var command = new AlvorSenseSendCommand("game-1", ["render"], TimeSpan.FromSeconds(1), StderrTailLines: 2);

        AlvorSenseForegroundResponses.WriteSend(response, command, sessionDir, output);

        var text = output.ToString();
        StringAssert.Contains(text, "\"stderrTail\": [");
        Assert.IsFalse(text.Contains("\"one\"", StringComparison.Ordinal));
        StringAssert.Contains(text, "\"two\"");
        StringAssert.Contains(text, "\"three\"");
    }

    /// <summary>Foreground send responses omit stderr tails when the target is still running or the request succeeded.</summary>
    [TestMethod]
    public void ForegroundResponses_WriteSend_OmitsStderrTailWhenNotUseful()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        Directory.CreateDirectory(sessionDir);
        File.WriteAllText(AlvorSensePaths.Stderr(sessionDir), "one\ntwo\n", Encoding.UTF8);
        var command = new AlvorSenseSendCommand("game-1", ["render"], TimeSpan.FromSeconds(1), StderrTailLines: 2);

        using var successOutput = new StringWriter();
        AlvorSenseForegroundResponses.WriteSend(SendResponse(ok: true, exited: true), command, sessionDir, successOutput);

        using var runningOutput = new StringWriter();
        AlvorSenseForegroundResponses.WriteSend(SendResponse(ok: false, exited: false), command, sessionDir, runningOutput);

        Assert.IsFalse(successOutput.ToString().Contains("stderrTail", StringComparison.Ordinal));
        Assert.IsFalse(runningOutput.ToString().Contains("stderrTail", StringComparison.Ordinal));
    }

    /// <summary>Log tails return the latest requested lines from UTF-8 files.</summary>
    [TestMethod]
    public void LogTail_Read_ReturnsLatestLines()
    {
        using var workspace = TempWorkspace.Create();
        var log = workspace.Write("stderr.log", "one\ntwo\nthree\n");

        CollectionAssert.AreEqual(new[] { "two", "three" }, AlvorSenseLogTail.Read(log, 2));
        CollectionAssert.AreEqual(Array.Empty<string>(), AlvorSenseLogTail.Read(log, 0));
        CollectionAssert.AreEqual(Array.Empty<string>(), AlvorSenseLogTail.Read(workspace.PathFor("missing.log"), 2));
    }

    /// <summary>Target failure text distinguishes timeouts from targets that exited before answering.</summary>
    [TestMethod]
    public void TargetFailure_Message_DescribesExitedTargets()
    {
        Assert.IsNull(AlvorSenseTargetFailure.Message(observed: true, targetExited: true, exitCode: 2));
        Assert.AreEqual("Timed out waiting for target.", AlvorSenseTargetFailure.Message(observed: false, targetExited: false, exitCode: null));
        Assert.AreEqual(
            "Target exited before the expected response.",
            AlvorSenseTargetFailure.Message(observed: false, targetExited: true, exitCode: null));
        Assert.AreEqual(
            "Target exited before the expected response (exit code 2).",
            AlvorSenseTargetFailure.Message(observed: false, targetExited: true, exitCode: 2));
    }

    /// <summary>Protocol JSON round-trips the camel-cased wire format used by session files.</summary>
    [TestMethod]
    public void Json_SaveAndLoad_RoundTripsProtocolValues()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("session", "request.json");
        var request = new AlvorSenseRequest("abc", ["render", "state"], Stop: false, AppendState: true);

        AlvorSenseJson.Save(path, request);
        var text = File.ReadAllText(path, Encoding.UTF8);
        var loaded = AlvorSenseJson.Load<AlvorSenseRequest>(path);

        StringAssert.Contains(text, "\"appendState\": true");
        Assert.AreEqual(request.Id, loaded.Id);
        CollectionAssert.AreEqual(request.Commands, loaded.Commands);
        Assert.AreEqual(request.Stop, loaded.Stop);
        Assert.AreEqual(request.AppendState, loaded.AppendState);
    }

    /// <summary>Invalid JSON files fail with a targeted protocol error.</summary>
    [TestMethod]
    public void Json_LoadInvalidJson_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.Write("broken.json", "null");

        var exception = Assert.ThrowsException<InvalidOperationException>(() => AlvorSenseJson.Load<AlvorSenseRequest>(path));

        StringAssert.Contains(exception.Message, "Invalid JSON");
    }

    /// <summary>Waiting for a response returns the response file when it is already present.</summary>
    [TestMethod]
    public void RequestStore_WaitResponse_ReturnsResponse()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        var response = new AlvorSenseResponse(
            "abc",
            Ok: true,
            CommandCount: 1,
            StateLine: "time=0.016",
            OutputLines: ["time=0.016"],
            ProcessExited: false,
            ExitCode: null,
            Error: null);
        AlvorSenseJson.Save(AlvorSensePaths.Response(sessionDir, "abc"), response);

        var loaded = AlvorSenseRequestStore.WaitResponse(sessionDir, "abc", TimeSpan.FromSeconds(1));

        Assert.AreEqual(response.Id, loaded.Id);
        Assert.IsTrue(loaded.Ok);
        Assert.AreEqual(1, loaded.CommandCount);
        Assert.AreEqual("time=0.016", loaded.StateLine);
        CollectionAssert.AreEqual(response.OutputLines, loaded.OutputLines);
    }

    /// <summary>Sending a request atomically writes a request file and returns the matching response.</summary>
    [TestMethod]
    public async Task RequestStore_Send_WritesRequestAndReadsResponse()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        Directory.CreateDirectory(Path.Combine(sessionDir, "requests"));
        Directory.CreateDirectory(Path.Combine(sessionDir, "responses"));
        AlvorSenseJson.Save(AlvorSensePaths.Ready(sessionDir), new { ready = true });
        var request = new AlvorSenseRequest("abc", ["state"], Stop: false, AppendState: true);

        var responder = Task.Run(() =>
        {
            while (!File.Exists(AlvorSensePaths.Request(sessionDir, "abc")))
                Thread.Sleep(10);

            var written = AlvorSenseJson.Load<AlvorSenseRequest>(AlvorSensePaths.Request(sessionDir, "abc"));
            var response = new AlvorSenseResponse(
                written.Id,
                Ok: true,
                CommandCount: written.Commands.Length,
                StateLine: "time=0",
                OutputLines: ["time=0"],
                ProcessExited: false,
                ExitCode: null,
                Error: null);
            AlvorSenseJson.Save(AlvorSensePaths.Response(sessionDir, written.Id), response);
        });

        var loaded = AlvorSenseRequestStore.Send(sessionDir, request, TimeSpan.FromSeconds(2));
        await responder;

        Assert.IsTrue(loaded.Ok);
        Assert.IsFalse(File.Exists(AlvorSensePaths.RequestTemp(sessionDir, "abc")));
        Assert.IsTrue(File.Exists(AlvorSensePaths.Request(sessionDir, "abc")));
    }

    /// <summary>Sending to a session without a ready marker fails before writing a request.</summary>
    [TestMethod]
    public void RequestStore_SendWithoutReady_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        var request = new AlvorSenseRequest("abc", ["state"], Stop: false, AppendState: true);

        Assert.ThrowsExactly<InvalidOperationException>(() => AlvorSenseRequestStore.Send(sessionDir, request, TimeSpan.FromMilliseconds(1)));
    }

    /// <summary>Waiting for a response times out when the host does not answer.</summary>
    [TestMethod]
    public void RequestStore_WaitResponseTimeout_Throws()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsExactly<TimeoutException>(() => AlvorSenseRequestStore.WaitResponse(workspace.Root, "missing", TimeSpan.FromMilliseconds(1)));
    }

    /// <summary>Session registry reads ready markers and mailbox counts from persisted session directories.</summary>
    [TestMethod]
    public void SessionRegistry_ListAndGet_ReturnSessionInfo()
    {
        using var workspace = TempWorkspace.Create();
        var previous = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workspace.Root);
            var sessionDir = AlvorSensePaths.SessionDir("game-1");
            Directory.CreateDirectory(Path.Combine(sessionDir, "requests"));
            Directory.CreateDirectory(Path.Combine(sessionDir, "responses"));
            AlvorSenseJson.Save(AlvorSensePaths.Ready(sessionDir), new { processId = Environment.ProcessId });
            File.WriteAllText(AlvorSensePaths.Request(sessionDir, "req"), "{}", Encoding.UTF8);
            File.WriteAllText(AlvorSensePaths.Response(sessionDir, "res"), "{}", Encoding.UTF8);
            var legacySessionDir = AlvorSensePaths.SessionDir("legacy");
            Directory.CreateDirectory(legacySessionDir);
            AlvorSenseJson.Save(AlvorSensePaths.Ready(legacySessionDir), new { ready = true });
            var staleSessionDir = AlvorSensePaths.SessionDir("stale");
            Directory.CreateDirectory(staleSessionDir);
            AlvorSenseJson.Save(AlvorSensePaths.Ready(staleSessionDir), new { processId = int.MaxValue });

            var listed = AlvorSenseSessionRegistry.List();
            var status = AlvorSenseSessionRegistry.Get("game-1");

            Assert.AreEqual(3, listed.Length);
            Assert.AreEqual("game-1", status.Id);
            Assert.IsTrue(status.Ready);
            Assert.AreEqual(Environment.ProcessId, status.ProcessId);
            Assert.AreEqual(1, status.RequestCount);
            Assert.AreEqual(1, status.ResponseCount);
            Assert.IsTrue(listed.Single(static x => x.Id == "legacy").Ready);
            Assert.IsFalse(listed.Single(static x => x.Id == "stale").Ready);
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }

    /// <summary>Session registry returns an empty list when the session root does not exist.</summary>
    [TestMethod]
    public void SessionRegistry_ListWithoutRoot_ReturnsEmpty()
    {
        using var workspace = TempWorkspace.Create();
        var previous = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workspace.Root);

            Assert.AreEqual(0, AlvorSenseSessionRegistry.List().Length);
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }

    /// <summary>Detached host start information uses a session-local runtime without inheriting foreground output pipes.</summary>
    [TestMethod]
    public void HostProcess_CreateStartInfo_UsesSessionRuntimeAndDetachedShell()
    {
        using var workspace = TempWorkspace.Create();
        var sessionDir = workspace.PathFor("session");
        var assembly = workspace.Write("source/AlvorSense.dll", "fake");
        var dependency = workspace.Write("source/tools/dependency.txt", "copy me");

        var start = AlvorSenseHostProcess.CreateStartInfo(sessionDir, assembly);

        Assert.IsTrue(start.UseShellExecute);
        Assert.AreEqual(System.Diagnostics.ProcessWindowStyle.Hidden, start.WindowStyle);
        Assert.IsFalse(start.RedirectStandardOutput);
        Assert.IsFalse(start.RedirectStandardError);
        Assert.IsTrue(File.Exists(Path.Combine(AlvorSensePaths.HostRuntime(sessionDir), "AlvorSense.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(AlvorSensePaths.HostRuntime(sessionDir), "tools", Path.GetFileName(dependency))));
        CollectionAssert.AreEqual(
            new[] { Path.Combine(AlvorSensePaths.HostRuntime(sessionDir), "AlvorSense.dll"), "host", "--session-dir", Path.GetFullPath(sessionDir) },
            start.ArgumentList.ToArray());
    }

    private static AlvorSenseResponse SendResponse(bool ok, bool exited) =>
        new(
            "abc",
            ok,
            CommandCount: 1,
            StateLine: null,
            OutputLines: [],
            ProcessExited: exited,
            ExitCode: exited ? 1 : null,
            Error: ok ? null : "failed");
}
