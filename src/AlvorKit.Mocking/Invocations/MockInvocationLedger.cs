namespace AlvorKit.Mocking;

/// <summary>
/// Appends one record at intercepted entry and coordinates completion, epoch
/// clearing, snapshots, and atomic verified marking.
/// </summary>
internal sealed class MockInvocationLedger
{
    private static long nextLedgerId;
    private readonly Lock gate = new();
    private readonly MockInvocationTimeline timeline;
    private MockHistorySegment current;

    /// <summary>Creates a ledger with an independent mock-local timeline.</summary>
    internal MockInvocationLedger()
        : this(new())
    {
    }

    /// <summary>Creates a ledger on a supplied future session-compatible timeline.</summary>
    internal MockInvocationLedger(MockInvocationTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        Id = Interlocked.Increment(ref nextLedgerId);
        this.timeline = timeline;
        current = new(new(Id, 0));
    }

    /// <summary>Gets the runtime-unique ledger ID.</summary>
    internal long Id { get; }

    /// <summary>Gets the mock-local or shared logical timeline.</summary>
    internal MockInvocationTimeline Timeline => timeline;

    /// <summary>
    /// Opens and publishes exactly one pending invocation before behavior
    /// dispatch.
    /// </summary>
    internal MockInvocationToken Open(
        MockInvocationIdentity identity,
        ReadOnlySpan<MockInvocationArgumentSnapshot> entryArguments) =>
        Open(identity, entryArguments, timeline);

    /// <summary>Opens an invocation on an explicit session-compatible timeline.</summary>
    internal MockInvocationToken Open(
        MockInvocationIdentity identity,
        ReadOnlySpan<MockInvocationArgumentSnapshot> entryArguments,
        MockInvocationTimeline invocationTimeline) =>
        OpenCore(
            identity,
            entryArguments.ToArray(),
            null,
            invocationTimeline);

    /// <summary>
    /// Opens an invocation by transferring ownership of an already validated-shape
    /// entry array and reusing its reflected parameter metadata.
    /// </summary>
    internal MockInvocationToken OpenOwned(
        MockInvocationIdentity identity,
        MockInvocationArgumentSnapshot[] entryArguments,
        ParameterInfo[] parameters,
        MockInvocationTimeline invocationTimeline) =>
        OpenCore(
            identity,
            entryArguments,
            parameters,
            invocationTimeline);

    private MockInvocationToken OpenCore(
        MockInvocationIdentity identity,
        MockInvocationArgumentSnapshot[] entries,
        ParameterInfo[]? parameters,
        MockInvocationTimeline invocationTimeline)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(invocationTimeline);

        MockInvocationLedgerContract.ValidateEntryArguments(
            identity,
            entries,
            parameters);

        var reservation = invocationTimeline.Reserve();
        MockHistorySegment segment;
        MockInvocationSlot slot;

        try
        {
            lock (gate)
            {
                segment = current;
                slot = new(
                    identity,
                    new(reservation.TimelineId, reservation.Sequence),
                    segment.Epoch,
                    entries);
                segment.Add(slot);
            }
        }
        catch
        {
            invocationTimeline.Cancel(reservation);
            throw;
        }

        invocationTimeline.Publish(reservation);
        return new(Id, segment, slot);
    }

    /// <summary>Publishes one heap-safe entry or exit projection.</summary>
    internal void PublishProjection(
        MockInvocationToken token,
        MockInvocationArgumentSnapshot snapshot)
    {
        MockInvocationLedgerContract.ValidateToken(Id, token);
        token.Slot.PublishProjection(snapshot);
    }

    /// <summary>Completes one invocation with a normal return.</summary>
    internal void CompleteReturned(
        MockInvocationToken token,
        MockInvocationExecutionSource source,
        MockInvocationReturn returned)
    {
        MockInvocationLedgerContract.ValidateToken(Id, token);
        token.Slot.CompleteReturned(source, returned);
    }

    /// <summary>Completes one invocation with the exact thrown exception.</summary>
    internal void CompleteThrown(
        MockInvocationToken token,
        MockInvocationExecutionSource source,
        Exception exception,
        MockInvocationFailureStage failureStage)
    {
        MockInvocationLedgerContract.ValidateToken(Id, token);
        token.Slot.CompleteThrown(source, exception, failureStage);
    }

    /// <summary>Adds one optional asynchronous event to an existing invocation.</summary>
    internal void CompleteAsync(
        MockInvocationToken token,
        MockInvocationAsyncCompletion completion)
    {
        MockInvocationLedgerContract.ValidateToken(Id, token);
        token.Slot.CompleteAsync(completion);
    }

    /// <summary>Captures current-epoch membership and invocation state.</summary>
    internal MockInvocationLedgerSnapshot Snapshot()
    {
        lock (gate)
            return Snapshot(current);
    }

    /// <summary>Refreshes outcome and verified state without changing membership.</summary>
    internal MockInvocationLedgerSnapshot Refresh(MockInvocationLedgerSnapshot snapshot)
    {
        MockInvocationLedgerContract.ValidateSnapshot(Id, snapshot);

        lock (gate)
            return new(Id, snapshot.Epoch, snapshot.CopySlots());
    }

    /// <summary>
    /// Starts a new empty epoch and returns a pinned snapshot of the retired
    /// epoch so in-flight calls can complete there safely.
    /// </summary>
    internal MockInvocationLedgerSnapshot ClearEpoch()
    {
        MockHistorySegment retired;

        lock (gate)
        {
            retired = current;
            current = new(new(Id, retired.Epoch.Number + 1));
            return Snapshot(retired);
        }
    }

    /// <summary>
    /// Marks selected snapshot positions only after all positions validate,
    /// making successful verification all-or-nothing to ledger readers.
    /// </summary>
    internal void MarkVerifiedAtomically(
        MockInvocationLedgerSnapshot snapshot,
        ReadOnlySpan<int> indices)
    {
        MockInvocationLedgerContract.ValidateSnapshot(Id, snapshot);

        lock (gate)
        {
            for (var i = 0; i < indices.Length; i++)
            {
                if ((uint)indices[i] >= (uint)snapshot.Invocations.Length)
                    throw new ArgumentOutOfRangeException(nameof(indices));

                for (var j = 0; j < i; j++)
                {
                    if (indices[j] == indices[i])
                        throw new ArgumentException("Verification indices must be unique.", nameof(indices));
                }
            }

            for (var i = 0; i < indices.Length; i++)
                snapshot.SlotAt(indices[i]).MarkVerified();
        }
    }

    private MockInvocationLedgerSnapshot Snapshot(MockHistorySegment segment) =>
        new(Id, segment.Epoch, segment.CopySlots());

}
