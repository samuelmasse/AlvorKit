namespace AlvorKit;

/// <summary>
/// Audits exact typed data-plane behavior through a receiver-free interception
/// runtime binding.
/// </summary>
[TestClass]
public sealed class MockReceiverFreeTypedRuntimeAuditTest
{
    private static int nextOffset;

    /// <summary>A live ref-struct predicate selects only its matching static invocation.</summary>
    [TestMethod]
    public void Bind_LiveTypedPredicate_DistinguishesValues()
    {
        ReceiverFreeTypedSpanCall call = BindSpan();
        var predicateCalls = 0;
        static void callback(scoped ref Span<int> values)
        {
            values[0] *= 10;
            values = values[..2];
        }

        using MockSession session = Mock.Session();
        Mock.When(
                () => call(
                    ref Arg.Match<Span<int>>(
                        0,
                        (scoped in values) =>
                        {
                            predicateCalls++;
                            return values.Length > 0 &&
                                values[0] == 5;
                        })))
            .Answer(
                new ReceiverFreeTypedSpanCall(
                    (scoped ref values) =>
                    {
                        callback(ref values);
                        return values[0] + values[1];
                    }));

        Span<int> matching = [5, 8, 13];
        Span<int> nonmatching = [7, 11, 17];
        int matchingResult = call(ref matching);
        int nonmatchingResult = call(ref nonmatching);

        Assert.AreEqual(
            2,
            predicateCalls,
            "The receiver-free typed prefix did not evaluate the live matcher.");
        Assert.AreEqual(58, matchingResult);
        Assert.AreEqual(2, matching.Length);
        Assert.AreEqual(50, matching[0]);
        Assert.AreEqual(-35, nonmatchingResult);
        Assert.AreEqual(2, nonmatching.Length);
        Assert.AreEqual(11, nonmatching[0]);
    }

    /// <summary>Exact callback and borrowed factory execute and verify without object carriers.</summary>
    [TestMethod]
    public void Bind_ExactCallbackAndFactory_ExecuteAndVerify()
    {
        ReceiverFreeTypedSpanCall transform = BindSpan();
        ReceiverFreeTypedViewCall view = BindView();
        var callbackCalls = 0;
        var factoryCalls = 0;

        using MockSession session = Mock.Session();
        Mock.When(
                () => transform(
                    ref Arg.AnyRef<Span<int>>(0)))
            .Answer(
                new ReceiverFreeTypedSpanCall(
                    (scoped ref values) =>
                    {
                        callbackCalls++;
                        values[0] += 40;
                        return values[0];
                    }));
        Mock.When(() => view(3))
            .ReturnFactory(
                () =>
                {
                    factoryCalls++;
                    return ReceiverFreeTypedTarget.FactoryView();
                });

        Span<int> values = [2, 3];
        int transformed = transform(ref values);
        ReadOnlySpan<int> returned = view(3);

        Assert.AreEqual(42, transformed);
        Assert.AreEqual(42, values[0]);
        Assert.AreEqual(1, callbackCalls);
        Assert.AreEqual(1, factoryCalls);
        Assert.IsTrue(returned.SequenceEqual([13, 21, 34]));
        Mock.Verify(
                () => transform(
                    ref Arg.AnyRef<Span<int>>(0)))
            .Once();
        Mock.Verify(() => view(3)).Once();
    }

    /// <summary>Entry and exit projectors retain only heap-safe copies around an exact callback.</summary>
    [TestMethod]
    public void Bind_EntryAndExitProjectors_RecordHeapSafeCopies()
    {
        ReceiverFreeTypedSpanCall call = BindSpan();

        using MockSession session = Mock.Session();
        Mock.When(
                () => call(
                    ref Arg.AnyRef<Span<int>>(0)))
            .SnapshotArgument(
                0,
                (scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                0,
                (scoped in Span<int> values) =>
                    values.ToArray())
            .Answer(
                new ReceiverFreeTypedSpanCall(
                    static (
                        scoped ref values) =>
                    {
                        values[0] = 20;
                        values = values[..2];
                        return values[0] + values[1];
                    }));
        Span<int> values = [2, 3, 5];

        Assert.AreEqual(23, call(ref values));

        MockInvocation invocation =
            session.SnapshotThrough(
                session.Checkpoint()).Single();
        MockInvocationArgument argument =
            invocation.Arguments[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            argument.Entry.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            argument.Exit.Kind);
        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            (int[])argument.Entry.Value!);
        CollectionAssert.AreEqual(
            new[] { 20, 3 },
            (int[])argument.Exit.Value!);
    }

    /// <summary>Passthrough projects the live value before and after the original call.</summary>
    [TestMethod]
    public void Bind_PassthroughProjectors_RecordOriginalExit()
    {
        ReceiverFreeTypedSpanCall call = BindSpan();

        using MockSession session = Mock.Session();
        Mock.When(
                () => call(
                    ref Arg.AnyRef<Span<int>>(0)))
            .SnapshotArgument(
                0,
                (scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                0,
                (scoped in Span<int> values) =>
                    values.ToArray())
            .Passthrough();
        Span<int> values = [2, 3, 5];

        Assert.AreEqual(-10, call(ref values));

        MockInvocationArgument argument =
            session.SnapshotThrough(
                    session.Checkpoint())
                .Single()
                .Arguments[0];
        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            (int[])argument.Entry.Value!);
        CollectionAssert.AreEqual(
            new[] { 3, 5 },
            (int[])argument.Exit.Value!);
    }

    /// <summary>Unprojected arguments and borrowed results retain metadata rather than boxes.</summary>
    [TestMethod]
    public void Bind_History_DoesNotBoxOrRetainBorrowedValues()
    {
        ReceiverFreeTypedSpanCall transform = BindSpan();
        ReceiverFreeTypedViewCall view = BindView();

        using MockSession session = Mock.Session();
        Mock.When(() => view(5))
            .ReturnFactory(
                ReceiverFreeTypedTarget.FactoryView);
        Span<int> values = [1, 2, 3];

        Assert.AreEqual(-6, transform(ref values));
        ReadOnlySpan<int> returned = view(5);
        Assert.IsTrue(returned.SequenceEqual([13, 21, 34]));

        MockInvocation[] history =
            session.SnapshotThrough(
                session.Checkpoint());
        MockInvocation transformInvocation =
            history.Single(invocation =>
                invocation.Identity.Operation.Name ==
                nameof(ReceiverFreeTypedTarget.Transform));
        MockInvocationArgument argument =
            transformInvocation.Arguments[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            argument.Entry.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            argument.Exit.Kind);
        Assert.IsNull(argument.Entry.Value);
        Assert.IsNull(argument.Exit.Value);
        Assert.AreEqual(
            MockUnavailableReason.ByRefLikeProjectionNotConfigured,
            argument.Entry.Unavailable!.Reason);

        MockInvocation viewInvocation =
            history.Single(invocation =>
                invocation.Identity.Operation.Name ==
                nameof(ReceiverFreeTypedTarget.View));
        Assert.AreEqual(
            MockInvocationReturnKind.Unavailable,
            viewInvocation.Completion.Return!.Kind);
        Assert.IsNull(
            viewInvocation.Completion.Return.Value);
        Assert.AreEqual(
            MockUnavailableReason.BorrowedReturnNotRetained,
            viewInvocation.Completion.Return.UnavailableReason);
    }

    private static ReceiverFreeTypedSpanCall BindSpan()
    {
        MethodInfo method = typeof(ReceiverFreeTypedTarget)
            .GetMethod(
                nameof(ReceiverFreeTypedTarget.Transform),
                BindingFlags.Static |
                BindingFlags.NonPublic)!;
        return MockInterceptionOperationRuntime.Bind(
            Site(),
            method,
            new ReceiverFreeTypedSpanCall(
                ReceiverFreeTypedTarget.Transform));
    }

    private static ReceiverFreeTypedViewCall BindView()
    {
        MethodInfo method = typeof(ReceiverFreeTypedTarget)
            .GetMethod(
                nameof(ReceiverFreeTypedTarget.View),
                BindingFlags.Static |
                BindingFlags.NonPublic)!;
        return MockInterceptionOperationRuntime.Bind(
            Site(),
            method,
            new ReceiverFreeTypedViewCall(
                ReceiverFreeTypedTarget.View));
    }

    private static MockInterceptionSiteDescriptor Site() =>
        new(
            typeof(MockReceiverFreeTypedRuntimeAuditTest)
                .Module.ModuleVersionId,
            typeof(MockReceiverFreeTypedRuntimeAuditTest)
                .MetadataToken,
            Interlocked.Increment(ref nextOffset),
            MockInvocationOperationKind.StaticMethod);
}

internal delegate int ReceiverFreeTypedSpanCall(
    scoped ref Span<int> values);

internal delegate void ReceiverFreeTypedSpanMutation(
    scoped ref Span<int> values);

internal delegate ReadOnlySpan<int> ReceiverFreeTypedViewCall(
    int key);

internal static class ReceiverFreeTypedTarget
{
    private static readonly int[] FactoryValues = [13, 21, 34];
    private static readonly int[] OriginalValues = [-1];

    internal static int Transform(
        scoped ref Span<int> values)
    {
        var sum = 0;
        foreach (int value in values)
            sum += value;
        values = values[1..];
        return -sum;
    }

    internal static ReadOnlySpan<int> View(int key)
    {
        if (key == 0)
            return FactoryValues.AsSpan(0, 0);
        return OriginalValues;
    }

    internal static ReadOnlySpan<int> FactoryView() =>
        FactoryValues;
}
