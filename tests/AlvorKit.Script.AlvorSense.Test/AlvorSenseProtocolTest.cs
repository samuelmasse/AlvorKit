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
        Assert.AreEqual(Path.Combine(sessionDir, "requests", "abc.tmp"), AlvorSensePaths.RequestTemp(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "requests", "abc.json"), AlvorSensePaths.Request(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "responses", "abc.json"), AlvorSensePaths.Response(sessionDir, "abc"));
        Assert.AreEqual(Path.Combine(sessionDir, "stdout.log"), AlvorSensePaths.Stdout(sessionDir));
        Assert.AreEqual(Path.Combine(sessionDir, "stderr.log"), AlvorSensePaths.Stderr(sessionDir));
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
            AlvorSenseJson.Save(AlvorSensePaths.Ready(sessionDir), new { processId = 123 });
            File.WriteAllText(AlvorSensePaths.Request(sessionDir, "req"), "{}", Encoding.UTF8);
            File.WriteAllText(AlvorSensePaths.Response(sessionDir, "res"), "{}", Encoding.UTF8);

            var listed = AlvorSenseSessionRegistry.List();
            var status = AlvorSenseSessionRegistry.Get("game-1");

            Assert.AreEqual(1, listed.Length);
            Assert.AreEqual("game-1", status.Id);
            Assert.IsTrue(status.Ready);
            Assert.AreEqual(123, status.ProcessId);
            Assert.AreEqual(1, status.RequestCount);
            Assert.AreEqual(1, status.ResponseCount);
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }
}
