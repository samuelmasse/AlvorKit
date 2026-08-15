namespace AlvorKit;

/// <summary>Proves session ownership and exact identity for receiver-free interception runtime dispatch.</summary>
[TestClass]
public sealed class MockReceiverFreeRuntimeTest
{
    private static int nextOffset;

    /// <summary>No-session static dispatch calls the original without history-path allocation.</summary>
    [TestMethod]
    public void Bind_NoSession_BypassesStateAndAllocation()
    {
        Func<int, int> call = Bind(out _);
        Assert.IsNull(MockSession.Current);
        Assert.AreEqual(6, call(3));

        const int operations = 1_024;
        long sum = 0;
        long allocatedBefore =
            GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < operations; index++)
            sum += call(index);
        long allocated =
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        Assert.AreEqual(0, allocated);
        Assert.AreEqual(
            (long)(operations - 1) * operations,
            sum);
    }

    /// <summary>Nested sessions isolate member setups and restore the outer session after disposal.</summary>
    [TestMethod]
    public void Bind_NestedSessions_IsolateAndRestoreMemberSetup()
    {
        Func<int, int> first = Bind(out _);
        Func<int, int> second = Bind(out _);

        using MockSession outer = Mock.Session();
        Mock.When(() => first(2)).Return(101);
        Assert.AreEqual(101, second(2));

        long innerId;
        using (MockSession inner = Mock.Session())
        {
            innerId = inner.Id;
            Assert.AreEqual(4, first(2));
            Mock.When(() => second(2)).Return(202);

            Assert.AreEqual(202, first(2));
            Assert.AreEqual(202, second(2));
            MockInvocation[] innerHistory =
                inner.SnapshotThrough(inner.Checkpoint());
            Assert.AreEqual(3, innerHistory.Length);
            Assert.IsTrue(innerHistory.All(invocation =>
                invocation.Identity.Target.OwnerId == inner.Id));
        }

        Assert.AreEqual(outer, MockSession.Current);
        Assert.AreNotEqual(outer.Id, innerId);
        Assert.AreEqual(101, first(2));
        MockInvocation[] outerHistory =
            outer.SnapshotThrough(outer.Checkpoint());
        Assert.AreEqual(2, outerHistory.Length);
        Assert.IsTrue(outerHistory.All(invocation =>
            invocation.Identity.Target.OwnerId == outer.Id));
    }

    /// <summary>Parallel sessions sharing one wrapper retain independent setup and history owners.</summary>
    [TestMethod]
    public void Bind_ParallelSessions_IsolateSetupAndHistory()
    {
        Func<int, int> call = Bind(out _);
        using var ready = new CountdownEvent(2);
        using var start = new ManualResetEventSlim();

        Task<(int Value, long SessionId, long OwnerId)> first =
            Task.Run(() => RunParallelSession(
                call,
                301,
                ready,
                start));
        Task<(int Value, long SessionId, long OwnerId)> second =
            Task.Run(() => RunParallelSession(
                call,
                401,
                ready,
                start));
        ready.Wait();
        start.Set();
        Task.WaitAll(first, second);

        Assert.AreEqual(301, first.Result.Value);
        Assert.AreEqual(401, second.Result.Value);
        Assert.AreNotEqual(
            first.Result.SessionId,
            second.Result.SessionId);
        Assert.AreEqual(
            first.Result.SessionId,
            first.Result.OwnerId);
        Assert.AreEqual(
            second.Result.SessionId,
            second.Result.OwnerId);
    }

    /// <summary>Member-wide sequences span sites while checkpoints and site filters retain exact identity.</summary>
    [TestMethod]
    public void Bind_SharedSequenceAndCheckpoints_RespectExactSites()
    {
        Func<int, int> first = Bind(out MockCallSite firstSite);
        Func<int, int> second = Bind(out MockCallSite secondSite);

        using MockSession session = Mock.Session();
        MockCheckpoint before = session.Checkpoint();
        Mock.When(() => first(Arg.Any<int>()))
            .ReturnSequence(11, 22, 33);

        Assert.AreEqual(11, first(1));
        Assert.AreEqual(22, second(2));
        MockCheckpoint middle = session.Checkpoint();
        Assert.AreEqual(33, first(3));
        MockCheckpoint through = session.Checkpoint();

        Mock.Verify(() => first(Arg.Any<int>()))
            .Between(before, middle)
            .Exactly(2);
        Mock.Verify(() => second(Arg.Any<int>()))
            .Between(middle, through)
            .Once();
        Mock.Verify(() => first(Arg.Any<int>()))
            .AtSite(firstSite)
            .Between(before, through)
            .Exactly(2);
        Mock.Verify(() => second(Arg.Any<int>()))
            .AtSite(secondSite)
            .Between(before, through)
            .Once();

        Mock.When(() => first(9))
            .AtSite(firstSite)
            .Return(90);
        Assert.AreEqual(90, first(9));
        Assert.AreEqual(33, second(9));

        MockInvocation[] history =
            session.SnapshotThrough(session.Checkpoint());
        AssertSiteIdentity(
            history,
            session.Id,
            firstSite,
            expectedCount: 3);
        AssertSiteIdentity(
            history,
            session.Id,
            secondSite,
            expectedCount: 2);
    }

    /// <summary>Callbacks, throws, strict failure, and passthrough complete once at their exact sites.</summary>
    [TestMethod]
    public void Bind_CallbackThrowAndPassthrough_CompleteAtExactSites()
    {
        Func<int, int> callback = Bind(out MockCallSite callbackSite);
        Func<int, int> throwing = Bind(out MockCallSite throwSite);
        Func<int, int> passthrough = Bind(out MockCallSite passthroughSite);
        Func<int, int> strict = Bind(out MockCallSite strictSite);
        var expected = new IOException("receiver-free");
        var originalFailure =
            new InvalidOperationException("receiver-free original");
        Func<int, int> originalThrow = Bind(
            out MockCallSite originalThrowSite,
            _ => throw originalFailure);
        int callbackCount = 0;

        using MockSession session = Mock.Session();
        Mock.When(() => callback(3))
            .AtSite(callbackSite)
            .Answer(call =>
            {
                callbackCount++;
                return call.Argument<int>(0) + 40;
            });
        Mock.When(() => throwing(4))
            .AtSite(throwSite)
            .Throw(expected);
        Mock.When(() => passthrough(5))
            .AtSite(passthroughSite)
            .Passthrough();
        Mock.When(() => strict(6))
            .AtSite(strictSite)
            .Strict();

        Assert.AreEqual(43, callback(3));
        Exception actual = Assert.ThrowsExactly<IOException>(
            () => throwing(4));
        Assert.AreEqual(10, passthrough(5));
        _ = Assert.ThrowsExactly<MockException>(
            () => strict(6));
        Exception actualOriginal =
            Assert.ThrowsExactly<InvalidOperationException>(
                () => originalThrow(7));

        Assert.AreSame(expected, actual);
        Assert.AreSame(originalFailure, actualOriginal);
        Assert.AreEqual(1, callbackCount);
        MockInvocation[] history =
            session.SnapshotThrough(session.Checkpoint());
        Assert.AreEqual(5, history.Length);

        MockInvocation callbackInvocation =
            InvocationAt(history, callbackSite);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            callbackInvocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            callbackInvocation.Completion.Kind);

        MockInvocation thrownInvocation =
            InvocationAt(history, throwSite);
        Assert.AreSame(
            expected,
            thrownInvocation.Completion.Exception);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            thrownInvocation.Completion.FailureStage);

        MockInvocation passthroughInvocation =
            InvocationAt(history, passthroughSite);
        Assert.AreEqual(
            MockInvocationExecutionSource.ReceiverFreeOriginal,
            passthroughInvocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            passthroughInvocation.Completion.Kind);

        MockInvocation strictInvocation =
            InvocationAt(history, strictSite);
        Assert.AreEqual(
            MockInvocationExecutionSource.StrictFailure,
            strictInvocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            strictInvocation.Completion.FailureStage);

        MockInvocation originalThrowInvocation =
            InvocationAt(history, originalThrowSite);
        Assert.AreSame(
            originalFailure,
            originalThrowInvocation.Completion.Exception);
        Assert.AreEqual(
            MockInvocationExecutionSource.ReceiverFreeOriginal,
            originalThrowInvocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.OriginalImplementation,
            originalThrowInvocation.Completion.FailureStage);
    }

    /// <summary>Receiver-free and instance calls share one checkpointed logical order.</summary>
    [TestMethod]
    public void Bind_InstanceAndReceiverFreeCalls_ShareLogicalSequence()
    {
        Func<int, int> receiverFree = Bind(out _);
        var instance =
            Mock.Create<IReceiverFreeOrderTarget>();

        using MockSession session = Mock.Session();
        Mock.When(() => instance.Step(1)).Return(101);
        Mock.When(() => instance.Step(3)).Return(303);
        MockCheckpoint before = session.Checkpoint();

        Assert.AreEqual(101, instance.Step(1));
        Assert.AreEqual(4, receiverFree(2));
        Assert.AreEqual(303, instance.Step(3));
        MockCheckpoint through = session.Checkpoint();

        session.VerifySequence(
            before,
            through,
            () => instance.Step(1),
            () => receiverFree(2),
            () => instance.Step(3));
        Mock.VerifyNoOtherCalls(instance);
        Mock.Verify(() => receiverFree(2))
            .Between(before, through)
            .Once();
    }

    /// <summary>Disposal releases receiver-free setup targets while the disposed session remains reachable.</summary>
    [TestMethod]
    public void Dispose_ReleasesReceiverFreeSetupGraph()
    {
        Func<int, int> call = Bind(out _);
        (MockSession session, WeakReference payload) =
            CreateDisposedSessionProof(call);

        Collect();

        Assert.IsFalse(payload.IsAlive);
        GC.KeepAlive(session);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (
        MockSession Session,
        WeakReference Payload)
        CreateDisposedSessionProof(Func<int, int> call)
    {
        MockSession session = Mock.Session();
        var payload = new ReceiverFreeCallbackPayload();
        var reference = new WeakReference(payload);
        Mock.When(() => call(7)).Answer(payload.Answer);
        Assert.AreEqual(507, call(7));
        session.Dispose();
        return (session, reference);
    }

    private static (
        int Value,
        long SessionId,
        long OwnerId)
        RunParallelSession(
            Func<int, int> call,
            int configured,
            CountdownEvent ready,
            ManualResetEventSlim start)
    {
        using MockSession session = Mock.Session();
        try
        {
            Mock.When(() => call(6)).Return(configured);
        }
        finally
        {
            ready.Signal();
        }
        start.Wait();

        int value = call(6);
        MockInvocation invocation =
            session.SnapshotThrough(session.Checkpoint()).Single();
        return (
            value,
            session.Id,
            invocation.Identity.Target.OwnerId);
    }

    private static Func<int, int> Bind(
        out MockCallSite callSite,
        Func<int, int>? original = null)
    {
        MethodInfo operation = typeof(ReceiverFreeRuntimeTarget)
            .GetMethod(
                nameof(ReceiverFreeRuntimeTarget.Compute),
                BindingFlags.Static |
                BindingFlags.NonPublic)!;
        MockInterceptionSiteDescriptor descriptor = Site();
        callSite = new(descriptor, operation);
        return MockInterceptionOperationRuntime.Bind(
            descriptor,
            operation,
            original ??
            new Func<int, int>(
                ReceiverFreeRuntimeTarget.Compute));
    }

    private static MockInterceptionSiteDescriptor Site() =>
        new(
            typeof(MockReceiverFreeRuntimeTest).Module.ModuleVersionId,
            typeof(MockReceiverFreeRuntimeTest).MetadataToken,
            Interlocked.Increment(ref nextOffset),
            MockInvocationOperationKind.StaticMethod);

    private static void AssertSiteIdentity(
        ReadOnlySpan<MockInvocation> history,
        long sessionId,
        MockCallSite site,
        int expectedCount)
    {
        int count = 0;
        foreach (MockInvocation invocation in history)
        {
            MockInvocationTarget target = invocation.Identity.Target;
            if (target.IlOffset !=
                site.Descriptor.OriginalIlOffset)
            {
                continue;
            }

            count++;
            Assert.AreEqual(
                MockInvocationTargetKind.CallSite,
                target.Kind);
            Assert.AreEqual(sessionId, target.OwnerId);
            Assert.AreEqual(
                site.Descriptor.ModuleVersionId,
                target.ModuleVersionId);
            Assert.AreEqual(
                site.Descriptor.ContainingMethodToken,
                target.ContainingMethodToken);
            Assert.AreEqual(
                site.Descriptor.OperationKind,
                target.OperationKind);
        }

        Assert.AreEqual(expectedCount, count);
    }

    private static MockInvocation InvocationAt(
        ReadOnlySpan<MockInvocation> history,
        MockCallSite site)
    {
        foreach (MockInvocation invocation in history)
        {
            if (invocation.Identity.Target.IlOffset ==
                site.Descriptor.OriginalIlOffset)
            {
                return invocation;
            }
        }

        throw new AssertFailedException(
            $"No invocation was recorded for '{site}'.");
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

internal static class ReceiverFreeRuntimeTarget
{
    internal static int Compute(int value) => value * 2;
}

internal interface IReceiverFreeOrderTarget
{
    int Step(int value);
}

internal sealed class ReceiverFreeCallbackPayload
{
    internal int Answer(MockCall call) =>
        call.Argument<int>(0) + 500;
}
