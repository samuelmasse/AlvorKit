namespace AlvorKit.LiveCode;

/// <summary>
/// Owns an explicitly enabled LiveCode endpoint, its game-thread pump, and an optional frozen-only execution lane.
/// </summary>
public sealed class LiveCodeHost : IDisposable
{
    /// <summary>Gets the private loopback protocol version.</summary>
    public const int ProtocolVersion = 3;

    private readonly LiveCodeHostServer server;
    private readonly LiveCodeAssemblyRunner runner;
    private readonly LiveCodeBridgeRegistry bridges;
    private readonly LiveCodeFrozenInspectionLane? frozenInspection;
    private int pumpThreadId;

    /// <summary>Creates an explicitly enabled host around a tracked graph and optional predefined bridge registry.</summary>
    public LiveCodeHost(
        InjectorScopeGraph graph,
        LiveCodeHostOptions options,
        LiveCodeBridgeRegistry? bridges = null)
    {
        if (options.FrozenInspection is { FreezeThreshold: var threshold }
            && threshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Frozen-inspection threshold must be positive.");
        }

        this.bridges = bridges ?? new();
        runner = new(graph);
        frozenInspection = options.FrozenInspection is null
            ? null
            : new(runner, options.FrozenInspection);
        server = new(graph, options, this.bridges, frozenInspection);
    }

    /// <summary>Gets the current session after <see cref="Start"/>.</summary>
    public LiveCodeSessionManifest Session =>
        server.Session
        ?? throw new InvalidOperationException("The LiveCode host has not started.");

    /// <summary>Starts the optional inspection thread, loopback listener, and discovery manifest.</summary>
    public LiveCodeSessionManifest Start()
    {
        frozenInspection?.Start();
        try
        {
            return server.Start();
        }
        catch
        {
            frozenInspection?.Dispose();
            throw;
        }
    }

    /// <summary>Executes up to <paramref name="maximumCommands"/> queued commands on the calling thread.</summary>
    public int Pump(int maximumCommands = 8)
    {
        if (maximumCommands <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumCommands));

        var currentThreadId = Environment.CurrentManagedThreadId;
        if (pumpThreadId == 0)
            pumpThreadId = currentThreadId;
        else if (pumpThreadId != currentThreadId)
            throw new InvalidOperationException("LiveCodeHost.Pump must always run on the same game thread.");

        frozenInspection?.Beat();
        var count = 0;
        while (count < maximumCommands && server.TryDequeue(out var pending))
        {
            switch (pending)
            {
                case LiveCodePendingExecution execution:
                    execution.Completion.TrySetResult(runner.Run(execution));
                    break;
                case LiveCodePendingBridge bridge:
                    bridge.Completion.TrySetResult(bridges.Run(bridge));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown LiveCode work type '{pending.GetType()}'.");
            }

            count++;
        }

        return count;
    }

    /// <summary>Stops listening, removes discovery, and rejects queued work on both execution lanes.</summary>
    public void Dispose()
    {
        server.Dispose();
        frozenInspection?.Dispose();
    }
}
