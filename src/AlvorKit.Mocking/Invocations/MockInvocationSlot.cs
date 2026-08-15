namespace AlvorKit;

/// <summary>
/// Owns the mutable completion, projection, and verified state of one ledger
/// record.
/// </summary>
internal sealed class MockInvocationSlot
{
    private readonly Lock gate = new();
    private readonly MockInvocationArgumentSnapshot[] entryArguments;
    private readonly MockInvocationArgumentSnapshot[] selectionArguments;
    private readonly MockInvocationArgumentSnapshot[] exitArguments;
    private MockInvocationCompletion completion = MockInvocationCompletion.Pending;
    private MockInvocationAsyncCompletion? asyncCompletion;
    private int verified;

    /// <summary>Creates one pending invocation slot.</summary>
    internal MockInvocationSlot(
        MockInvocationIdentity identity,
        MockInvocationCoordinate coordinate,
        MockHistoryEpoch epoch,
        MockInvocationArgumentSnapshot[] entryArguments)
    {
        ArgumentNullException.ThrowIfNull(entryArguments);
        Identity = identity;
        Coordinate = coordinate;
        Epoch = epoch;
        this.entryArguments = entryArguments;
        selectionArguments = [.. entryArguments];
        exitArguments = new MockInvocationArgumentSnapshot[entryArguments.Length];

        for (var i = 0; i < entryArguments.Length; i++)
        {
            var entry = entryArguments[i];
            exitArguments[i] = MockInvocationArgumentSnapshot.UnavailableValue(
                new(
                    entry.DeclaredIndex,
                    entry.DeclaredType,
                    MockSnapshotPhase.Exit,
                    MockUnavailableReason.ExitProjectionNotConfigured));
        }
    }

    /// <summary>Gets the target, operation, and backend identity.</summary>
    internal MockInvocationIdentity Identity { get; }

    /// <summary>Gets the logical timeline coordinate.</summary>
    internal MockInvocationCoordinate Coordinate { get; }

    /// <summary>Gets the history epoch entered by this invocation.</summary>
    internal MockHistoryEpoch Epoch { get; }

    /// <summary>Publishes a heap-safe entry or exit projection.</summary>
    internal void PublishProjection(MockInvocationArgumentSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateSnapshot(snapshot);

        lock (gate)
        {
            EnsurePending();

            if (snapshot.Phase == MockSnapshotPhase.Entry)
                entryArguments[snapshot.DeclaredIndex] = snapshot;
            else
                exitArguments[snapshot.DeclaredIndex] = snapshot;
        }
    }

    /// <summary>Completes the invocation with a normal return exactly once.</summary>
    internal void CompleteReturned(
        MockInvocationExecutionSource source,
        MockInvocationReturn returned)
    {
        ValidateReturn(returned);
        var next = MockInvocationCompletion.Returned(source, returned);

        lock (gate)
        {
            EnsurePending();
            completion = next;
        }
    }

    /// <summary>Completes the invocation with the exact thrown exception once.</summary>
    internal void CompleteThrown(
        MockInvocationExecutionSource source,
        Exception exception,
        MockInvocationFailureStage failureStage)
    {
        var next = MockInvocationCompletion.Threw(source, exception, failureStage);

        lock (gate)
        {
            EnsurePending();

            for (var i = 0; i < exitArguments.Length; i++)
            {
                var entry = entryArguments[i];
                exitArguments[i] = MockInvocationArgumentSnapshot.UnavailableValue(
                    new(
                        entry.DeclaredIndex,
                        entry.DeclaredType,
                        MockSnapshotPhase.Exit,
                        MockUnavailableReason.NoNormalCompletion));
            }

            completion = next;
        }
    }

    /// <summary>Adds one optional asynchronous event after synchronous return.</summary>
    internal void CompleteAsync(MockInvocationAsyncCompletion value)
    {
        ArgumentNullException.ThrowIfNull(value);

        lock (gate)
        {
            if (completion.Kind != MockInvocationCompletionKind.Returned)
                throw new InvalidOperationException("Asynchronous completion requires a synchronous return.");
            if (asyncCompletion is not null)
                throw new InvalidOperationException("The invocation already has an asynchronous completion.");

            asyncCompletion = value;
        }
    }

    /// <summary>Marks this slot as verified while the ledger lock is held.</summary>
    internal void MarkVerified() => Volatile.Write(ref verified, 1);

    /// <summary>Creates one internally consistent immutable snapshot.</summary>
    internal MockInvocation Snapshot()
    {
        lock (gate)
        {
            var arguments = new MockInvocationArgument[entryArguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = new(
                    i,
                    entryArguments[i].DeclaredType,
                    entryArguments[i],
                    exitArguments[i]);
            }

            return new(
                Identity,
                Coordinate,
                Epoch,
                arguments,
                selectionArguments,
                completion,
                asyncCompletion,
                Volatile.Read(ref verified) != 0);
        }
    }

    private void ValidateSnapshot(MockInvocationArgumentSnapshot snapshot)
    {
        if ((uint)snapshot.DeclaredIndex >= (uint)entryArguments.Length)
            throw new ArgumentOutOfRangeException(nameof(snapshot));

        var entry = entryArguments[snapshot.DeclaredIndex];
        if (snapshot.DeclaredType != entry.DeclaredType)
            throw new ArgumentException("The snapshot declared type does not match the invocation.", nameof(snapshot));
        if (snapshot.Phase == MockSnapshotPhase.Entry &&
            entry.Unavailable?.Reason == MockUnavailableReason.OutHasNoEntryValue)
        {
            throw new ArgumentException(
                "An output parameter has no entry value to project.",
                nameof(snapshot));
        }
    }

    private void ValidateReturn(MockInvocationReturn returned)
    {
        ArgumentNullException.ThrowIfNull(returned);
        if (Identity.Operation is MethodInfo method &&
            method.ReturnType != returned.DeclaredType)
        {
            throw new ArgumentException(
                "The retained return type does not match the intercepted method.",
                nameof(returned));
        }
    }

    private void EnsurePending()
    {
        if (completion.Kind != MockInvocationCompletionKind.Pending)
            throw new InvalidOperationException("The invocation has already completed.");
    }
}
