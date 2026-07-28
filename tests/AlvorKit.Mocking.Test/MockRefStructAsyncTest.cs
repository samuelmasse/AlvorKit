namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockRefStructAsyncTest
{
    private static readonly TimeSpan CompletionBound =
        TimeSpan.FromMilliseconds(750);

    /// <summary>A Task answer copies borrowed input before suspension and records two ordered stages.</summary>
    [TestMethod]
    public async Task TaskAnswer_CopiesBeforeSuspension()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(() => target.CountTask(
                Arg.Any<ReadOnlySpan<byte>>(0)))
            .Answer((ReadOnlySpan<byte> bytes) =>
                CountAfterAsync(bytes.ToArray(), release.Task));
        byte[] source = [2, 3, 5];

        Task<int> returned = target.CountTask(source);
        MockInvocation synchronous = Single(target);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            synchronous.Completion.Kind);
        Assert.IsNull(synchronous.AsyncCompletion);
        source.AsSpan().Fill(99);
        release.SetResult();

        Assert.AreEqual(10, await returned);
        MockInvocation completed = await Completed(target, 0);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            completed.AsyncCompletion!.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            completed.Completion.Source);
    }

    /// <summary>Non-generic Task and ValueTask answers publish success and cancellation events.</summary>
    [TestMethod]
    public async Task NonGenericAwaitables_RecordSuccessAndCancellation()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Mock.When(() => target.CompleteTask(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                Task.CompletedTask);
        Mock.When(() => target.CompleteValueTask(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                new ValueTask(Task.FromCanceled(canceled.Token)));

        Task task = target.CompleteTask([1]);
        ValueTask valueTask = target.CompleteValueTask([2]);

        await task;
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(
            async () => await valueTask);
        MockInvocation[] invocations = await CompletedAll(target, 2);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            invocations.Single(invocation =>
                invocation.Identity.Operation.Name ==
                nameof(IRefStructAsyncTarget.CompleteTask))
                .AsyncCompletion!.Kind);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Canceled,
            invocations.Single(invocation =>
                invocation.Identity.Operation.Name ==
                nameof(IRefStructAsyncTarget.CompleteValueTask))
                .AsyncCompletion!.Kind);
    }

    /// <summary>A source-backed ValueTask is preserved once and the caller consumes only its Task-backed replacement.</summary>
    [TestMethod]
    public async Task SourceBackedValueTask_IsConsumedOnce()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var source = new SingleUseValueTaskSource<int>();
        Mock.When(() => target.CountValueTask(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                source.Create());

        ValueTask<int> returned =
            target.CountValueTask([13, 21]);
        Assert.AreEqual(1, source.OnCompletedCalls);
        Assert.AreEqual(0, source.GetResultCalls);

        source.SetResult(34);

        Assert.AreEqual(34, await returned);
        MockInvocation completed = await Completed(target, 0);
        Assert.AreEqual(1, source.OnCompletedCalls);
        Assert.AreEqual(1, source.GetResultCalls);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            completed.AsyncCompletion!.Kind);
    }

    /// <summary>Synchronous throws and faulted tasks retain distinct stages and exact exception identity.</summary>
    [TestMethod]
    public async Task CallbackAndFactoryFailures_RemainDistinct()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var callbackThrow =
            new InvalidOperationException("callback sync");
        var callbackFault =
            new IOException("callback async");
        var factoryThrow =
            new ArgumentException("factory sync");
        var factoryFault =
            new FormatException("factory async");
        Mock.When(() => target.ThrowCallback(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> _) =>
                throw callbackThrow);
        Mock.When(() => target.FaultCallback(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> _) =>
                Task.FromException<int>(callbackFault));
        Mock.When(target.ThrowFactory)
            .ReturnFactory(() => throw factoryThrow);
        Mock.When(target.FaultFactory)
            .ReturnFactory(() =>
                Task.FromException<int>(factoryFault));

        Exception callbackThrown =
            Assert.Throws<InvalidOperationException>(
                () => target.ThrowCallback([1]));
        Task<int> callbackTask = target.FaultCallback([2]);
        Exception factoryThrown =
            Assert.Throws<ArgumentException>(
                () => target.ThrowFactory());
        Task<int> factoryTask = target.FaultFactory();
        Exception callbackAwaited =
            await AwaitFailure(callbackTask);
        Exception factoryAwaited =
            await AwaitFailure(factoryTask);
        MockInvocation[] invocations = await CompletedAll(target, 4);

        Assert.AreSame(callbackThrow, callbackThrown);
        Assert.AreSame(factoryThrow, factoryThrown);
        Assert.AreSame(callbackFault, callbackAwaited);
        Assert.AreSame(factoryFault, factoryAwaited);
        AssertSynchronousThrow(
            invocations,
            nameof(IRefStructAsyncTarget.ThrowCallback),
            callbackThrow,
            MockInvocationFailureStage.Behavior);
        AssertSynchronousThrow(
            invocations,
            nameof(IRefStructAsyncTarget.ThrowFactory),
            factoryThrow,
            MockInvocationFailureStage.ReturnFactory);
        AssertAsyncFault(
            invocations,
            nameof(IRefStructAsyncTarget.FaultCallback),
            callbackFault);
        AssertAsyncFault(
            invocations,
            nameof(IRefStructAsyncTarget.FaultFactory),
            factoryFault);
    }

    /// <summary>An allows-ref-struct generic proxy consumes a Span live and retains no borrowed carrier value.</summary>
    [TestMethod]
    public async Task ProxyGenericTaskAnswer_ConsumesSpanWithoutBoxing()
    {
        var target = Mock.Create<IRefStructAsyncGenericTarget>();
        Mock.When(() => target.Count(
                Arg.Any<Span<int>>(0)))
            .Answer((Span<int> values) =>
            {
                int[] copy = values.ToArray();
                values[0] = 55;
                return Task.FromResult(copy.Sum());
            });
        Span<int> values = [8, 13];

        Task<int> returned = target.Count(values);

        Assert.AreEqual(55, values[0]);
        Assert.AreEqual(21, await returned);
        MockInvocation completed = await Completed(target, 0);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            completed.Arguments[0].Entry.Kind);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            completed.AsyncCompletion!.Kind);
    }

    /// <summary>Interface and virtual proxy backends execute task answers without original fallthrough.</summary>
    [TestMethod]
    public async Task BackendMatrix_ExecutesTaskAnswers()
    {
        await AssertBackend(
            Mock.Create<IRefStructAsyncClassTarget>(),
            null);
        var virtualTarget = Mock.Create<RefStructAsyncVirtualTarget>();
        await AssertBackend(
            virtualTarget,
            () => virtualTarget.Calls);
    }

    /// <summary>A pending async event completes in its retired history epoch exactly once.</summary>
    [TestMethod]
    public async Task AsyncCompletion_RefreshesRetiredEpoch()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var completion =
            new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(() => target.CountTask(
                Arg.Any<ReadOnlySpan<byte>>(0)))
            .Answer((ReadOnlySpan<byte> _) =>
                completion.Task);
        MockInvocationLedger ledger =
            Mock.GetMocked(target)!.Invocations;

        Task<int> returned = target.CountTask([1]);
        MockInvocationLedgerSnapshot retired = ledger.Snapshot();
        Mock.ClearInvocations(target);
        completion.SetResult(89);

        Assert.AreEqual(89, await returned);
        MockInvocation refreshed =
            await Completed(ledger, retired);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            refreshed.AsyncCompletion!.Kind);
        Assert.AreEqual(0, ledger.Snapshot().Invocations.Length);
    }

    /// <summary>A captured session checkpoint gains the later async stage without gaining another timeline entry.</summary>
    [TestMethod]
    public async Task AsyncCompletion_RefreshesCapturedSessionCheckpoint()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var completion =
            new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(() => target.CountTask(
                Arg.Any<ReadOnlySpan<byte>>(0)))
            .Answer((ReadOnlySpan<byte> _) =>
                completion.Task);
        using var session = Mock.Session();

        Task<int> returned = target.CountTask([1, 2]);
        MockCheckpoint checkpoint = session.Checkpoint();
        var before = session.SnapshotThrough(checkpoint);
        Assert.AreEqual(1, before.Length);
        Assert.IsNull(before[0].AsyncCompletion);
        MockInvocationCoordinate coordinate = before[0].Coordinate;

        completion.SetResult(17);
        Assert.AreEqual(17, await returned);
        await Completed(target, 0);

        var after = session.SnapshotThrough(checkpoint);
        Assert.AreEqual(1, after.Length);
        Assert.AreEqual(coordinate, after[0].Coordinate);
        Assert.AreEqual(
            checkpoint.Sequence,
            session.Checkpoint().Sequence);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            after[0].AsyncCompletion!.Kind);
    }

    /// <summary>A malformed ValueTask source cannot convert a successful callback return into a synchronous mock failure.</summary>
    [TestMethod]
    public void MalformedValueTaskSource_DoesNotFailDispatch()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var expected =
            new InvalidOperationException("malformed source");
        var source =
            new ThrowingValueTaskSource<int>(expected);
        Mock.When(() => target.CountValueTask(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> _) =>
                source.Create());

        ValueTask<int> returned =
            target.CountValueTask([1]);

        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.IsNull(invocation.AsyncCompletion);
        Exception actual = Assert.Throws<InvalidOperationException>(
            () => returned.GetAwaiter().GetResult());
        Assert.AreSame(expected, actual);
    }

    /// <summary>A declared object result containing a Task receives no asynchronous event.</summary>
    [TestMethod]
    public async Task ObjectReturningTask_IsNotObserved()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var completion =
            new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(() => target.ObjectResult(
                Arg.Any<int>()))
            .Answer((int _) => (object)completion.Task);

        object result = target.ObjectResult(1);
        completion.SetResult(144);

        Assert.AreEqual(144, await (Task<int>)result);
        Assert.IsNull(Single(target).AsyncCompletion);
    }

    /// <summary>An ordinary MockCall answer observes its per-call task.</summary>
    [TestMethod]
    public async Task OrdinaryAnswer_RecordsAsyncCompletion()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        Mock.When(() => target.OrdinaryTask(
                Arg.Any<int>()))
            .Answer(_ => Task.FromResult(73));

        Assert.AreEqual(73, await target.OrdinaryTask(1));
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            (await Completed(target, 0)).AsyncCompletion!.Kind);
    }

    /// <summary>Async registrations retain only the returned Task and completed invocation slot.</summary>
    [TestMethod]
    public void PendingTask_ReleasesMockAndCallbackOwner()
    {
        var completion =
            new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        (WeakReference target, WeakReference owner) =
            ConfigureTransient(completion.Task);

        ForceCollection();

        Assert.IsFalse(target.IsAlive);
        Assert.IsFalse(owner.IsAlive);
        FieldInfo[] fields =
            typeof(MockAsyncCompletionRegistration).GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic);
        CollectionAssert.AreEquivalent(
            new[] { typeof(Task), typeof(MockInvocationSlot) },
            fields.Select(field => field.FieldType).ToArray());
        completion.SetResult(233);
    }

    /// <summary>A state-machine callback with borrowed input rejects before publication.</summary>
    [TestMethod]
    public void BorrowedStateMachineCallback_RejectsBeforePublication()
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        Func<ReadOnlySpan<int>, Task<int>> callback =
            BorrowedStateMachineMarker;
        MockSetupClause<Task<int>> clause = Mock.When(
            () => target.CountMarked(
                Arg.Any<ReadOnlySpan<int>>(0)));

        MockException error = Assert.Throws<MockException>(
            () => clause.Answer((Delegate)callback));

        StringAssert.Contains(error.Message, "copies the value");
        Assert.AreEqual(
            0,
            Mock.GetMocked(target)!.SnapshotSetups().Length);
    }

    private static async Task<int> CountAfterAsync(
        byte[] values,
        Task release)
    {
        await release;
        return values.Sum(value => value);
    }

    private static async Task<Exception> AwaitFailure(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new AssertFailedException(
            "The task completed successfully.");
    }

    private static void AssertSynchronousThrow(
        MockInvocation[] invocations,
        string method,
        Exception expected,
        MockInvocationFailureStage stage)
    {
        MockInvocation invocation = invocations.Single(candidate =>
            candidate.Identity.Operation.Name == method);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.AreEqual(stage, invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
        Assert.IsNull(invocation.AsyncCompletion);
    }

    private static void AssertAsyncFault(
        MockInvocation[] invocations,
        string method,
        Exception expected)
    {
        MockInvocation invocation = invocations.Single(candidate =>
            candidate.Identity.Operation.Name == method);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Faulted,
            invocation.AsyncCompletion!.Kind);
        Assert.AreSame(expected, invocation.AsyncCompletion.Exception);
    }

    private static async Task AssertBackend(
        IRefStructAsyncClassTarget target,
        Func<int>? calls)
    {
        Mock.When(() => target.Count(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                Task.FromResult(values.Length + 30));

        Assert.AreEqual(33, await target.Count([1, 2, 3]));
        if (calls is not null)
            Assert.AreEqual(0, calls());
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            (await Completed(target, 0)).AsyncCompletion!.Kind);
    }

    private static MockInvocation Single(object target) =>
        Mock.GetMocked(target)!.Invocations
            .Snapshot().Invocations[0];

    private static async Task<MockInvocation> Completed(
        object target,
        int index)
    {
        System.Diagnostics.Stopwatch watch =
            System.Diagnostics.Stopwatch.StartNew();
        while (watch.Elapsed < CompletionBound)
        {
            MockInvocation[] invocations =
                [.. Mock.GetMocked(target)!.Invocations
                    .Snapshot().Invocations];
            if (invocations.Length > index
                && invocations[index].AsyncCompletion is not null)
            {
                return invocations[index];
            }

            await Task.Yield();
        }

        throw new AssertFailedException(
            $"Invocation {index} did not publish async completion.");
    }

    private static async Task<MockInvocation[]> CompletedAll(
        object target,
        int count)
    {
        System.Diagnostics.Stopwatch watch =
            System.Diagnostics.Stopwatch.StartNew();
        while (watch.Elapsed < CompletionBound)
        {
            MockInvocation[] invocations =
                [.. Mock.GetMocked(target)!.Invocations
                    .Snapshot().Invocations];
            if (invocations.Length == count
                && invocations.All(invocation =>
                    invocation.Completion.Kind ==
                        MockInvocationCompletionKind.Threw
                    || invocation.AsyncCompletion is not null))
            {
                return invocations;
            }

            await Task.Yield();
        }

        throw new AssertFailedException(
            $"{count} invocations did not reach terminal stages.");
    }

    private static async Task<MockInvocation> Completed(
        MockInvocationLedger ledger,
        MockInvocationLedgerSnapshot retired)
    {
        System.Diagnostics.Stopwatch watch =
            System.Diagnostics.Stopwatch.StartNew();
        while (watch.Elapsed < CompletionBound)
        {
            MockInvocation invocation =
                ledger.Refresh(retired).Invocations[0];
            if (invocation.AsyncCompletion is not null)
                return invocation;
            await Task.Yield();
        }

        throw new AssertFailedException(
            "The retired invocation did not publish async completion.");
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Target, WeakReference Owner)
        ConfigureTransient(Task<int> task)
    {
        var target = Mock.Create<IRefStructAsyncTarget>();
        var owner = new AsyncCallbackOwner(task);
        Mock.When(() => target.CountTask(
                Arg.Any<ReadOnlySpan<byte>>(0)))
            .Answer(owner.Count);
        _ = target.CountTask([1]);
        return (new(target), new(owner));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void ForceCollection()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    [System.Runtime.CompilerServices.AsyncStateMachine(
        typeof(BorrowedMarkerStateMachine))]
    private static Task<int> BorrowedStateMachineMarker(
        ReadOnlySpan<int> values) =>
        Task.FromResult(values.Length);
}

internal interface IRefStructAsyncTarget
{
    Task<int> CountTask(ReadOnlySpan<byte> values);

    ValueTask<int> CountValueTask(ReadOnlySpan<int> values);

    Task CompleteTask(ReadOnlySpan<int> values);

    ValueTask CompleteValueTask(ReadOnlySpan<int> values);

    Task<int> ThrowCallback(ReadOnlySpan<int> values);

    Task<int> FaultCallback(ReadOnlySpan<int> values);

    Task<int> ThrowFactory();

    Task<int> FaultFactory();

    object ObjectResult(int value);

    Task<int> OrdinaryTask(int value);

    Task<int> CountMarked(ReadOnlySpan<int> values);
}

internal interface IRefStructAsyncGenericTarget
{
    Task<int> Count<T>(T values)
        where T : allows ref struct;
}

internal interface IRefStructAsyncClassTarget
{
    Task<int> Count(ReadOnlySpan<int> values);
}

internal class RefStructAsyncVirtualTarget :
    IRefStructAsyncClassTarget
{
    internal int Calls;

    public virtual Task<int> Count(
        ReadOnlySpan<int> values)
    {
        Calls++;
        return Task.FromResult(-1);
    }
}

internal sealed class RefStructAsyncSealedTarget :
    IRefStructAsyncClassTarget
{
    internal int Calls;

    public Task<int> Count(ReadOnlySpan<int> values)
    {
        Calls++;
        return Task.FromResult(-1);
    }
}

internal sealed class RefStructAsyncPartialTarget
{
    internal int CountCalls;
    internal int NeighborCalls;

    public Task<int> Count(ReadOnlySpan<int> values)
    {
        _ = values.Length;
        CountCalls++;
        return Task.FromResult(-1);
    }

    public Task<int> Neighbor(ReadOnlySpan<int> values)
    {
        NeighborCalls++;
        return Task.FromResult(values.Length + 50);
    }
}

internal sealed class AsyncCallbackOwner(Task<int> task)
{
    internal Task<int> Count(ReadOnlySpan<byte> values)
    {
        _ = values.Length;
        return task;
    }
}

internal sealed class SingleUseValueTaskSource<T> :
    System.Threading.Tasks.Sources.IValueTaskSource<T>
{
    private System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore<T>
        core = new()
        {
            RunContinuationsAsynchronously = true
        };

    internal int GetResultCalls;
    internal int OnCompletedCalls;

    internal ValueTask<T> Create() =>
        new(this, core.Version);

    internal void SetResult(T result) =>
        core.SetResult(result);

    public T GetResult(short token)
    {
        Interlocked.Increment(ref GetResultCalls);
        return core.GetResult(token);
    }

    public System.Threading.Tasks.Sources.ValueTaskSourceStatus
        GetStatus(short token) =>
        core.GetStatus(token);

    public void OnCompleted(
        Action<object?> continuation,
        object? state,
        short token,
        System.Threading.Tasks.Sources.ValueTaskSourceOnCompletedFlags flags)
    {
        Interlocked.Increment(ref OnCompletedCalls);
        core.OnCompleted(
            continuation,
            state,
            token,
            flags);
    }
}

internal sealed class ThrowingValueTaskSource<T>(Exception exception) :
    System.Threading.Tasks.Sources.IValueTaskSource<T>
{
    internal ValueTask<T> Create() =>
        new(this, 0);

    public T GetResult(short token) =>
        throw exception;

    public System.Threading.Tasks.Sources.ValueTaskSourceStatus
        GetStatus(short token) =>
        throw exception;

    public void OnCompleted(
        Action<object?> continuation,
        object? state,
        short token,
        System.Threading.Tasks.Sources.ValueTaskSourceOnCompletedFlags flags) =>
        throw exception;
}

internal struct BorrowedMarkerStateMachine :
    System.Runtime.CompilerServices.IAsyncStateMachine
{
    public readonly void MoveNext()
    {
    }

    public readonly void SetStateMachine(
        System.Runtime.CompilerServices.IAsyncStateMachine stateMachine)
    {
    }
}
