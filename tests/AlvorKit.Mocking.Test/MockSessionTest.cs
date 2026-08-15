namespace AlvorKit;

[TestClass]
public sealed class MockSessionTest
{
    /// <summary>Sessions enter, nest, and restore the ambient scope in LIFO order.</summary>
    [TestMethod]
    public void Session_Nesting_RestoresParent()
    {
        Assert.IsNull(MockSession.Current);
        using var outer = Mock.Session();
        Assert.AreSame(outer, MockSession.Current);

        using (var inner = Mock.Session())
            Assert.AreSame(inner, MockSession.Current);

        Assert.AreSame(outer, MockSession.Current);
    }

    /// <summary>Session context flows through normal task execution.</summary>
    [TestMethod]
    public async Task Session_Await_FlowsCurrentContext()
    {
        using var session = Mock.Session();

        var observed = await Task.Run(() => MockSession.Current);

        Assert.AreSame(session, observed);
    }

    /// <summary>An explicit run restores session context when execution-context flow is suppressed.</summary>
    [TestMethod]
    public async Task Session_Run_WorksWithSuppressedFlow()
    {
        using var session = Mock.Session();
        Task<MockSession?> task;
        using (ExecutionContext.SuppressFlow())
        {
            task = Task.Run(
                () => session.Run(
                    async () =>
                    {
                        await Task.Yield();
                        return MockSession.Current;
                    }));
        }

        Assert.AreSame(session, await task);
        Assert.AreSame(session, MockSession.Current);
    }

    /// <summary>Explicit execution restores the prior ambient scope after user failure.</summary>
    [TestMethod]
    public void Session_Run_UserFailureRestoresPreviousScope()
    {
        using var outer = Mock.Session();
        using var inner = Mock.Session();
        var expected = new InvalidOperationException("run");

        var actual = Assert.Throws<InvalidOperationException>(
            () => outer.Run(() => throw expected));

        Assert.AreSame(expected, actual);
        Assert.AreSame(inner, MockSession.Current);
    }

    /// <summary>Cross-mock calls receive one monotonically increasing logical entry order.</summary>
    [TestMethod]
    public void Session_CrossMockCalls_ShareTimeline()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();

        first.GetValue();
        second.GetValue();
        first.ComputeSum(1, 2);
        var checkpoint = session.Checkpoint();

        var invocations = session.SnapshotThrough(checkpoint);
        Assert.AreEqual(3, invocations.Length);
        Assert.AreEqual(1L, invocations[0].Coordinate.Sequence);
        Assert.AreEqual(2L, invocations[1].Coordinate.Sequence);
        Assert.AreEqual(3L, invocations[2].Coordinate.Sequence);
        Assert.AreEqual(
            invocations[0].Coordinate.TimelineId,
            invocations[2].Coordinate.TimelineId);
    }

    /// <summary>Setup capture receives no session sequence number.</summary>
    [TestMethod]
    public void Session_SetupCapture_DoesNotAdvanceTimeline()
    {
        var mock = Mock.Create<IMockTarget>();
        using var session = Mock.Session();

        Mock.When(mock.GetValue).Return(42);
        var beforeCall = session.Checkpoint();
        Assert.AreEqual(0L, beforeCall.Sequence);

        Assert.AreEqual(42, mock.GetValue());
        Assert.AreEqual(1L, session.Checkpoint().Sequence);

        var raisedValue = 0;
        mock.OnActionEvent += value => raisedValue = value;
        var beforeRaise = session.Checkpoint();

        Mock.Raise(() => mock.OnActionEvent += null!, 12);

        Assert.AreEqual(12, raisedValue);
        Assert.AreEqual(beforeRaise.Sequence, session.Checkpoint().Sequence);
    }

    /// <summary>Concurrent callers across mocks receive every unique session sequence exactly once.</summary>
    [TestMethod]
    public async Task Session_ConcurrentCrossMockCallsHaveUniqueLogicalOrder()
    {
        const int callCount = 96;
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var callers = new Task[callCount];

        for (var i = 0; i < callers.Length; i++)
        {
            var value = i;
            callers[i] = Task.Run(
                async () =>
                {
                    await start.Task;
                    var mock = (value & 1) == 0 ? first : second;
                    mock.ComputeSum(value, 1);
                });
        }

        start.SetResult();
        await Task.WhenAll(callers);

        var invocations = session.SnapshotThrough(session.Checkpoint());
        Assert.AreEqual(callCount, invocations.Length);
        for (var i = 0; i < invocations.Length; i++)
            Assert.AreEqual(i + 1, invocations[i].Coordinate.Sequence);
    }

    /// <summary>Synchronous return and throw outcomes remain distinct single invocation records.</summary>
    [TestMethod]
    public void Session_SynchronousOutcomeShapesDoNotDuplicateEntries()
    {
        var returning = Mock.Create<IMockTarget>();
        var throwing = Mock.Create<IMockTarget>();
        Mock.When(returning.GetValue).Return(8);
        var expected = new IOException("configured");
        Mock.When(throwing.GetValue).Throw(expected);
        using var session = Mock.Session();

        Assert.AreEqual(8, returning.GetValue());
        Assert.AreSame(
            expected,
            Assert.Throws<IOException>(
                () => throwing.GetValue()));

        var invocations = session.SnapshotThrough(session.Checkpoint());
        Assert.AreEqual(2, invocations.Length);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocations[0].Completion.Kind);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocations[1].Completion.Kind);
        Assert.AreSame(expected, invocations[1].Completion.Exception);
    }

    /// <summary>An optional asynchronous outcome augments its returned invocation instead of appending another.</summary>
    [TestMethod]
    public void Session_OptionalAsyncOutcomeUsesExistingEntry()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        var mocked = Mock.GetMocked(mock)!;
        using var session = Mock.Session();
        session.Register(mocked);
        var identity = new MockInvocationIdentity(
            MockInvocationTarget.ForMock(
                mocked.Invocations.Id,
                typeof(IMockTarget)),
            ReturnTaskMethod,
            "session-test");
        var token = mocked.Invocations.Open(
            identity,
            [],
            session.Timeline);

        mocked.Invocations.CompleteReturned(
            token,
            MockInvocationExecutionSource.Configured,
            MockInvocationReturn.Shallow(
                typeof(Task),
                Task.CompletedTask));
        mocked.Invocations.CompleteAsync(
            token,
            new(MockInvocationAsyncCompletionKind.Succeeded));

        var invocations = session.SnapshotThrough(session.Checkpoint());
        Assert.AreEqual(1, invocations.Length);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocations[0].Completion.Kind);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            invocations[0].AsyncCompletion!.Kind);
    }

    /// <summary>Checkpoint verification uses a stable lower-exclusive, upper-inclusive window.</summary>
    [TestMethod]
    public void Session_Between_ExcludesEarlierAndLaterCalls()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).Return(42);
        mock.GetValue();
        using var session = Mock.Session();
        var before = session.Checkpoint();
        mock.GetValue();
        var through = session.Checkpoint();
        mock.GetValue();

        Mock.Verify(mock.GetValue)
            .Between(before, through)
            .Once();

        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));
    }

    /// <summary>Adjacent checkpoint windows include their upper boundary and exclude their lower boundary.</summary>
    [TestMethod]
    public void Session_Between_UsesStableAdjacentWindows()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        var start = session.Checkpoint();
        mock.ComputeSum(1, 0);
        var first = session.Checkpoint();
        mock.ComputeSum(2, 0);
        mock.ComputeSum(3, 0);
        var middle = session.Checkpoint();
        mock.ComputeSum(4, 0);
        var end = session.Checkpoint();

        Mock.Verify(() => mock.ComputeSum(1, 0))
            .Between(start, first)
            .Once();
        Mock.Verify(() => mock.ComputeSum(2, 0))
            .Between(first, middle)
            .Once();
        Mock.Verify(() => mock.ComputeSum(3, 0))
            .Between(first, middle)
            .Once();
        Mock.Verify(() => mock.ComputeSum(4, 0))
            .Between(middle, end)
            .Once();

        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Checkpoint windows reject reversed or foreign session boundaries.</summary>
    [TestMethod]
    public void Session_Between_InvalidCheckpoints_Throws()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        MockCheckpoint foreign;
        using (var other = Mock.Session())
            foreign = other.Checkpoint();

        using var session = Mock.Session();
        var before = session.Checkpoint();
        mock.GetValue();
        var through = session.Checkpoint();

        Assert.Throws<MockException>(
            () => Mock.Verify(mock.GetValue).Between(through, before));
        Assert.Throws<MockException>(
            () => Mock.Verify(mock.GetValue).Between(before, foreign));
    }

    /// <summary>Sequence verification matches and marks an exact cross-mock logical order.</summary>
    [TestMethod]
    public void Session_VerifySequence_MatchesCrossMockOrder()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        first.RaiseEvent();
        second.RaiseEvent();
        first.RaiseEvent();

        session.VerifySequence(
            first.RaiseEvent,
            second.RaiseEvent,
            first.RaiseEvent);

        Mock.VerifyNoOtherCalls(first);
        Mock.VerifyNoOtherCalls(second);
    }

    /// <summary>Failed sequence verification reports the first divergence and marks no calls.</summary>
    [TestMethod]
    public void Session_VerifySequence_DivergenceMarksNothing()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        first.RaiseEvent();
        second.RaiseEvent();

        var exception = Assert.Throws<MockException>(
            () => session.VerifySequence(
                second.RaiseEvent,
                first.RaiseEvent));

        StringAssert.Contains(exception.Message, "position 0");
        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(first));
        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(second));
    }

    /// <summary>Sequence verification rejects missing and extra actual calls deterministically.</summary>
    [TestMethod]
    public void Session_VerifySequence_MissingOrExtraCallThrows()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        mock.RaiseEvent();

        Assert.Throws<MockException>(
            () => session.VerifySequence());
        Assert.Throws<MockException>(
            () => session.VerifySequence(
                mock.RaiseEvent,
                mock.RaiseEvent));
    }

    /// <summary>Parallel sessions retain independent timelines and histories.</summary>
    [TestMethod]
    public async Task Session_ParallelScopes_AreIsolated()
    {
        static Task<(long Timeline, int Count)> Run()
        {
            return Task.Run(
                () =>
                {
                    var mock = Mock.CreateLoose<IMockTarget>();
                    using var session = Mock.Session();
                    mock.GetValue();
                    mock.GetValue();
                    var checkpoint = session.Checkpoint();
                    return (
                        session.Timeline.Id,
                        session.SnapshotThrough(checkpoint).Length);
                });
        }

        var first = Run();
        var second = Run();
        var results = await Task.WhenAll(first, second);

        Assert.AreNotEqual(results[0].Timeline, results[1].Timeline);
        Assert.AreEqual(2, results[0].Count);
        Assert.AreEqual(2, results[1].Count);
    }

    /// <summary>Separate sessions see only their own timeline even when they share one instance mock.</summary>
    [TestMethod]
    public void Session_SharedMockHistoryIsTimelineIsolated()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        long firstTimeline;

        using (var first = Mock.Session())
        {
            mock.ComputeSum(1, 0);
            var history = first.SnapshotThrough(
                first.Checkpoint());
            Assert.AreEqual(1, history.Length);
            firstTimeline = history[0].Coordinate.TimelineId;
        }

        using (var second = Mock.Session())
        {
            mock.ComputeSum(2, 0);
            var history = second.SnapshotThrough(
                second.Checkpoint());
            Assert.AreEqual(1, history.Length);
            Assert.AreNotEqual(
                firstTimeline,
                history[0].Coordinate.TimelineId);
            Assert.AreEqual(
                2,
                history[0].Arguments[0].Entry.Value);
        }
    }

    /// <summary>Clearing swaps epochs while a deterministically blocked invocation completes in its entry epoch.</summary>
    [TestMethod]
    public async Task Session_ClearDuringBlockedCallKeepsRetiredCompletion()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        var entered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(mock.RaiseEvent)
            .Do(_ =>
            {
                entered.TrySetResult();
                release.Task.GetAwaiter().GetResult();
            });
        using var session = Mock.Session();
        var call = Task.Run(mock.RaiseEvent);

        await entered.Task;
        var ledger = Mock.GetMocked(mock)!.Invocations;
        MockInvocationLedgerSnapshot retired;
        try
        {
            retired = ledger.Snapshot();
            Assert.AreEqual(
                MockInvocationCompletionKind.Pending,
                retired.Invocations[0].Completion.Kind);

            Mock.ClearInvocations(mock);
            Assert.AreEqual(0, ledger.Snapshot().Invocations.Length);
        }
        finally
        {
            release.TrySetResult();
        }

        await call;

        var completed = ledger.Refresh(retired);
        Assert.AreEqual(1, completed.Invocations.Length);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            completed.Invocations[0].Completion.Kind);
        Assert.AreEqual(0, ledger.Snapshot().Invocations.Length);

        mock.RaiseEvent();
        var current = ledger.Snapshot();
        Assert.AreEqual(1, current.Invocations.Length);
        Assert.AreEqual(1, current.Epoch.Number);
        Assert.AreEqual(2, current.Invocations[0].Coordinate.Sequence);
    }

    /// <summary>Cleared epochs release shallow values after tokens and snapshots are gone.</summary>
    [TestMethod]
    public void Session_ClearedEpochValuesBecomeCollectible()
    {
        var ledger = new MockInvocationLedger();

        var retained = RecordThenClear(ledger);
        CollectGarbage();

        Assert.IsFalse(retained.IsAlive);
        Assert.AreEqual(0, ledger.Snapshot().Invocations.Length);
    }

    /// <summary>Out-of-order disposal fails without abandoning either scope.</summary>
    [TestMethod]
    public void Session_OutOfOrderDispose_ThrowsAndCanRecover()
    {
        var outer = Mock.Session();
        var inner = Mock.Session();

        Assert.Throws<MockException>(outer.Dispose);
        inner.Dispose();
        outer.Dispose();

        Assert.IsNull(MockSession.Current);
    }

    /// <summary>Disposed sessions reject checkpoints and explicit execution.</summary>
    [TestMethod]
    public void Session_Disposed_RejectsUse()
    {
        var session = Mock.Session();
        session.Dispose();
        session.Dispose();

        Assert.Throws<ObjectDisposedException>(
            () => session.Checkpoint());
        Assert.Throws<ObjectDisposedException>(() => session.Run(() => { }));
        Assert.Throws<ObjectDisposedException>(
            () => session.Run(static () => 1));

        using var active = Mock.Session();
        Assert.Throws<ArgumentNullException>(
            () => active.Run((Action)null!));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference RecordThenClear(
        MockInvocationLedger ledger)
    {
        var value = new object();
        var reference = new WeakReference(value);
        var identity = new MockInvocationIdentity(
            MockInvocationTarget.ForMock(
                ledger.Id,
                typeof(MockSessionTest)),
            RetainMethod,
            "session-test");
        MockInvocationArgumentSnapshot[] entries =
        [
            MockInvocationArgumentSnapshot.Shallow(
                0,
                typeof(object),
                MockSnapshotPhase.Entry,
                value)
        ];
        var token = ledger.Open(identity, entries);
        ledger.CompleteReturned(
            token,
            MockInvocationExecutionSource.LooseFallback,
            MockInvocationReturn.Void());
        ledger.ClearEpoch();
        return reference;
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static Task ReturnTask() => Task.CompletedTask;

    private static void Retain(object value)
    {
        _ = value;
    }

    private static readonly MethodInfo ReturnTaskMethod =
        typeof(MockSessionTest).GetMethod(
            nameof(ReturnTask),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo RetainMethod =
        typeof(MockSessionTest).GetMethod(
            nameof(Retain),
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
