namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockTypedCallbackRuntimeTest
{
    /// <summary>A by-value readonly span reaches a direct callback without entering the carrier.</summary>
    [TestMethod]
    public void Observe_ReadOnlySpanReachesTypedCallback()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        int[] observed = [];
        Mock.When(() => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Do((ReadOnlySpan<int> values) =>
                observed = values.ToArray());

        target.Observe([2, 3, 5]);

        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            observed);
        AssertConfiguredReturned(target, 1);
    }

    /// <summary>A mutable span callback edits the caller-owned contents directly.</summary>
    [TestMethod]
    public void Fill_MutableSpanWritesCallerStorage()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.Fill(
                Arg.Any<Span<int>>(0)))
            .Do((Span<int> values) =>
            {
                for (var index = 0; index < values.Length; index++)
                    values[index] = index + 8;
            });
        Span<int> values = stackalloc int[3];

        target.Fill(values);

        CollectionAssert.AreEqual(
            new[] { 8, 9, 10 },
            values.ToArray());
    }

    /// <summary>A mixed typed answer returns its calculation and mutates caller-owned span contents.</summary>
    [TestMethod]
    public void Answer_MixedArgumentsReturnsCalculatedValue()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.Calculate(
                Arg.Any<int>(),
                Arg.Any<ReadOnlySpan<int>>(1),
                Arg.Any<Span<int>>(2)))
            .Answer(
                (
                    int offset,
                    ReadOnlySpan<int> source,
                    Span<int> destination) =>
                {
                    source.CopyTo(destination);
                    return offset + source.Length;
                });
        Span<int> destination = stackalloc int[3];

        int result = target.Calculate(
            13,
            [21, 34, 55],
            destination);

        Assert.AreEqual(16, result);
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            destination.ToArray());
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(
            MockInvocationReturnKind.Shallow,
            invocation.Completion.Return!.Kind);
        Assert.AreEqual(16, invocation.Completion.Return.Value);
    }

    /// <summary>An arbitrary readonly ref struct is consumed live and never retained in history.</summary>
    [TestMethod]
    public void ArbitraryRefStruct_IsConsumedDirectly()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        var observed = 0;
        Mock.When(() => target.Window(
                Arg.Any<TypedRuntimeWindow>(0)))
            .Do((TypedRuntimeWindow window) =>
                observed = window.Values[0] + window.Values[1]);

        target.Window(new([89, 144]));

        Assert.AreEqual(233, observed);
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            invocation.Arguments[0].Entry.Kind);
    }

    /// <summary>An exact natural callback observes in, rewrites ref, and assigns out ref-struct arguments.</summary>
    [TestMethod]
    public void ExactCallback_WritesRefAndOutArgumentsDirectly()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.Exact(
                Arg.Any<ReadOnlySpan<int>>(0),
                ref Arg.AnyRef<Span<int>>(1),
                out _))
            .Do(
                (
                    scoped in ReadOnlySpan<int> source,
                    scoped ref Span<int> destination,
                    scoped out TypedRuntimeWindow written) =>
                {
                    source.CopyTo(destination);
                    destination = destination[..source.Length];
                    written = new(destination);
                });
        ReadOnlySpan<int> source = [377, 610];
        Span<int> destination = stackalloc int[4];

        target.Exact(
            in source,
            ref destination,
            out TypedRuntimeWindow written);

        Assert.AreEqual(2, destination.Length);
        CollectionAssert.AreEqual(
            new[] { 377, 610 },
            destination.ToArray());
        CollectionAssert.AreEqual(
            new[] { 377, 610 },
            written.Values.ToArray());
    }

    /// <summary>An exact natural answer combines ordinary, in, ref, and out arguments and returns directly.</summary>
    [TestMethod]
    public void ExactAnswer_WritesReferencesAndReturnsResult()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.ExactAnswer(
                Arg.Any<int>(),
                Arg.Any<ReadOnlySpan<int>>(1),
                ref Arg.AnyRef<Span<int>>(2),
                out _))
            .Answer(
                (
                    int offset,
                    scoped in ReadOnlySpan<int> source,
                    scoped ref Span<int> destination,
                    scoped out TypedRuntimeWindow written) =>
                {
                    source.CopyTo(destination);
                    destination = destination[..source.Length];
                    written = new(destination);
                    return offset + source.Length;
                });
        ReadOnlySpan<int> source = [987, 1597, 2584];
        Span<int> destination = stackalloc int[4];

        int result = target.ExactAnswer(
            21,
            in source,
            ref destination,
            out TypedRuntimeWindow written);

        Assert.AreEqual(24, result);
        Assert.AreEqual(3, destination.Length);
        CollectionAssert.AreEqual(
            new[] { 987, 1597, 2584 },
            written.Values.ToArray());
    }

    /// <summary>A typed callback throw preserves exact identity and records one configured behavior failure.</summary>
    [TestMethod]
    public void CallbackThrow_PropagatesExactIdentityOnce()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        var expected = new InvalidOperationException("typed callback");
        var calls = 0;
        Mock.When(() => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Do((ReadOnlySpan<int> _) =>
            {
                calls++;
                throw expected;
            });

        Exception actual = Assert.Throws<InvalidOperationException>(
            () => target.Observe([1]));
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];

        Assert.AreSame(expected, actual);
        Assert.AreEqual(1, calls);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>A callback may reenter the same mock because no setup or ledger lock surrounds user code.</summary>
    [TestMethod]
    public void CallbackReentry_CompletesBothCalls()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        var observed = 0;
        Mock.When(target.Ping).Return(4181);
        Mock.When(() => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Do((ReadOnlySpan<int> values) =>
                observed = target.Ping() + values.Length);

        target.Observe([1, 2]);

        Assert.AreEqual(4183, observed);
        AssertConfiguredReturned(target, 2);
    }

    /// <summary>A typed callback does not hold mock-state locks while another thread publishes a setup.</summary>
    [TestMethod]
    public void Callback_AllowsConcurrentSetupPublication()
    {
        TimeSpan coordinationBound =
            TimeSpan.FromMilliseconds(750);
        var target = Mock.Create<ITypedCallbackTarget>();
        Thread? publisher = null;
        Exception? publisherFailure = null;
        var publishedInsideCallback = false;
        Mock.When(() => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Do((ReadOnlySpan<int> _) =>
            {
                publisher = new(() =>
                {
                    try
                    {
                        Mock.When(target.Ping).Return(6765);
                    }
                    catch (Exception exception)
                    {
                        publisherFailure = exception;
                    }
                });
                publisher.Start();
                publishedInsideCallback =
                    publisher.Join(coordinationBound);
            });

        target.Observe([1]);

        Assert.IsNotNull(publisher);
        Assert.IsTrue(
            publisher.Join(coordinationBound),
            "The setup publisher did not finish after callback dispatch.");
        Assert.IsTrue(
            publishedInsideCallback,
            "The setup publisher could not finish during typed user code.");
        Assert.IsNull(publisherFailure);
        Assert.AreEqual(6765, target.Ping());
    }

    /// <summary>Concurrent typed callbacks retain only their own live span frame.</summary>
    [TestMethod]
    public void ConcurrentCallbacks_KeepLiveArgumentsIsolated()
    {
        const int Count = 64;
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.Fill(
                Arg.Any<Span<int>>(0)))
            .Do((Span<int> values) =>
            {
                Thread.Yield();
                values[1] = values[0] * 2;
            });
        var failures = 0;

        Parallel.For(
            1,
            Count + 1,
            value =>
            {
                int[] storage = [value, 0];
                target.Fill(storage);
                if (storage[1] != value * 2)
                    Interlocked.Increment(ref failures);
            });

        Assert.AreEqual(0, failures);
        AssertConfiguredReturned(target, Count);
    }

    /// <summary>Interface and virtual proxy backends execute typed answers without unwanted fallthrough.</summary>
    [TestMethod]
    public void BackendMatrix_ExecutesTypedAnswer()
    {
        AssertClassCallback(
            Mock.Create<ITypedCallbackClassTarget>(),
            null);
        var virtualTarget = Mock.Create<TypedCallbackVirtualTarget>();
        AssertClassCallback(virtualTarget, () => virtualTarget.Calls);
    }

    /// <summary>Proxy-owned generic methods invoke distinct closed standard delegates directly.</summary>
    [TestMethod]
    public void ProxyGenericMethods_InvokePerConstructionTypedAnswers()
    {
        var target = Mock.Create<ITypedCallbackGenericTarget>();
        Mock.When(() => target.Echo(
                Arg.Any<int>()))
            .Answer((int value) => value + 1);
        Mock.When(() => target.Echo(
                Arg.Any<string>()))
            .Answer((string value) => value + "!");
        Mock.When(() => target.Combine(
                Arg.Any<int>(0),
                Arg.Any<int>(1)))
            .Answer((int first, int second) =>
                first + second);
        Mock.When(() => target.Count(
                Arg.Any<Span<int>>(0)))
            .Answer((Span<int> values) =>
            {
                values[0] += 10;
                return values.Length;
            });
        Span<int> values = [2, 3];

        Assert.AreEqual(8, target.Echo(7));
        Assert.AreEqual("ok!", target.Echo("ok"));
        Assert.AreEqual(13, target.Combine(5, 8));
        Assert.AreEqual(2, target.Count(values));
        Assert.AreEqual(12, values[0]);

        MockSetup[] setups =
            Mock.GetMocked(target)!.SnapshotSetups();
        Type[] callbackTypes = [..
            setups.Select(setup =>
                setup.Behavior.Claim().Callback!.GetType())];
        CollectionAssert.Contains(
            callbackTypes,
            typeof(Func<int, int>));
        CollectionAssert.Contains(
            callbackTypes,
            typeof(Func<string, string>));
        CollectionAssert.Contains(
            callbackTypes,
            typeof(Func<int, int, int>));
        CollectionAssert.Contains(
            callbackTypes,
            typeof(Func<Span<int>, int>));
        Assert.AreEqual(4, callbackTypes.Distinct().Count());
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        MockInvocation countInvocation =
            invocations.Single(invocation =>
                    invocation.Identity.Operation.Name ==
                    nameof(ITypedCallbackGenericTarget.Count));
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            countInvocation.Arguments[0].Entry.Kind);
    }

    /// <summary>An explicit ordinary MockCall answer remains selected over typed answer extensions.</summary>
    [TestMethod]
    public void ExplicitMockCallAnswer_UsesOrdinaryCallbackContext()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.Sum(
                Arg.Any<int>(),
                Arg.Any<int>()))
            .Answer(call => call.Argument<int>(0) * call.Argument<int>(1));

        Assert.AreEqual(42, target.Sum(6, 7));
        AssertConfiguredReturned(target, 1);
    }

    /// <summary>A natural delegate beyond the standard Action arity executes directly over every live argument.</summary>
    [TestMethod]
    public void WideNaturalDelegate_ExecutesAllSeventeenArguments()
    {
        var target = Mock.Create<ITypedCallbackWideTarget>();
        var observed = 0;
        Mock.When(() => target.Wide(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int>(), Arg.Any<int>()))
            .Do(
                (
                    int v0, int v1, int v2, int v3, int v4, int v5,
                    int v6, int v7, int v8, int v9, int v10, int v11,
                    int v12, int v13, int v14, int v15, int v16) =>
                {
                    observed =
                        v0 + v1 + v2 + v3 + v4 + v5 +
                        v6 + v7 + v8 + v9 + v10 + v11 +
                        v12 + v13 + v14 + v15 + v16;
                });

        target.Wide(
            1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 11, 12,
            13, 14, 15, 16, 17);

        Assert.AreEqual(153, observed);
        AssertConfiguredReturned(target, 1);
    }

    /// <summary>Typed callbacks run between exact entry and exit projection over the caller-visible slots.</summary>
    [TestMethod]
    public void CallbackProjectors_RecordEntryAndFinalSlots()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        Mock.When(() => target.ExactAnswer(
                Arg.Any<int>(),
                Arg.Any<ReadOnlySpan<int>>(1),
                ref Arg.AnyRef<Span<int>>(2),
                out _))
            .SnapshotArgument(
                1,
                (
                    scoped in ReadOnlySpan<int> values) =>
                    values.ToArray())
            .SnapshotArgument(
                2,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                2,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                3,
                (
                    scoped in TypedRuntimeWindow window) =>
                    window.Values.ToArray())
            .Answer(
                (
                    int offset,
                    scoped in ReadOnlySpan<int> source,
                    scoped ref Span<int> destination,
                    scoped out TypedRuntimeWindow written) =>
                {
                    source.CopyTo(destination);
                    destination = destination[..source.Length];
                    written = new(destination);
                    return offset + source.Length;
                });
        ReadOnlySpan<int> source = [3, 5];
        Span<int> destination = stackalloc int[3];

        Assert.AreEqual(
            10,
            target.ExactAnswer(
                8,
                in source,
                ref destination,
                out _));

        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        CollectionAssert.AreEqual(
            new[] { 3, 5 },
            (int[])invocation.Arguments[1].Entry.Value!);
        CollectionAssert.AreEqual(
            new[] { 0, 0, 0 },
            (int[])invocation.Arguments[2].Entry.Value!);
        CollectionAssert.AreEqual(
            new[] { 3, 5 },
            (int[])invocation.Arguments[2].Exit.Value!);
        CollectionAssert.AreEqual(
            new[] { 3, 5 },
            (int[])invocation.Arguments[3].Exit.Value!);
    }

    /// <summary>An async-void callback is rejected before its setup can enter the immutable generation.</summary>
    [TestMethod]
    public void AsyncVoidCallback_RejectsBeforePublication()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        static async void callback(int _) => await Task.Yield();
        MockSetupClause clause = Mock.When(
            () => target.Ordinary(Arg.Any<int>()));

        MockException error = Assert.Throws<MockException>(
            () => clause.Do((Action<int>)callback));

        StringAssert.Contains(error.Message, "Async-void");
        Assert.AreEqual(
            0,
            Mock.GetMocked(target)!.SnapshotSetups().Length);
    }

    /// <summary>Weak mock ownership releases both a configured typed callback owner and its target.</summary>
    [TestMethod]
    public void CallbackLifetime_ReleasesTargetAndOwner()
    {
        (WeakReference target, WeakReference owner) =
            ConfigureTransientCallback();

        ForceCollection();

        Assert.IsFalse(target.IsAlive);
        Assert.IsFalse(owner.IsAlive);
    }

    private static void AssertClassCallback(
        ITypedCallbackClassTarget target,
        Func<int>? getCalls)
    {
        Mock.When(() => target.Transform(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                values.Length + 50);

        Assert.AreEqual(53, target.Transform([1, 2, 3]));
        if (getCalls is not null)
            Assert.AreEqual(0, getCalls());
    }

    private static void AssertConfiguredReturned(
        object target,
        int count)
    {
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(count, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Kind ==
            MockInvocationCompletionKind.Returned
            && invocation.Completion.Source ==
            MockInvocationExecutionSource.Configured));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Target, WeakReference Owner)
        ConfigureTransientCallback()
    {
        var target = Mock.Create<ITypedCallbackTarget>();
        var owner = new TypedCallbackOwner();
        Mock.When(() => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Do(owner.Observe);

        target.Observe([13, 21]);
        Assert.AreEqual(34, owner.Observed);
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
}

internal interface ITypedCallbackTarget
{
    void Observe(ReadOnlySpan<int> values);

    void Fill(Span<int> values);

    int Calculate(
        int offset,
        ReadOnlySpan<int> source,
        Span<int> destination);

    void Window(TypedRuntimeWindow window);

    void Exact(
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out TypedRuntimeWindow written);

    int ExactAnswer(
        int offset,
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out TypedRuntimeWindow written);

    int Ping();

    void Ordinary(int value);

    int Sum(int first, int second);
}

internal readonly ref struct TypedRuntimeWindow(
    ReadOnlySpan<int> values)
{
    internal ReadOnlySpan<int> Values { get; } = values;
}

internal interface ITypedCallbackClassTarget
{
    int Transform(ReadOnlySpan<int> values);
}

internal interface ITypedCallbackGenericTarget
{
    T Echo<T>(T value);

    T Combine<T>(T first, T second);

    int Count<T>(T value)
        where T : allows ref struct;
}

internal interface ITypedCallbackWideTarget
{
    void Wide(
        int v0, int v1, int v2, int v3, int v4, int v5,
        int v6, int v7, int v8, int v9, int v10, int v11,
        int v12, int v13, int v14, int v15, int v16);
}

internal sealed class TypedCallbackOwner
{
    internal int Observed { get; private set; }

    internal void Observe(ReadOnlySpan<int> values)
    {
        Observed = values[0] + values[1];
    }
}

internal class TypedCallbackVirtualTarget : ITypedCallbackClassTarget
{
    internal int Calls;

    public virtual int Transform(ReadOnlySpan<int> values)
    {
        Calls++;
        return -1;
    }
}

internal sealed class TypedCallbackSealedTarget : ITypedCallbackClassTarget
{
    internal int Calls;

    public int Transform(ReadOnlySpan<int> values)
    {
        Calls++;
        return -1;
    }
}

internal sealed class TypedCallbackPartialTarget
{
    internal int ConfiguredCalls;
    internal int NeighborCalls;

    public int Configured(ReadOnlySpan<int> values)
    {
        _ = values.Length;
        ConfiguredCalls++;
        return -1;
    }

    public int Neighbor(ReadOnlySpan<int> values)
    {
        NeighborCalls++;
        return values.Length + 40;
    }
}
