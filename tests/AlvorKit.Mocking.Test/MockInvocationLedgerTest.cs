namespace AlvorKit;

[TestClass]
public sealed class MockInvocationLedgerTest
{
    private static readonly MethodInfo Method =
        typeof(MockInvocationLedgerTest).GetMethod(
            nameof(Target),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly MethodInfo AsyncMethod =
        typeof(MockInvocationLedgerTest).GetMethod(
            nameof(TargetAsync),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Open appends one pending call with heap-safe arguments in declared order.</summary>
    [TestMethod]
    public void Open_AppendsPendingInvocationInDeclaredOrder()
    {
        var ledger = new MockInvocationLedger();

        var token = ledger.Open(Identity(), Entries(17));
        var snapshot = ledger.Snapshot();
        var invocations = snapshot.Invocations;

        Assert.AreEqual(1, invocations.Length);
        Assert.AreEqual(MockInvocationCompletionKind.Pending, invocations[0].Completion.Kind);
        Assert.AreEqual(token.Coordinate, invocations[0].Coordinate);
        Assert.AreEqual(token.Epoch, invocations[0].Epoch);

        var arguments = invocations[0].Arguments;
        Assert.AreEqual(4, arguments.Length);
        Assert.AreEqual(17, arguments[0].Entry.Value);
        Assert.AreEqual(typeof(string).MakeByRefType(), arguments[1].DeclaredType);
        Assert.AreEqual(
            MockUnavailableReason.ByRefLikeProjectionNotConfigured,
            arguments[2].Entry.Unavailable!.Reason);
        Assert.AreEqual(
            MockUnavailableReason.OutHasNoEntryValue,
            arguments[3].Entry.Unavailable!.Reason);
        Assert.Throws<ArgumentException>(
            () => ledger.PublishProjection(
                token,
                MockInvocationArgumentSnapshot.Projected(
                    3,
                    typeof(Span<int>).MakeByRefType(),
                    MockSnapshotPhase.Entry,
                    Array.Empty<int>())));
    }

    /// <summary>Projection and normal completion update the original record without appending another.</summary>
    [TestMethod]
    public void CompleteReturned_UpdatesOriginalInvocation()
    {
        var ledger = new MockInvocationLedger();
        var token = ledger.Open(Identity(), Entries(3));
        int[] entryProjection = [2, 4];
        int[] exitProjection = [8, 16];

        ledger.PublishProjection(
            token,
            MockInvocationArgumentSnapshot.Projected(
                2,
                typeof(ReadOnlySpan<int>),
                MockSnapshotPhase.Entry,
                entryProjection));
        ledger.PublishProjection(
            token,
            MockInvocationArgumentSnapshot.Projected(
                3,
                typeof(Span<int>).MakeByRefType(),
                MockSnapshotPhase.Exit,
                exitProjection));
        ledger.CompleteReturned(
            token,
            MockInvocationExecutionSource.Configured,
            MockInvocationReturn.Void());

        var invocation = ledger.Snapshot().Invocations[0];

        Assert.AreEqual(MockInvocationCompletionKind.Returned, invocation.Completion.Kind);
        Assert.AreEqual(MockInvocationExecutionSource.Configured, invocation.Completion.Source);
        Assert.AreSame(entryProjection, invocation.Arguments[2].Entry.Value);
        Assert.AreSame(exitProjection, invocation.Arguments[3].Exit.Value);
        Assert.AreEqual(1, ledger.Snapshot().Invocations.Length);
    }

    /// <summary>Throw completion preserves exception identity and marks exit values unavailable.</summary>
    [TestMethod]
    public void CompleteThrown_PreservesExactExceptionAndCompletesOnce()
    {
        var ledger = new MockInvocationLedger();
        var token = ledger.Open(Identity(), Entries(5));
        var expected = new InvalidOperationException("expected");

        ledger.CompleteThrown(
            token,
            MockInvocationExecutionSource.PartialPassthrough,
            expected,
            MockInvocationFailureStage.OriginalImplementation);

        var invocation = ledger.Snapshot().Invocations[0];
        Assert.AreEqual(MockInvocationCompletionKind.Threw, invocation.Completion.Kind);
        Assert.AreSame(expected, invocation.Completion.Exception);
        Assert.AreEqual(
            MockInvocationFailureStage.OriginalImplementation,
            invocation.Completion.FailureStage);

        foreach (var argument in invocation.Arguments)
        {
            Assert.AreEqual(
                MockUnavailableReason.NoNormalCompletion,
                argument.Exit.Unavailable!.Reason);
        }

        Assert.Throws<InvalidOperationException>(
            () => ledger.CompleteReturned(
                token,
                MockInvocationExecutionSource.Configured,
                MockInvocationReturn.Void()));
        Assert.AreEqual(1, ledger.Snapshot().Invocations.Length);
    }

    /// <summary>Optional asynchronous completion remains one event on the existing invocation.</summary>
    [TestMethod]
    public void CompleteAsync_AddsOneEventAfterSynchronousReturn()
    {
        var ledger = new MockInvocationLedger();
        var token = ledger.Open(Identity(AsyncMethod), Entries(7));
        var expected = new IOException("fault");

        ledger.CompleteReturned(
            token,
            MockInvocationExecutionSource.Configured,
            MockInvocationReturn.Shallow(typeof(Task), Task.CompletedTask));
        ledger.CompleteAsync(
            token,
            new(MockInvocationAsyncCompletionKind.Faulted, expected));

        var invocation = ledger.Snapshot().Invocations[0];
        Assert.AreEqual(MockInvocationAsyncCompletionKind.Faulted, invocation.AsyncCompletion!.Kind);
        Assert.AreSame(expected, invocation.AsyncCompletion.Exception);
        Assert.Throws<InvalidOperationException>(
            () => ledger.CompleteAsync(
                token,
                new(MockInvocationAsyncCompletionKind.Succeeded)));
    }

    /// <summary>Concurrent opens lose no calls and snapshots sort every unique logical sequence.</summary>
    [TestMethod]
    public void Open_ConcurrentCallsHaveExactOrderedSequences()
    {
        const int count = 48;
        var ledger = new MockInvocationLedger();
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = new Task[count];

        for (var i = 0; i < tasks.Length; i++)
        {
            var value = i;
            tasks[i] = Task.Run(async () =>
            {
                await start.Task;
                var token = ledger.Open(Identity(), Entries(value));
                ledger.CompleteReturned(
                    token,
                    MockInvocationExecutionSource.Configured,
                    MockInvocationReturn.Void());
            });
        }

        start.SetResult();
        Task.WaitAll(tasks);

        var invocations = ledger.Snapshot().Invocations;
        Assert.AreEqual(count, invocations.Length);
        Assert.AreEqual(count, ledger.Timeline.Checkpoint().Sequence);

        for (var i = 0; i < invocations.Length; i++)
        {
            Assert.AreEqual(i + 1, invocations[i].Coordinate.Sequence);
            Assert.AreEqual(MockInvocationCompletionKind.Returned, invocations[i].Completion.Kind);
        }
    }

    /// <summary>Clear starts a new epoch while a blocked call completes in its entry epoch.</summary>
    [TestMethod]
    public void ClearEpoch_BlockedInvocationCompletesInRetiredEpoch()
    {
        var ledger = new MockInvocationLedger();
        var token = ledger.Open(Identity(), Entries(1));
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completing = Task.Run(async () =>
        {
            await release.Task;
            ledger.CompleteReturned(
                token,
                MockInvocationExecutionSource.PartialPassthrough,
                MockInvocationReturn.Void());
        });

        var retired = ledger.ClearEpoch();
        var currentToken = ledger.Open(Identity(), Entries(2));
        ledger.CompleteReturned(
            currentToken,
            MockInvocationExecutionSource.Configured,
            MockInvocationReturn.Void());
        release.SetResult();
        completing.GetAwaiter().GetResult();

        var completedRetired = ledger.Refresh(retired);
        Assert.AreEqual(0, completedRetired.Epoch.Number);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            completedRetired.Invocations[0].Completion.Kind);

        var current = ledger.Snapshot();
        Assert.AreEqual(1, current.Epoch.Number);
        Assert.AreEqual(1, current.Invocations.Length);
        Assert.AreEqual(2, current.Invocations[0].Arguments[0].Entry.Value);
    }

    /// <summary>Verified marking validates the whole selection before changing any record.</summary>
    [TestMethod]
    public void MarkVerifiedAtomically_InvalidSelectionMarksNothing()
    {
        var ledger = CompletedLedger(3);
        var snapshot = ledger.Snapshot();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ledger.MarkVerifiedAtomically(snapshot, [0, 9]));
        Assert.IsTrue(ledger.Snapshot().Invocations.ToArray().All(
            static invocation => !invocation.IsVerified));

        ledger.MarkVerifiedAtomically(snapshot, [0, 2]);

        var marked = ledger.Snapshot().Invocations;
        Assert.IsTrue(marked[0].IsVerified);
        Assert.IsFalse(marked[1].IsVerified);
        Assert.IsTrue(marked[2].IsVerified);
    }

    /// <summary>Timeline checkpoints wait for every earlier reservation to publish or cancel.</summary>
    [TestMethod]
    public void Timeline_OutOfOrderPublicationAdvancesContiguousWatermark()
    {
        var timeline = new MockInvocationTimeline();
        var first = timeline.Reserve();
        var second = timeline.Reserve();

        timeline.Publish(second);
        Assert.AreEqual(0, timeline.Checkpoint().Sequence);

        timeline.Cancel(first);
        Assert.AreEqual(2, timeline.Checkpoint().Sequence);
        Assert.Throws<InvalidOperationException>(() => timeline.Publish(second));
    }

    /// <summary>Invalid declared argument order is rejected before consuming a sequence.</summary>
    [TestMethod]
    public void Open_InvalidDeclaredOrderDoesNotConsumeTimeline()
    {
        var ledger = new MockInvocationLedger();
        var entries = Entries(1);
        (entries[0], entries[1]) = (entries[1], entries[0]);

        Assert.Throws<ArgumentException>(() => ledger.Open(Identity(), entries));
        Assert.AreEqual(0, ledger.Timeline.Checkpoint().Sequence);
        Assert.AreEqual(0, ledger.Snapshot().Invocations.Length);
    }

    private static MockInvocationLedger CompletedLedger(int count)
    {
        var ledger = new MockInvocationLedger();
        for (var i = 0; i < count; i++)
        {
            var token = ledger.Open(Identity(), Entries(i));
            ledger.CompleteReturned(
                token,
                MockInvocationExecutionSource.Configured,
                MockInvocationReturn.Void());
        }

        return ledger;
    }

    private static MockInvocationIdentity Identity(MethodInfo? method = null) =>
        new(
            MockInvocationTarget.ForMock(1, typeof(MockInvocationLedgerTest)),
            method ?? Method,
            "ledger-test");

    private static MockInvocationArgumentSnapshot[] Entries(int value) =>
    [
        MockInvocationArgumentSnapshot.Shallow(
            0,
            typeof(int),
            MockSnapshotPhase.Entry,
            value),
        MockInvocationArgumentSnapshot.Shallow(
            1,
            typeof(string).MakeByRefType(),
            MockSnapshotPhase.Entry,
            "entry"),
        MockInvocationArgumentSnapshot.UnavailableValue(
            new(
                2,
                typeof(ReadOnlySpan<int>),
                MockSnapshotPhase.Entry,
                MockUnavailableReason.ByRefLikeProjectionNotConfigured)),
        MockInvocationArgumentSnapshot.UnavailableValue(
            new(
                3,
                typeof(Span<int>).MakeByRefType(),
                MockSnapshotPhase.Entry,
                MockUnavailableReason.OutHasNoEntryValue))
    ];

    private void Target(
        int value,
        ref string text,
        ReadOnlySpan<int> values,
        out Span<int> output)
    {
        output = default;
        _ = value + text.Length + values.Length;
    }

    private Task TargetAsync(
        int value,
        ref string text,
        ReadOnlySpan<int> values,
        out Span<int> output)
    {
        output = default;
        _ = value + text.Length + values.Length;
        return Task.CompletedTask;
    }
}
