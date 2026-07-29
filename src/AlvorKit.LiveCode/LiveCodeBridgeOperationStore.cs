namespace AlvorKit.LiveCode;

/// <summary>Bounds, reserves, and exposes two-phase bridge operations without entering the game thread.</summary>
internal sealed class LiveCodeBridgeOperationStore(int maximumOperations)
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, LiveCodeBridgeOperation> operations =
        new(StringComparer.Ordinal);

    internal LiveCodeBridgeOperation Reserve(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 128)
            throw new InvalidOperationException("Bridge operation id must contain 1 to 128 characters.");

        lock (gate)
        {
            RemoveCompleted();
            if (operations.ContainsKey(id))
                throw new InvalidOperationException($"Bridge operation id '{id}' is already reserved.");
            if (operations.Count >= maximumOperations)
            {
                throw new InvalidOperationException(
                    $"LiveCode already retains the maximum {maximumOperations} bridge operations.");
            }

            var operation = new LiveCodeBridgeOperation(id);
            operations.Add(id, operation);
            return operation;
        }
    }

    internal LiveCodeBridgeOperationStatusResponse Read(string id)
    {
        lock (gate)
        {
            if (!operations.TryGetValue(id, out var operation))
                throw new InvalidOperationException($"Bridge operation '{id}' was not found.");
            return operation.Snapshot();
        }
    }

    internal void CancelPending(string error)
    {
        LiveCodeBridgeOperation[] snapshot;
        lock (gate)
            snapshot = [.. operations.Values];

        foreach (var operation in snapshot)
        {
            operation.FailBeforeRun(new(
                LiveCodeBridgeExecutionStatus.Failed,
                "",
                0,
                [],
                [],
                [],
                0,
                error,
                null,
                null));
        }
    }

    private void RemoveCompleted()
    {
        if (operations.Count < maximumOperations)
            return;

        var completed = operations.Values
            .Select(static operation => operation.Snapshot())
            .Where(static operation => operation.State == LiveCodeBridgeOperationState.Completed)
            .OrderBy(static operation => operation.CompletedUtc)
            .Take(Math.Max(1, maximumOperations / 4))
            .Select(static operation => operation.OperationId)
            .ToArray();
        foreach (var id in completed)
            operations.Remove(id);
    }
}
