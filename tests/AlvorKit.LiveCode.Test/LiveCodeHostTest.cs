namespace AlvorKit.LiveCode.Test;

/// <summary>Verifies discovery, graph transport, and exact game-thread command execution over loopback.</summary>
[TestClass]
public class LiveCodeHostTest
{
    /// <summary>A client can inspect the graph and execute a compiled command only when the host pump runs.</summary>
    [TestMethod]
    public void ClientInspectsGraphAndExecutesThroughPump()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector, "Test Root");
        using var host = new LiveCodeHost(
            graph,
            new("test")
            {
                DiscoveryDirectory = workspace.Root,
                CompilationAssembly = typeof(LiveCodeHostTest).Assembly,
            });
        var session = host.Start();
        var client = new LiveCodeClient(session);

        Assert.IsTrue(session.FrozenInspectionEnabled);
        var transportedGraph = client.Graph().GetAwaiter().GetResult();
        var references = client.References().GetAwaiter().GetResult();
        var execution = client.Execute(
            transportedGraph.RootId,
            typeof(LoopbackCommand).FullName!,
            File.ReadAllBytes(typeof(LoopbackCommand).Assembly.Location));

        PumpUntilComplete(host, execution);
        var result = execution.GetAwaiter().GetResult();
        Assert.AreEqual(LiveCodeExecutionStatus.Completed, result.Status);
        CollectionAssert.Contains(result.Lines, "Executed on the host pump.");
        Assert.IsTrue(references.AssemblyPaths.Contains(typeof(ILiveCodeCommand).Assembly.Location));
        CollectionAssert.Contains(references.GlobalUsings, "System.Xml.Linq");
        Assert.AreEqual("Test Root", transportedGraph.Nodes[0].Label);
    }

    /// <summary>Without compilation metadata, only explicitly configured imports enter the manifest.</summary>
    [TestMethod]
    public void ReferencesWithoutCompilationMetadataUseOnlyExplicitImports()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        using var host = new LiveCodeHost(
            graph,
            new("test")
            {
                DiscoveryDirectory = workspace.Root,
                CompilationAssembly = null,
                FrozenInspection = null,
                GlobalUsings = ["Fixture.LiveCode"],
            });
        var session = host.Start();
        var client = new LiveCodeClient(session);

        var references = client.References().GetAwaiter().GetResult();

        Assert.IsFalse(session.FrozenInspectionEnabled);
        CollectionAssert.AreEqual(
            new[] { "Fixture.LiveCode" },
            references.GlobalUsings);
    }

    /// <summary>The dependency catalog ignores dynamic assemblies that have no runtime image.</summary>
    [TestMethod]
    public void DependencyCatalogIgnoresDynamicAssembly()
    {
        var assembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
            new("LiveCodeDynamicFixture"),
            System.Reflection.Emit.AssemblyBuilderAccess.Run);
        var paths = new HashSet<string>();

        LiveCodeDependencyCatalog.AddTo(paths, assembly);

        Assert.AreEqual(0, paths.Count);
    }

    /// <summary>A queued command receives a scope-ended result when its exact target is no longer active.</summary>
    [TestMethod]
    public void ExecutionRejectsEndedScope()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var child = graph.Scope<HostScope>(injector, "Transient");
        var childId = graph.Snapshot().Nodes[1].Id.Value;
        graph.End(child);
        using var host = new LiveCodeHost(
            graph,
            new("test") { DiscoveryDirectory = workspace.Root });
        var client = new LiveCodeClient(host.Start());

        var execution = client.Execute(
            childId,
            typeof(LoopbackCommand).FullName!,
            File.ReadAllBytes(typeof(LoopbackCommand).Assembly.Location));

        PumpUntilComplete(host, execution);
        Assert.AreEqual(
            LiveCodeExecutionStatus.ScopeEnded,
            execution.GetAwaiter().GetResult().Status);
    }

    /// <summary>A discoverable versioned bridge executes structured JSON on the same explicit host pump.</summary>
    [TestMethod]
    public void ClientDiscoversAndExecutesPredefinedBridge()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var bridges = new LiveCodeBridgeRegistry();
        bridges.Register(new EchoBridge());
        using var host = new LiveCodeHost(
            graph,
            new("test") { DiscoveryDirectory = workspace.Root },
            bridges);
        var client = new LiveCodeClient(host.Start());

        var descriptors = client.Bridges().GetAwaiter().GetResult();
        var execution = client.Bridge(
            "test.echo",
            JsonSerializer.SerializeToElement(new { message = "hello bridge" }),
            version: 1);

        PumpUntilComplete(host, execution);
        var result = execution.GetAwaiter().GetResult();
        Assert.AreEqual(1, descriptors.Length);
        Assert.AreEqual(LiveCodeBridgeLease.None, descriptors[0].Lease);
        Assert.AreEqual(LiveCodeBridgeExecutionStatus.Completed, result.Status);
        Assert.AreEqual("hello bridge", result.Values["echo"].GetString());
        CollectionAssert.Contains(result.Lines, "Bridge reached the host pump.");
    }

    /// <summary>The out-of-band lane rejects ordinary scoped code while the frame heartbeat is fresh.</summary>
    [TestMethod]
    public void FrozenExecutionRejectsWhileGameFramesAdvance()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        using var host = new LiveCodeHost(
            graph,
            new("test")
            {
                DiscoveryDirectory = workspace.Root,
                FrozenInspection = new() { FreezeThreshold = TimeSpan.FromSeconds(1) }
            });
        var client = new LiveCodeClient(host.Start());
        host.Pump();

        var result = client.ExecuteFrozen(
            graph.RootId.Value,
            typeof(LoopbackCommand).FullName!,
            File.ReadAllBytes(typeof(LoopbackCommand).Assembly.Location)).GetAwaiter().GetResult();

        Assert.AreEqual(LiveCodeExecutionStatus.GameRunning, result.Execution.Status);
        Assert.IsFalse(result.Started.IsFrozen);
        Assert.IsTrue(result.Started.InspectorThreadAlive);
    }

    /// <summary>A stalled heartbeat permits normal constructor injection and execution without another game-thread pump.</summary>
    [TestMethod]
    public void FrozenExecutionUsesExactScopeOnDedicatedThread()
    {
        using var workspace = TempWorkspace.Create();
        var injector = new Injector();
        var injectedRegistry = new LiveCodeBridgeRegistry();
        injectedRegistry.Register(new EchoBridge());
        injector.Add(injectedRegistry);
        var graph = new InjectorScopeGraph(injector);
        var scopeId = graph.RootId.Value;
        using var host = new LiveCodeHost(
            graph,
            new("test")
            {
                DiscoveryDirectory = workspace.Root,
                FrozenInspection = new() { FreezeThreshold = TimeSpan.FromMilliseconds(40) }
            });
        var client = new LiveCodeClient(host.Start());
        var gameThreadId = Environment.CurrentManagedThreadId;
        host.Pump();
        var frozen = WaitUntilFrozen(client);

        var result = client.ExecuteFrozen(
            scopeId,
            typeof(FrozenLoopbackCommand).FullName!,
            File.ReadAllBytes(typeof(FrozenLoopbackCommand).Assembly.Location)).GetAwaiter().GetResult();

        Assert.IsTrue(frozen.IsFrozen);
        Assert.AreEqual(
            LiveCodeExecutionStatus.Completed,
            result.Execution.Status,
            $"{result.Execution.ExceptionType}: {result.Execution.Error}");
        Assert.AreEqual("1", result.Execution.Values["bridgeCount"]);
        Assert.AreNotEqual(
            gameThreadId.ToString(),
            result.Execution.Values["thread"]);
        Assert.AreEqual(scopeId, result.Execution.ScopeId);
        Assert.IsTrue(result.Started.InspectionRunning);
        Assert.IsFalse(result.Completed.InspectionRunning);
    }

    private static void PumpUntilComplete(
        LiveCodeHost host,
        Task<LiveCodeExecutionResult> execution)
    {
        var timeout = Stopwatch.StartNew();
        while (!execution.IsCompleted && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            host.Pump();
            Thread.Sleep(5);
        }

        Assert.IsTrue(execution.IsCompleted, "LiveCode execution did not reach the host pump.");
    }

    private static void PumpUntilComplete(
        LiveCodeHost host,
        Task<LiveCodeBridgeExecutionResult> execution)
    {
        var timeout = Stopwatch.StartNew();
        while (!execution.IsCompleted && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            host.Pump();
            Thread.Sleep(5);
        }

        Assert.IsTrue(execution.IsCompleted, "LiveCode bridge did not reach the host pump.");
    }

    private static LiveCodeFrozenInspectionSnapshot WaitUntilFrozen(LiveCodeClient client)
    {
        var timeout = Stopwatch.StartNew();
        LiveCodeFrozenInspectionSnapshot snapshot;
        do
        {
            Thread.Sleep(10);
            snapshot = client.FrozenInspectionStatus().GetAwaiter().GetResult();
        }
        while (!snapshot.IsFrozen && timeout.Elapsed < TimeSpan.FromSeconds(5));

        Assert.IsTrue(snapshot.IsFrozen, "The test frame heartbeat did not become stale.");
        return snapshot;
    }
}

/// <summary>Unscoped test command loaded into a collectible context and constructed by the root injector.</summary>
public sealed class LoopbackCommand : ILiveCodeCommand
{
    /// <summary>Reports that invocation reached the explicit host pump.</summary>
    public void Run(LiveCodeContext output) =>
        output.WriteLine("Executed on the host pump.");
}

/// <summary>Ordinary child-scoped command run by the dedicated frozen-inspection thread.</summary>
public sealed class FrozenLoopbackCommand(
    LiveCodeBridgeRegistry registry) : ILiveCodeCommand
{
    /// <summary>Reports injected state and the actual execution thread.</summary>
    public void Run(LiveCodeContext output)
    {
        output.Value("bridgeCount", registry.Describe().Length);
        output.Value("thread", Environment.CurrentManagedThreadId);
    }
}

/// <summary>Marks a disposable child target used by the ended-scope test.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HostAttribute : InjectorAttribute;

/// <summary>Tracked child scope used to verify stale exact-scope rejection.</summary>
[Host]
public class HostScope : InjectorScope<HostAttribute>;

/// <summary>Small structured bridge used to prove registration, discovery, versioning, and execution.</summary>
public sealed class EchoBridge : ILiveCodeBridge
{
    /// <inheritdoc />
    public LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "test.echo",
        1,
        "Echo a test payload.",
        false,
        LiveCodeBridgeLease.None,
        JsonSerializer.SerializeToElement(new { type = "object" }));

    /// <inheritdoc />
    public void Run(LiveCodeBridgeContext context, JsonElement request)
    {
        context.WriteLine("Bridge reached the host pump.");
        context.Value("echo", request.GetProperty("message").GetString());
    }
}
