namespace AlvorKit;

/// <summary>Runs ordinary scoped LiveCode commands on a dedicated thread only after frame heartbeats stop.</summary>
internal sealed class LiveCodeFrozenInspectionLane(
    LiveCodeAssemblyRunner runner,
    LiveCodeFrozenInspectionOptions options) : IDisposable
{
    private readonly ConcurrentQueue<LiveCodePendingFrozenExecution> pending = new();
    private readonly AutoResetEvent signal = new(false);
    private readonly Lock lifecycleGate = new();
    private readonly double thresholdMilliseconds = options.FreezeThreshold.TotalMilliseconds;
    private Thread? thread;
    private long lastFrameTimestamp;
    private long frameNumber;
    private int inspectionRunning;
    private int inspectorThreadId;
    private int started;
    private int disposed;

    internal void Start()
    {
        if (Interlocked.Exchange(ref started, 1) != 0)
            throw new InvalidOperationException("The frozen-inspection lane has already started.");

        Volatile.Write(ref lastFrameTimestamp, Stopwatch.GetTimestamp());
        thread = new(Work)
        {
            IsBackground = true,
            Name = "AlvorKit Frozen Inspection"
        };
        thread.Start();
    }

    /// <summary>Records one game-thread safe-point without allocating.</summary>
    internal void Beat()
    {
        Volatile.Write(ref lastFrameTimestamp, Stopwatch.GetTimestamp());
        _ = Interlocked.Increment(ref frameNumber);
    }

    internal LiveCodeFrozenInspectionSnapshot Snapshot()
    {
        var last = Volatile.Read(ref lastFrameTimestamp);
        var age = last == 0
            ? 0
            : Stopwatch.GetElapsedTime(last).TotalMilliseconds;
        var frames = Volatile.Read(ref frameNumber);
        return new(
            true,
            frames > 0 && age >= thresholdMilliseconds,
            frames,
            age,
            thresholdMilliseconds,
            thread?.IsAlive == true,
            Volatile.Read(ref inspectionRunning) != 0,
            Volatile.Read(ref inspectorThreadId));
    }

    internal void Enqueue(LiveCodePendingFrozenExecution execution)
    {
        lock (lifecycleGate)
        {
            if (Volatile.Read(ref disposed) != 0)
            {
                execution.Cancel("The LiveCode host stopped before frozen execution.", Snapshot());
                return;
            }

            pending.Enqueue(execution);
            signal.Set();
        }
    }

    public void Dispose()
    {
        lock (lifecycleGate)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
                return;

            signal.Set();
        }

        var stopped = thread is null
            || thread.ManagedThreadId != Environment.CurrentManagedThreadId
            && thread.Join(TimeSpan.FromSeconds(1));
        if (stopped)
            signal.Dispose();

        var snapshot = Snapshot();
        while (pending.TryDequeue(out var execution))
            execution.Cancel("The LiveCode host stopped before frozen execution.", snapshot);
    }

    internal static LiveCodeFrozenInspectionSnapshot DisabledSnapshot() =>
        new(false, false, 0, 0, 0, false, false, 0);

    private void Work()
    {
        Volatile.Write(ref inspectorThreadId, Environment.CurrentManagedThreadId);
        while (Volatile.Read(ref disposed) == 0)
        {
            signal.WaitOne();
            if (Volatile.Read(ref disposed) != 0)
                break;
            while (pending.TryDequeue(out var execution))
                Execute(execution);
        }
    }

    private void Execute(LiveCodePendingFrozenExecution pendingExecution)
    {
        var startedSnapshot = Snapshot();
        if (!startedSnapshot.IsFrozen)
        {
            pendingExecution.Completion.TrySetResult(new(
                startedSnapshot,
                Snapshot(),
                new(
                    LiveCodeExecutionStatus.GameRunning,
                    pendingExecution.ScopeId,
                    [],
                    [],
                    0,
                    $"Frozen execution requires a frame heartbeat at least {thresholdMilliseconds:F0} ms old.",
                    null,
                    null)));
            return;
        }

        Volatile.Write(ref inspectionRunning, 1);
        try
        {
            startedSnapshot = Snapshot();
            var result = startedSnapshot.IsFrozen
                ? runner.Run(
                    pendingExecution.ScopeId,
                    pendingExecution.EntryType,
                    pendingExecution.Assembly,
                    pendingExecution.Symbols)
                : new(
                    LiveCodeExecutionStatus.GameRunning,
                    pendingExecution.ScopeId,
                    [],
                    [],
                    0,
                    "The game-frame heartbeat resumed before frozen execution began.",
                    null,
                    null);
            Volatile.Write(ref inspectionRunning, 0);
            pendingExecution.Completion.TrySetResult(new(
                startedSnapshot,
                Snapshot(),
                result));
        }
        finally
        {
            Volatile.Write(ref inspectionRunning, 0);
        }
    }
}
