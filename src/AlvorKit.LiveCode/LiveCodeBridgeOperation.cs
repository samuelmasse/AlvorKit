namespace AlvorKit.LiveCode;

/// <summary>Thread-safe target-side lifecycle for one accepted two-phase bridge operation.</summary>
internal sealed class LiveCodeBridgeOperation(string id)
{
    private readonly Lock gate = new();
    private LiveCodeBridgeOperationState state = LiveCodeBridgeOperationState.Pending;
    private DateTimeOffset? startedUtc;
    private DateTimeOffset? completedUtc;
    private LiveCodeBridgeExecutionResult? result;

    internal string Id { get; } = id;

    internal DateTimeOffset AcceptedUtc { get; } = DateTimeOffset.UtcNow;

    internal bool TryStart()
    {
        lock (gate)
        {
            if (state != LiveCodeBridgeOperationState.Pending)
                return false;

            state = LiveCodeBridgeOperationState.Running;
            startedUtc = DateTimeOffset.UtcNow;
            return true;
        }
    }

    internal void Complete(LiveCodeBridgeExecutionResult terminal)
    {
        lock (gate)
        {
            if (state == LiveCodeBridgeOperationState.Completed)
                return;
            if (state != LiveCodeBridgeOperationState.Running)
                throw new InvalidOperationException($"Bridge operation '{Id}' was not running.");

            result = terminal;
            completedUtc = DateTimeOffset.UtcNow;
            state = LiveCodeBridgeOperationState.Completed;
        }
    }

    internal void FailBeforeRun(LiveCodeBridgeExecutionResult terminal)
    {
        lock (gate)
        {
            if (state != LiveCodeBridgeOperationState.Pending)
                return;

            result = terminal;
            completedUtc = DateTimeOffset.UtcNow;
            state = LiveCodeBridgeOperationState.Completed;
        }
    }

    internal LiveCodeBridgeOperationStatusResponse Snapshot()
    {
        lock (gate)
        {
            return new(
                Id,
                state,
                AcceptedUtc,
                startedUtc,
                completedUtc,
                result);
        }
    }
}
