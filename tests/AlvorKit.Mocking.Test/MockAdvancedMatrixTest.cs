namespace AlvorKit;

[TestClass]
public sealed class MockAdvancedMatrixTest
{
    private static readonly TimeSpan CoordinationBound =
        TimeSpan.FromMilliseconds(750);

    /// <summary>Typed matching, callbacks, answers, and projections compose across every parameter passing shape.</summary>
    [TestMethod]
    public void TypedInputs_AllPassingKindsAndRefStructShapesCompose()
    {
        var target = Mock.Create<IAdvancedTypedTarget>();
        Mock.When(() => target.AnySpan(
                Arg.Any<Span<int>>(0)))
            .Return(11);
        Mock.When(() => target.Predicate(
                Arg.Match<ReadOnlySpan<int>>(
                    0,
                    values =>
                        values.SequenceEqual([2, 3, 5]))))
            .SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) =>
                    values.ToArray())
            .Return(13);
        var observedWindow = 0;
        Mock.When(() => target.Observe(
                Arg.Any<AdvancedWindow>(0)))
            .Do((AdvancedWindow window) =>
                observedWindow =
                    window.Values[0] +
                    window.Values[1]);
        Mock.When(() => target.Transform(
                Arg.Any<int>(),
                Arg.Any<ReadOnlySpan<int>>(1),
                ref Arg.AnyRef<Span<int>>(2),
                out _))
            .SnapshotArgument(
                1,
                (ReadOnlySpan<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                2,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                3,
                (
                    scoped in AdvancedWindow window) =>
                    window.Values.ToArray())
            .Answer(
                (
                    int offset,
                    scoped in ReadOnlySpan<int> source,
                    scoped ref Span<int> destination,
                    scoped out AdvancedWindow written) =>
                {
                    source.CopyTo(destination);
                    destination = destination[..source.Length];
                    written = new(destination);
                    return offset + source.Length;
                });

        Span<int> anyValues = [1, 1];
        Assert.AreEqual(11, target.AnySpan(anyValues));
        int[] predicateValues = [2, 3, 5];
        Assert.AreEqual(
            13,
            target.Predicate(predicateValues));
        predicateValues.AsSpan().Fill(0);
        target.Observe(new([8, 13]));
        ReadOnlySpan<int> source = [21, 34, 55];
        Span<int> destination = stackalloc int[4];

        int transformed = target.Transform(
            10,
            in source,
            ref destination,
            out AdvancedWindow written);

        Assert.AreEqual(13, transformed);
        Assert.AreEqual(21, observedWindow);
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            destination.ToArray());
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            written.Values.ToArray());

        MockInvocation[] invocations =
            [.. Mock.GetMocked(target)!.Invocations
                .Snapshot().Invocations];
        AssertUnavailableEntry(
            InvocationNamed(
                invocations,
                nameof(IAdvancedTypedTarget.AnySpan)),
            0);
        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            (int[])InvocationNamed(
                    invocations,
                    nameof(IAdvancedTypedTarget.Predicate))
                .Arguments[0].Entry.Value!);
        AssertUnavailableEntry(
            InvocationNamed(
                invocations,
                nameof(IAdvancedTypedTarget.Observe)),
            0);

        MockInvocation exact = InvocationNamed(
            invocations,
            nameof(IAdvancedTypedTarget.Transform));
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            (int[])exact.Arguments[1].Entry.Value!);
        AssertUnavailableEntry(exact, 2);
        Assert.AreEqual(
            MockUnavailableReason.OutHasNoEntryValue,
            exact.Arguments[3].Entry.Unavailable!.Reason);
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            (int[])exact.Arguments[2].Exit.Value!);
        CollectionAssert.AreEqual(
            new[] { 21, 34, 55 },
            (int[])exact.Arguments[3].Exit.Value!);
    }

    /// <summary>Ref-struct and managed-reference returns stay live while history retains metadata only.</summary>
    [TestMethod]
    public void BorrowedReturns_RetainNoLiveValueOrInteriorReference()
    {
        var target = Mock.Create<IAdvancedReturnTarget>();
        var owner = new AdvancedReturnOwner(
            [3, 5, 8],
            13);
        Mock.When(target.MutableSpan)
            .ReturnFactory(owner.MutableSpan);
        Mock.WhenRef(target.Mutable)
            .ReturnRef(owner.Mutable);
        Mock.WhenRefReadonly(target.ReadOnly)
            .ReturnRef(owner.ReadOnly);

        Span<int> span = target.MutableSpan();
        ref int mutable = ref target.Mutable();
        ref readonly int readOnly = ref target.ReadOnly();
        span[1] = 34;
        mutable = 55;
        ref int repeated = ref target.Mutable();

        CollectionAssert.AreEqual(
            new[] { 3, 34, 8 },
            owner.Values);
        Assert.AreEqual(55, readOnly);
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref mutable,
                ref repeated));

        ReadOnlySpan<MockInvocation> invocations =
            Mock.GetMocked(target)!.Invocations
                .Snapshot().Invocations;
        Assert.AreEqual(4, invocations.Length);
        foreach (MockInvocation invocation in invocations)
        {
            MockInvocationReturn returned =
                invocation.Completion.Return!;
            Assert.AreEqual(
                MockInvocationReturnKind.Unavailable,
                returned.Kind);
            Assert.AreEqual(
                MockUnavailableReason.BorrowedReturnNotRetained,
                returned.UnavailableReason);
            Assert.IsNull(returned.Value);
        }
    }

    /// <summary>A task answer copies borrowed input before suspension and records its later completion.</summary>
    [TestMethod]
    public async Task AsyncAnswer_CopiesInputBeforeAwait()
    {
        var target = Mock.Create<IAdvancedAsyncTarget>();
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Mock.When(() => target.SumAsync(
                Arg.Any<ReadOnlySpan<byte>>(0)))
            .Answer((ReadOnlySpan<byte> values) =>
                SumAfterRelease(
                    values.ToArray(),
                    release.Task));
        byte[] source = [2, 3, 5, 7];

        Task<int> result = target.SumAsync(source);
        source.AsSpan().Fill(100);
        release.SetResult();

        Assert.AreEqual(17, await result);
        MockInvocation invocation =
            await AwaitAsyncCompletion(target);
        AssertUnavailableEntry(invocation, 0);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationAsyncCompletionKind.Succeeded,
            invocation.AsyncCompletion!.Kind);
    }

    /// <summary>Strict, loose, virtual, and generic proxy shapes share one typed contract.</summary>
    [TestMethod]
    public void Backends_FallbacksShapesAndAutomaticGenericStayConsistent()
    {
        var strict = Mock.Create<IAdvancedShapeTarget>();
        var loose = Mock.CreateLoose<IAdvancedShapeTarget>();

        Assert.Throws<MockException>(
            () => strict.Transform([1]));
        Assert.AreEqual(0, loose.Transform([1]));

        var virtualTarget =
            Mock.Create<AdvancedVirtualTarget>();
        AssertConfiguredShape(
            virtualTarget,
            () => virtualTarget.Calls);
        var generic =
            Mock.Create<IAdvancedGenericTarget>();
        Mock.When(() => generic.Echo(7))
            .Return(11);
        Mock.When(() => generic.Echo("seven"))
            .Return("eleven");
        Assert.AreEqual(11, generic.Echo(7));
        Assert.AreEqual(
            "eleven",
            generic.Echo("seven"));
    }

    /// <summary>Checkpoint windows preserve exact cross-mock logical order without relying on physical time.</summary>
    [TestMethod]
    public void Checkpoints_CrossMockOrderUsesLogicalEntrySequence()
    {
        var left =
            Mock.CreateLoose<IAdvancedOrderTarget>();
        var right =
            Mock.CreateLoose<IAdvancedOrderTarget>();
        using var session = Mock.Session();
        left.Step(-1);
        MockCheckpoint after = session.Checkpoint();
        left.Step(1);
        right.Step(2);
        MockCheckpoint through = session.Checkpoint();
        right.Step(99);

        session.VerifySequence(
            after,
            through,
            () => left.Step(1),
            () => right.Step(2));
        Mock.Verify(() => left.Step(1))
            .Between(after, through)
            .Once();
        Mock.Verify(() => right.Step(2))
            .Between(after, through)
            .Once();
    }

    /// <summary>Concurrent typed callbacks keep live frames isolated while reentering the same mock.</summary>
    [TestMethod]
    public void TypedCalls_ConcurrentReentryKeepsFramesIsolated()
    {
        const int callerCount = 8;
        var target =
            Mock.Create<IAdvancedConcurrentTarget>();
        using var overlap = new Barrier(callerCount);
        Mock.When(() => target.Inner(
                Arg.Any<int>()))
            .Answer((int value) =>
                value + 1000);
        Mock.When(() => target.Outer(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
            {
                int first = values[0];
                int second = values[1];
                Assert.IsTrue(
                    overlap.SignalAndWait(
                        CoordinationBound),
                    "Typed callers failed to overlap.");
                return target.Inner(first) + second;
            });

        var callers = new Task<int>[callerCount];
        for (var index = 0;
             index < callerCount;
             index++)
        {
            int capture = index;
            callers[index] = Task.Factory.StartNew(
                () =>
                {
                    Span<int> values =
                        [capture, capture * 10];
                    return target.Outer(values);
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        Assert.IsTrue(
            Task.WaitAll(callers, CoordinationBound),
            "Concurrent typed callers did not finish.");
        for (var index = 0;
             index < callers.Length;
             index++)
        {
            Assert.AreEqual(
                1000 + (index * 11),
                callers[index].Result);
        }

        ReadOnlySpan<MockInvocation> invocations =
            Mock.GetMocked(target)!.Invocations
                .Snapshot().Invocations;
        Assert.AreEqual(
            callerCount * 2,
            invocations.Length);
        var outerCount = 0;
        var innerCount = 0;
        foreach (MockInvocation invocation in invocations)
        {
            Assert.AreEqual(
                MockInvocationCompletionKind.Returned,
                invocation.Completion.Kind);
            if (invocation.Identity.Operation.Name ==
                nameof(IAdvancedConcurrentTarget.Outer))
            {
                outerCount++;
            }
            else if (invocation.Identity.Operation.Name ==
                nameof(IAdvancedConcurrentTarget.Inner))
            {
                innerCount++;
            }
        }

        Assert.AreEqual(callerCount, outerCount);
        Assert.AreEqual(callerCount, innerCount);
        Mock.Verify(() => target.Outer(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Exactly(callerCount);
        Mock.Verify(() => target.Inner(
                Arg.Any<int>()))
            .Exactly(callerCount);
        Mock.VerifyNoOtherCalls(target);
    }

    private static async Task<int> SumAfterRelease(
        byte[] values,
        Task release)
    {
        await release;
        return values.Sum(
            static value => value);
    }

    private static async Task<MockInvocation>
        AwaitAsyncCompletion(object target)
    {
        for (var attempt = 0;
             attempt < 1000;
             attempt++)
        {
            MockInvocation invocation =
                Mock.GetMocked(target)!.Invocations
                    .Snapshot().Invocations[0];
            if (invocation.AsyncCompletion is not null)
                return invocation;

            await Task.Yield();
        }

        throw new AssertFailedException(
            "The asynchronous completion was not published.");
    }

    private static void AssertConfiguredShape(
        IAdvancedShapeTarget target,
        Func<int> calls)
    {
        Mock.When(() => target.Transform(
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                values.Length + 20);

        Assert.AreEqual(
            23,
            target.Transform([1, 2, 3]));
        Assert.AreEqual(0, calls());
    }

    private static MockInvocation InvocationNamed(
        ReadOnlySpan<MockInvocation> invocations,
        string name)
    {
        foreach (MockInvocation invocation in invocations)
        {
            if (invocation.Identity.Operation.Name == name)
                return invocation;
        }

        throw new AssertFailedException(
            $"No invocation named '{name}' was retained.");
    }

    private static void AssertUnavailableEntry(
        MockInvocation invocation,
        int index)
    {
        MockInvocationArgumentSnapshot entry =
            invocation.Arguments[index].Entry;
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            entry.Kind);
        Assert.AreEqual(
            MockUnavailableReason.ByRefLikeProjectionNotConfigured,
            entry.Unavailable!.Reason);
        Assert.IsNull(entry.Value);
    }
}

internal interface IAdvancedTypedTarget
{
    int AnySpan(Span<int> values);

    int Predicate(ReadOnlySpan<int> values);

    void Observe(AdvancedWindow window);

    int Transform(
        int offset,
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out AdvancedWindow written);
}

internal readonly ref struct AdvancedWindow(
    ReadOnlySpan<int> values)
{
    internal ReadOnlySpan<int> Values { get; } =
        values;
}

internal interface IAdvancedReturnTarget
{
    Span<int> MutableSpan();

    ref int Mutable();

    ref readonly int ReadOnly();
}

internal sealed class AdvancedReturnOwner(
    int[] values,
    int value)
{
    private int value = value;

    internal int[] Values { get; } =
        values;

    internal Span<int> MutableSpan() =>
        Values;

    internal ref int Mutable() =>
        ref value;

    internal ref readonly int ReadOnly() =>
        ref value;
}

internal interface IAdvancedAsyncTarget
{
    Task<int> SumAsync(
        ReadOnlySpan<byte> values);
}

internal interface IAdvancedShapeTarget
{
    int Transform(
        ReadOnlySpan<int> values);
}

internal class AdvancedVirtualTarget :
    IAdvancedShapeTarget
{
    internal int Calls;

    public virtual int Transform(
        ReadOnlySpan<int> values)
    {
        Calls++;
        return -1;
    }
}

internal sealed class AdvancedSealedTarget :
    IAdvancedShapeTarget
{
    internal int Calls;

    public int Transform(
        ReadOnlySpan<int> values)
    {
        Calls++;
        return -1;
    }
}

internal sealed class AdvancedPartialTarget :
    IAdvancedShapeTarget
{
    internal int TransformCalls;
    internal int NeighborCalls;

    public int Transform(
        ReadOnlySpan<int> values)
    {
        TransformCalls++;
        return -1;
    }

    internal int Neighbor(
        ReadOnlySpan<int> values)
    {
        NeighborCalls++;
        return values.Length + 40;
    }
}

internal interface IAdvancedGenericTarget
{
    T Echo<T>(T value);
}

internal interface IAdvancedOrderTarget
{
    void Step(int value);
}

internal interface IAdvancedConcurrentTarget
{
    int Outer(
        ReadOnlySpan<int> values);

    int Inner(int value);
}
