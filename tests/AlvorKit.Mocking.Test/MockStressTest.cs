namespace AlvorKit;

[TestClass]
public sealed class MockStressTest
{
    private static readonly TimeSpan StressTimeout =
        TimeSpan.FromMilliseconds(750);
    private static readonly MethodInfo SetupPublicationMethod =
        typeof(MockStressTest).GetMethod(
            nameof(SetupPublicationTarget),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Concurrent ordinary calls publish one completed ledger record for every joined caller.</summary>
    [TestMethod]
    public void ConcurrentOrdinaryCalls_LedgerHasExactCompletedCount()
    {
        const int callerCount = 32;
        var mock = Mock.CreateLoose<IMockTarget>();
        var start = NewSignal();
        var callers = new Task[callerCount];

        for (var index = 0; index < callerCount; index++)
        {
            callers[index] = Task.Run(() =>
            {
                start.Task.GetAwaiter().GetResult();
                mock.GetValue();
            });
        }

        start.SetResult();
        Join(callers);

        var invocations = Snapshot(mock).Invocations;
        Assert.AreEqual(callerCount, invocations.Length);
        for (var index = 0; index < invocations.Length; index++)
        {
            Assert.AreEqual(index + 1, invocations[index].Coordinate.Sequence);
            Assert.AreEqual(
                MockInvocationCompletionKind.Returned,
                invocations[index].Completion.Kind);
            Assert.AreEqual(
                MockInvocationExecutionSource.LooseFallback,
                invocations[index].Completion.Source);
        }

        Mock.Verify(mock.GetValue).Exactly(callerCount);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Concurrent return-sequence claims produce the configured multiset exactly once.</summary>
    [TestMethod]
    public void ConcurrentReturnSequence_ClaimsExactConfiguredMultiset()
    {
        const int callerCount = 32;
        var mock = Mock.Create<IMockTarget>();
        var configured = new int[callerCount];
        var results = new int[callerCount];
        for (var index = 0; index < callerCount; index++)
            configured[index] = index + 100;

        Mock.When(mock.GetValue).ReturnSequence(configured);
        var start = NewSignal();
        var callers = new Task[callerCount];
        for (var index = 0; index < callerCount; index++)
        {
            var resultIndex = index;
            callers[index] = Task.Run(() =>
            {
                start.Task.GetAwaiter().GetResult();
                results[resultIndex] = mock.GetValue();
            });
        }

        start.SetResult();
        Join(callers);

        CollectionAssert.AreEquivalent(configured, results);
        Assert.AreEqual(callerCount, Snapshot(mock).Invocations.Length);
        Mock.Verify(mock.GetValue).Exactly(callerCount);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Parallel ordinary answers can reenter the same mock without losing outer or nested calls.</summary>
    [TestMethod]
    public void ConcurrentAnswer_ReentersSameMockWithExactOuterAndNestedCounts()
    {
        const int callerCount = 16;
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).Return(10);
        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), 1))
            .Answer(call => mock.GetValue() + call.Argument<int>(0));

        var start = NewSignal();
        var results = new int[callerCount];
        var callers = new Task[callerCount];
        for (var index = 0; index < callerCount; index++)
        {
            var callIndex = index;
            callers[index] = Task.Run(() =>
            {
                start.Task.GetAwaiter().GetResult();
                results[callIndex] = mock.ComputeSum(callIndex, 1);
            });
        }

        start.SetResult();
        Join(callers);

        for (var index = 0; index < callerCount; index++)
            Assert.AreEqual(index + 10, results[index]);

        Assert.AreEqual(callerCount * 2, Snapshot(mock).Invocations.Length);
        Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), 1))
            .Exactly(callerCount);
        Mock.Verify(mock.GetValue).Exactly(callerCount);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Concurrent readers observe only complete immutable setup generations while a writer publishes.</summary>
    [TestMethod]
    public void SetupPublication_ConcurrentReadersObserveOnlyCompleteGenerations()
    {
        const int publicationCount = 32;
        const int readerCount = 4;
        var store = new MockSetupStore();
        store.Add(Setup(0));
        using var publicationBarrier = new Barrier(readerCount + 1);
        var observations = new MockSetup[readerCount][][];
        var participants = new Task[readerCount + 1];

        participants[0] = LongRunningTask(() =>
        {
            for (var value = 1; value <= publicationCount; value++)
            {
                SignalAndWait(
                    publicationBarrier,
                    $"Writer did not enter publication round {value}.");
                store.Add(Setup(value));
                SignalAndWait(
                    publicationBarrier,
                    $"Writer did not finish publication round {value}.");
            }
        });

        for (var readerIndex = 0; readerIndex < readerCount; readerIndex++)
        {
            var capture = readerIndex;
            observations[capture] = new MockSetup[publicationCount][];
            participants[capture + 1] = LongRunningTask(() =>
            {
                for (var round = 0; round < publicationCount; round++)
                {
                    SignalAndWait(
                        publicationBarrier,
                        $"Reader {capture} did not enter publication round {round + 1}.");
                    observations[capture][round] = store.Snapshot();
                    SignalAndWait(
                        publicationBarrier,
                        $"Reader {capture} did not finish publication round {round + 1}.");
                }
            });
        }

        Join(participants);

        for (var readerIndex = 0; readerIndex < readerCount; readerIndex++)
        {
            for (var round = 0; round < publicationCount; round++)
            {
                var snapshot = observations[readerIndex][round];
                var priorLength = round + 1;
                Assert.IsTrue(
                    snapshot.Length == priorLength ||
                    snapshot.Length == priorLength + 1,
                    $"Reader {readerIndex} observed length {snapshot.Length} " +
                    $"during publication round {round + 1}.");
                AssertCompleteGeneration(snapshot);
            }
        }

        var final = store.Snapshot();
        Assert.AreEqual(publicationCount + 1, final.Length);
        AssertCompleteGeneration(final);
        for (var value = 0; value <= publicationCount; value++)
        {
            Assert.AreEqual(
                value,
                store.Find(SetupPublicationMethod, [value])!.Claim().Value);
        }
    }

    /// <summary>A checkpoint window remains stable while already-gated later callers continue and join.</summary>
    [TestMethod]
    public void Checkpoint_LaterJoinedCallsDoNotMoveCapturedBoundary()
    {
        const int laterCallerCount = 8;
        var mock = Mock.Create<IMockTarget>();
        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), 0))
            .Answer(call => call.Argument<int>(0) + 100);

        using var session = Mock.Session();
        using var ready = new CountdownEvent(laterCallerCount);
        var startLater = NewSignal();
        var laterResults = new int[laterCallerCount];
        var laterCallers = new Task[laterCallerCount];
        for (var index = 0; index < laterCallerCount; index++)
        {
            var callIndex = index;
            laterCallers[index] = Task.Run(() =>
            {
                ready.Signal();
                startLater.Task.GetAwaiter().GetResult();
                laterResults[callIndex] =
                    mock.ComputeSum(callIndex + 2, 0);
            });
        }

        var readyInTime = ready.Wait(StressTimeout);
        if (!readyInTime)
            startLater.TrySetResult();
        Assert.IsTrue(readyInTime, "Later callers did not reach their deterministic gate.");

        var before = session.Checkpoint();
        Assert.AreEqual(101, mock.ComputeSum(1, 0));
        var through = session.Checkpoint();
        startLater.SetResult();
        Join(laterCallers);
        var final = session.Checkpoint();

        for (var index = 0; index < laterCallerCount; index++)
            Assert.AreEqual(index + 102, laterResults[index]);

        Assert.AreEqual(1, session.SnapshotThrough(through).Length);
        Assert.AreEqual(
            laterCallerCount + 1,
            session.SnapshotThrough(final).Length);
        Mock.Verify(() => mock.ComputeSum(1, 0))
            .Between(before, through)
            .Once();
        Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), 0))
            .Between(through, final)
            .Exactly(laterCallerCount);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Clearing a mock retires a blocked callback's epoch while subsequent calls use the new epoch.</summary>
    [TestMethod]
    public void ClearInvocations_BlockedCallbackCompletesInRetiredEpoch()
    {
        var mock = Mock.Create<IMockTarget>();
        var entered = NewSignal();
        var release = NewSignal();
        Mock.When(mock.GetValue)
            .Answer(_ =>
            {
                entered.TrySetResult();
                release.Task.GetAwaiter().GetResult();
                return 41;
            });
        Mock.When(() => mock.ComputeSum(2, 3)).Return(5);

        var blockedCaller = Task.Run(mock.GetValue);
        var enteredInTime = entered.Task.Wait(StressTimeout);
        if (!enteredInTime)
            release.TrySetResult();
        Assert.IsTrue(enteredInTime, "The configured callback did not reach its deterministic gate.");

        var ledger = Mock.GetMocked(mock)!.Invocations;
        var retired = ledger.Snapshot();
        try
        {
            Assert.AreEqual(0L, retired.Epoch.Number);
            Assert.AreEqual(1, retired.Invocations.Length);
            Assert.AreEqual(
                MockInvocationCompletionKind.Pending,
                retired.Invocations[0].Completion.Kind);

            Mock.ClearInvocations(mock);
            var afterClear = ledger.Snapshot();
            Assert.AreEqual(1L, afterClear.Epoch.Number);
            Assert.AreEqual(0, afterClear.Invocations.Length);
            Assert.AreEqual(5, mock.ComputeSum(2, 3));
        }
        finally
        {
            release.TrySetResult();
        }

        Join([blockedCaller]);
        Assert.AreEqual(41, blockedCaller.Result);

        var completedRetired = ledger.Refresh(retired);
        Assert.AreEqual(0L, completedRetired.Epoch.Number);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            completedRetired.Invocations[0].Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            completedRetired.Invocations[0].Completion.Source);

        var current = ledger.Snapshot();
        Assert.AreEqual(1L, current.Epoch.Number);
        Assert.AreEqual(1, current.Invocations.Length);
        Assert.AreEqual(
            nameof(IMockTarget.ComputeSum),
            current.Invocations[0].Identity.Operation.Name);
        Mock.Verify(() => mock.ComputeSum(2, 3)).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Concurrent calls across two mocks receive one complete session-wide numbering sequence.</summary>
    [TestMethod]
    public void Session_ConcurrentCrossMockCallsShareExactNumbering()
    {
        const int callerCount = 32;
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        var start = NewSignal();
        var callers = new Task[callerCount];

        for (var index = 0; index < callerCount; index++)
        {
            var callIndex = index;
            callers[index] = Task.Run(() =>
            {
                start.Task.GetAwaiter().GetResult();
                if ((callIndex & 1) == 0)
                    first.ComputeSum(callIndex, 0);
                else
                    second.ComputeSum(callIndex, 0);
            });
        }

        start.SetResult();
        Join(callers);

        var checkpoint = session.Checkpoint();
        var invocations = session.SnapshotThrough(checkpoint);
        var firstOwner = Mock.GetMocked(first)!.Invocations.Id;
        var secondOwner = Mock.GetMocked(second)!.Invocations.Id;
        var firstCount = 0;
        var secondCount = 0;

        Assert.AreEqual(callerCount, invocations.Length);
        for (var index = 0; index < invocations.Length; index++)
        {
            Assert.AreEqual(index + 1, invocations[index].Coordinate.Sequence);
            Assert.AreEqual(
                checkpoint.TimelineId,
                invocations[index].Coordinate.TimelineId);
            if (invocations[index].Identity.Target.OwnerId == firstOwner)
                firstCount++;
            else if (invocations[index].Identity.Target.OwnerId == secondOwner)
                secondCount++;
            else
                Assert.Fail("A session invocation belonged to neither participating mock.");
        }

        Assert.AreEqual(callerCount / 2, firstCount);
        Assert.AreEqual(callerCount / 2, secondCount);
        Mock.Verify(() => first.ComputeSum(Arg.Any<int>(), 0))
            .Exactly(callerCount / 2);
        Mock.Verify(() => second.ComputeSum(Arg.Any<int>(), 0))
            .Exactly(callerCount / 2);
        Mock.VerifyNoOtherCalls(first);
        Mock.VerifyNoOtherCalls(second);
    }

    /// <summary>Overlapping parallel sessions retain separate timelines that each begin at sequence one.</summary>
    [TestMethod]
    public void Session_ParallelOverlappingScopesRemainIsolated()
    {
        const int callCount = 12;
        using var ready = new CountdownEvent(2);
        var start = NewSignal();

        Task<(long TimelineId, long[] Sequences)> RunSession() =>
            Task.Run(() =>
            {
                var mock = Mock.CreateLoose<IMockTarget>();
                using var session = Mock.Session();
                ready.Signal();
                start.Task.GetAwaiter().GetResult();
                for (var index = 0; index < callCount; index++)
                    mock.ComputeSum(index, 0);

                var checkpoint = session.Checkpoint();
                var invocations = session.SnapshotThrough(checkpoint);
                var sequences = new long[invocations.Length];
                for (var index = 0; index < invocations.Length; index++)
                    sequences[index] = invocations[index].Coordinate.Sequence;
                return (session.Timeline.Id, sequences);
            });

        var first = RunSession();
        var second = RunSession();
        var readyInTime = ready.Wait(StressTimeout);
        if (!readyInTime)
            start.TrySetResult();
        Assert.IsTrue(readyInTime, "Parallel sessions did not reach their deterministic gate.");
        start.SetResult();
        Join([first, second]);

        Assert.AreNotEqual(first.Result.TimelineId, second.Result.TimelineId);
        AssertSessionSequences(first.Result.Sequences, callCount);
        AssertSessionSequences(second.Result.Sequences, callCount);
    }

    /// <summary>Matcher and callback failures complete their records and leave later dispatch unpoisoned.</summary>
    [TestMethod]
    public void UserCodeFailures_CompleteLedgerAndCleanDispatchState()
    {
        var mock = Mock.Create<IMockTarget>();
        var matcherFailure = new InvalidOperationException("matcher");
        var callbackFailure = new IOException("callback");
        Mock.When(() => mock.ComputeSum(
                Arg.Match<int>(_ => throw matcherFailure),
                0))
            .Return(1);
        Mock.When(mock.GetValue)
            .Answer(_ => throw callbackFailure);

        Assert.AreSame(
            matcherFailure,
            Assert.Throws<InvalidOperationException>(
                () => mock.ComputeSum(3, 0)));
        Mock.When(() => mock.ComputeSum(5, 0)).Return(50);
        Assert.AreEqual(50, mock.ComputeSum(5, 0));

        Assert.AreSame(
            callbackFailure,
            Assert.Throws<IOException>(() => mock.GetValue()));
        Mock.When(mock.GetValue).Return(7);
        Assert.AreEqual(7, mock.GetValue());

        var invocations = Snapshot(mock).Invocations;
        Assert.AreEqual(4, invocations.Length);
        AssertFailure(
            invocations[0],
            matcherFailure,
            MockInvocationFailureStage.Matcher);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocations[1].Completion.Kind);
        AssertFailure(
            invocations[2],
            callbackFailure,
            MockInvocationFailureStage.Behavior);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocations[3].Completion.Kind);
        for (var index = 0; index < invocations.Length; index++)
        {
            Assert.AreEqual(index + 1, invocations[index].Coordinate.Sequence);
            Assert.AreNotEqual(
                MockInvocationCompletionKind.Pending,
                invocations[index].Completion.Kind);
        }

        Mock.Verify(() => mock.ComputeSum(3, 0)).Once();
        Mock.Verify(() => mock.ComputeSum(5, 0)).Once();
        Mock.Verify(mock.GetValue).Exactly(2);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Overlapping typed callbacks reenter one mock while retaining each caller's live span frame.</summary>
    [TestMethod]
    public void TypedCallbacks_ConcurrentReentryKeepsLiveFramesIsolated()
    {
        const int callerCount = 16;
        var target = Mock.Create<ITypedCallbackTarget>();
        using var overlap = new Barrier(callerCount);
        Mock.When(target.Ping).Return(1000);
        Mock.When(() => target.Fill(
                Arg.Any<Span<int>>(0)))
            .Do((Span<int> values) =>
            {
                int nested = target.Ping();
                SignalAndWait(
                    overlap,
                    "Typed callbacks did not overlap before the bound.");
                values[1] = values[0] + nested;
            });

        var results = new int[callerCount][];
        var callers = new Task[callerCount];
        for (var index = 0; index < callerCount; index++)
        {
            int callIndex = index;
            callers[index] = LongRunningTask(() =>
            {
                int[] values = [callIndex, 0];
                target.Fill(values);
                results[callIndex] = values;
            });
        }

        Join(callers);

        for (var index = 0; index < callerCount; index++)
        {
            CollectionAssert.AreEqual(
                new[] { index, index + 1000 },
                results[index]);
        }

        ReadOnlySpan<MockInvocation> invocations =
            Snapshot(target).Invocations;
        Assert.AreEqual(callerCount * 2, invocations.Length);
        var fillCount = 0;
        var pingCount = 0;
        foreach (MockInvocation invocation in invocations)
        {
            if (invocation.Identity.Operation.Name ==
                nameof(ITypedCallbackTarget.Fill))
                fillCount++;
            else if (invocation.Identity.Operation.Name ==
                nameof(ITypedCallbackTarget.Ping))
                pingCount++;
            Assert.AreEqual(
                MockInvocationCompletionKind.Returned,
                invocation.Completion.Kind);
            Assert.AreEqual(
                MockInvocationExecutionSource.Configured,
                invocation.Completion.Source);
        }

        Assert.AreEqual(callerCount, fillCount);
        Assert.AreEqual(callerCount, pingCount);
        Mock.Verify(() => target.Fill(
                Arg.Any<Span<int>>(0)))
            .Exactly(callerCount);
        Mock.Verify(target.Ping).Exactly(callerCount);
        Mock.VerifyNoOtherCalls(target);
    }

    /// <summary>Concurrent projectors retain exact entry/exit pairs and recover after one exact failure.</summary>
    [TestMethod]
    public void Projectors_ConcurrentAttributionAndFailureRecovery()
    {
        const int callerCount = 32;
        var target = Mock.CreateLoose<IProjectionTarget>();
        var projectorFailure = new InvalidOperationException("projector");
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgument(
                0,
                (
                    scoped in int value) =>
                    value < 0
                        ? throw projectorFailure
                        : value)
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    value)
            .SnapshotArgumentOnExit(
                1,
                (
                    scoped in int value) =>
                    value)
            .Answer(
                call =>
                {
                    int entry = call.Argument<int>(0);
                    call.SetReference(0, entry + 1000);
                    call.SetReference(1, entry + 2000);
                    return entry;
                });

        var start = NewSignal();
        var returned = new int[callerCount];
        var refExits = new int[callerCount];
        var outExits = new int[callerCount];
        var callers = new Task[callerCount];
        for (var index = 0; index < callerCount; index++)
        {
            int callIndex = index;
            callers[index] = Task.Run(() =>
            {
                start.Task.GetAwaiter().GetResult();
                int value = callIndex;
                returned[callIndex] = target.Exchange(
                    ref value,
                    out int output);
                refExits[callIndex] = value;
                outExits[callIndex] = output;
            });
        }

        start.SetResult();
        Join(callers);

        var invocations = Snapshot(target).Invocations;
        var seen = new bool[callerCount];
        Assert.AreEqual(callerCount, invocations.Length);
        for (var index = 0; index < callerCount; index++)
        {
            Assert.AreEqual(index, returned[index]);
            Assert.AreEqual(index + 1000, refExits[index]);
            Assert.AreEqual(index + 2000, outExits[index]);

            MockInvocation invocation = invocations[index];
            Assert.AreEqual(
                MockInvocationArgumentSnapshotKind.Projected,
                invocation.Arguments[0].Entry.Kind);
            Assert.AreEqual(
                MockInvocationArgumentSnapshotKind.Projected,
                invocation.Arguments[0].Exit.Kind);
            Assert.AreEqual(
                MockInvocationArgumentSnapshotKind.Projected,
                invocation.Arguments[1].Exit.Kind);
            int entry = (int)invocation.Arguments[0].Entry.Value!;
            Assert.IsFalse(seen[entry]);
            seen[entry] = true;
            Assert.AreEqual(
                entry + 1000,
                invocation.Arguments[0].Exit.Value);
            Assert.AreEqual(
                entry + 2000,
                invocation.Arguments[1].Exit.Value);
            Assert.AreEqual(
                entry,
                invocation.Completion.Return!.Value);
        }

        var failingValue = -1;
        Assert.AreSame(
            projectorFailure,
            Assert.Throws<InvalidOperationException>(
                () => target.Exchange(
                    ref failingValue,
                    out _)));

        var laterValue = callerCount;
        Assert.AreEqual(
            callerCount,
            target.Exchange(
                ref laterValue,
                out int laterOutput));
        Assert.AreEqual(callerCount + 1000, laterValue);
        Assert.AreEqual(callerCount + 2000, laterOutput);

        invocations = Snapshot(target).Invocations;
        Assert.AreEqual(callerCount + 2, invocations.Length);
        AssertFailure(
            invocations[callerCount],
            projectorFailure,
            MockInvocationFailureStage.EntryProjector);
        Assert.AreEqual(
            MockUnavailableReason.NoNormalCompletion,
            invocations[callerCount].Arguments[0].Exit.Unavailable!.Reason);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocations[callerCount + 1].Completion.Kind);
        for (var index = 0; index < invocations.Length; index++)
        {
            Assert.AreEqual(index + 1, invocations[index].Coordinate.Sequence);
            Assert.AreNotEqual(
                MockInvocationCompletionKind.Pending,
                invocations[index].Completion.Kind);
        }
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static Task LongRunningTask(Action action) =>
        Task.Factory.StartNew(
            action,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

    private static void Join(Task[] tasks)
    {
        Assert.IsTrue(
            Task.WaitAll(tasks, StressTimeout),
            "Joined callers did not complete within the stress-test bound.");
    }

    private static void SignalAndWait(Barrier barrier, string failureMessage)
    {
        if (!barrier.SignalAndWait(StressTimeout))
            throw new TimeoutException(failureMessage);
    }

    private static MockSetup Setup(int value) =>
        new(
            SetupPublicationMethod,
            [new(value)],
            new MockConstantBehavior(value, []));

    private static int SetupPublicationTarget(int value) => value;

    private static void AssertCompleteGeneration(MockSetup[] snapshot)
    {
        for (var index = 0; index < snapshot.Length; index++)
        {
            var expectedValue = snapshot.Length - index - 1;
            Assert.AreSame(SetupPublicationMethod, snapshot[index].Method);
            Assert.AreEqual(
                expectedValue,
                snapshot[index].Behavior.Claim().Value);
        }
    }

    private static MockInvocationLedgerSnapshot Snapshot(object mock) =>
        Mock.GetMocked(mock)!.Invocations.Snapshot();

    private static void AssertSessionSequences(long[] sequences, int expectedCount)
    {
        Assert.AreEqual(expectedCount, sequences.Length);
        for (var index = 0; index < sequences.Length; index++)
            Assert.AreEqual(index + 1, sequences[index]);
    }

    private static void AssertFailure(
        MockInvocation invocation,
        Exception expected,
        MockInvocationFailureStage stage)
    {
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreSame(expected, invocation.Completion.Exception);
        Assert.AreEqual(stage, invocation.Completion.FailureStage);
    }
}
