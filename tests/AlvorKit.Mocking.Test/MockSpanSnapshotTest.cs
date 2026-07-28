namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockSpanSnapshotTest
{
    /// <summary>An entry projection is stable after its source mutates and leaves scope.</summary>
    [TestMethod]
    public void SpanEntrySnapshot_RemainsStableAfterMutation()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Any<ReadOnlySpan<int>>(0)))
            .SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) => values.ToArray())
            .Return(11);

        int[] source = [2, 3, 5];
        Assert.AreEqual(11, target.Observe(source));
        source.AsSpan().Fill(99);

        MockInvocationArgumentSnapshot entry =
            Single(target).Arguments[0].Entry;
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            entry.Kind);
        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            (int[])entry.Value!);
    }

    /// <summary>History retains only the projected array after source storage leaves scope.</summary>
    [TestMethod]
    public void SpanEntrySnapshot_DoesNotRetainSourceStorage()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Any<ReadOnlySpan<int>>(0)))
            .SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) => values.ToArray())
            .Return(7);

        WeakReference source = InvokeWithTransientSource(target);
        ForceCollection();

        Assert.IsFalse(source.IsAlive);
        CollectionAssert.AreEqual(
            new[] { 89, 97 },
            (int[])Single(target).Arguments[0].Entry.Value!);
    }

    /// <summary>An arbitrary readonly ref struct projects only its selected heap-safe meaning.</summary>
    [TestMethod]
    public void ArbitraryRefStruct_ProjectsHeapSafeValue()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Borrow(
                    Arg.Any<BorrowedMatcherWindow>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in BorrowedMatcherWindow window) =>
                    window.Values.ToArray())
            .Return(13);

        var window = new BorrowedMatcherWindow([7, 11]);
        Assert.AreEqual(13, target.Borrow(window));

        MockInvocationArgumentSnapshot entry =
            Single(target).Arguments[0].Entry;
        Assert.AreEqual(
            typeof(BorrowedMatcherWindow),
            entry.DeclaredType);
        CollectionAssert.AreEqual(
            new[] { 7, 11 },
            (int[])entry.Value!);
    }

    /// <summary>A ref span publishes distinct entry and final-slot projections.</summary>
    [TestMethod]
    public void RefSpan_ProjectsEntryAndExit()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Mutate(
                    ref Arg.AnyRef<Span<int>>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .Return(17);

        Span<int> values = [17, 19];
        Assert.AreEqual(17, target.Mutate(ref values));
        values.Fill(99);

        MockInvocationArgument argument =
            Single(target).Arguments[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            argument.Entry.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            argument.Exit.Kind);
        CollectionAssert.AreEqual(
            new[] { 17, 19 },
            (int[])argument.Entry.Value!);
        CollectionAssert.AreEqual(
            new[] { 17, 19 },
            (int[])argument.Exit.Value!);
    }

    /// <summary>An out span is read only after configured default initialization.</summary>
    [TestMethod]
    public void OutSpan_ProjectsInitializedExit()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(() => target.Produce(out _))
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .Return(23);

        Assert.AreEqual(
            23,
            target.Produce(out Span<int> produced));
        Assert.IsTrue(produced.IsEmpty);

        MockInvocationArgument argument =
            Single(target).Arguments[0];
        Assert.AreEqual(
            MockUnavailableReason.OutHasNoEntryValue,
            argument.Entry.Unavailable!.Reason);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            argument.Exit.Kind);
        CollectionAssert.AreEqual(
            Array.Empty<int>(),
            (int[])argument.Exit.Value!);
    }

    /// <summary>Entry projection precedes behavior and exit projection follows ordinary writeback.</summary>
    [TestMethod]
    public void RefProjection_BracketsBehaviorAndWriteback()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var projectedEntry = false;
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgument(
                0,
                (
                    scoped in int value) =>
                    {
                        projectedEntry = true;
                        return value;
                    })
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
                    Assert.IsTrue(projectedEntry);
                    call.SetReference(0, 29);
                    call.SetReference(1, 31);
                    return 37;
                });

        var reference = 5;
        Assert.AreEqual(
            37,
            target.Exchange(
                ref reference,
                out int output));
        Assert.AreEqual(29, reference);
        Assert.AreEqual(31, output);

        ReadOnlySpan<MockInvocationArgument> arguments =
            Single(target).Arguments;
        Assert.AreEqual(5, arguments[0].Entry.Value);
        Assert.AreEqual(29, arguments[0].Exit.Value);
        Assert.AreEqual(31, arguments[1].Exit.Value);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[0].Entry.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[0].Exit.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[1].Exit.Kind);
    }

    /// <summary>Automatic ref capture remains shallow where no exit projector was selected.</summary>
    [TestMethod]
    public void RefProjection_PreservesUnprojectedShallowExit()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        Mock.When(
                () => target.Mix(
                    ref Arg.AnyRef<int>(0),
                    ref Arg.AnyRef<int>(1)))
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    value)
            .Answer(
                call =>
                {
                    call.SetReference(0, 101);
                    call.SetReference(1, 103);
                    return 107;
                });

        var projected = 2;
        var shallow = 3;
        Assert.AreEqual(
            107,
            target.Mix(
                ref projected,
                ref shallow));

        ReadOnlySpan<MockInvocationArgument> arguments =
            Single(target).Arguments;
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[0].Exit.Kind);
        Assert.AreEqual(101, arguments[0].Exit.Value);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Shallow,
            arguments[1].Exit.Kind);
        Assert.AreEqual(103, arguments[1].Exit.Value);
    }

    /// <summary>Typed return factories project ref and out values after writeback.</summary>
    [TestMethod]
    public void ReturnFactory_ProjectsExitAfterWriteback()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
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
            .ReturnFactory(() => 109);

        var reference = 113;
        Assert.AreEqual(
            109,
            target.Exchange(
                ref reference,
                out int output));

        ReadOnlySpan<MockInvocationArgument> arguments =
            Single(target).Arguments;
        Assert.AreEqual(113, reference);
        Assert.AreEqual(0, output);
        Assert.AreEqual(113, arguments[0].Exit.Value);
        Assert.AreEqual(0, arguments[1].Exit.Value);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[0].Exit.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            arguments[1].Exit.Kind);
    }

    /// <summary>Managed-reference return publication retains its projector metadata.</summary>
    [TestMethod]
    public void ManagedRefReturn_ProjectsBeforeAliasCompletion()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        Mock.When(
                () => target.Select(
                    ref Arg.AnyRef<int>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in int value) =>
                    value)
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    value)
            .Return(127);

        var argument = 131;
        ref int returned = ref target.Select(ref argument);

        Assert.AreEqual(127, returned);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            invocation.Arguments[0].Entry.Kind);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            invocation.Arguments[0].Exit.Kind);
        Assert.AreEqual(131, invocation.Arguments[0].Entry.Value);
        Assert.AreEqual(131, invocation.Arguments[0].Exit.Value);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
    }

    /// <summary>An entry projector may re-enter the same mock without setup-store locking.</summary>
    [TestMethod]
    public void EntryProjector_ReentersSameMock()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(target.Ping).Return(41);
        Mock.When(
                () => target.Observe(
                    Arg.Any<ReadOnlySpan<int>>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in ReadOnlySpan<int> values) =>
                    {
                        Assert.AreEqual(41, target.Ping());
                        return values.ToArray();
                    })
            .Return(43);

        Assert.AreEqual(43, target.Observe([47]));

        ReadOnlySpan<MockInvocation> invocations =
            Snapshot(target).Invocations;
        Assert.AreEqual(2, invocations.Length);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            invocations[0].Arguments[0].Entry.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocations[1].Completion.Source);
    }

    /// <summary>An exit projector may re-enter after writeback without completion locking.</summary>
    [TestMethod]
    public void ExitProjector_ReentersSameMock()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        Mock.When(target.Ping).Return(137);
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    {
                        Assert.AreEqual(137, target.Ping());
                        return value;
                    })
            .Answer(
                call =>
                {
                    call.SetReference(0, 139);
                    return 149;
                });

        var reference = 151;
        Assert.AreEqual(
            149,
            target.Exchange(
                ref reference,
                out _));

        ReadOnlySpan<MockInvocation> invocations =
            Snapshot(target).Invocations;
        Assert.AreEqual(2, invocations.Length);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Projected,
            invocations[0].Arguments[0].Exit.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocations[1].Completion.Source);
    }

    /// <summary>An entry projector exception propagates unchanged and completes one entry-stage failure.</summary>
    [TestMethod]
    public void EntryProjectorThrow_CompletesExactFailure()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        var expected = new InvalidOperationException("entry projector");
        Mock.When(
                () => target.Observe(
                    Arg.Any<ReadOnlySpan<int>>(0)))
            .SnapshotArgument<ReadOnlySpan<int>, int[]>(
                0,
                (
                    scoped in _) =>
                    throw expected)
            .Return(53);

        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Observe([1]));

        Assert.AreSame(expected, actual);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.EntryProjector,
            invocation.Completion.FailureStage);
        Assert.AreSame(
            expected,
            invocation.Completion.Exception);
        Assert.AreEqual(
            MockUnavailableReason.NoNormalCompletion,
            invocation.Arguments[0].Exit.Unavailable!.Reason);
    }

    /// <summary>An exit projector exception supersedes return after caller-visible writeback.</summary>
    [TestMethod]
    public void ExitProjectorThrow_PreservesWritebackAndCompletesExactFailure()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var expected = new InvalidOperationException("exit projector");
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgumentOnExit<int, int>(
                0,
                (
                    scoped in _) =>
                    throw expected)
            .Answer(
                call =>
                {
                    call.SetReference(0, 59);
                    call.SetReference(1, 61);
                    return 67;
                });

        var reference = 5;
        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Exchange(
                ref reference,
                out _));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(59, reference);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.ExitProjector,
            invocation.Completion.FailureStage);
        Assert.AreSame(
            expected,
            invocation.Completion.Exception);
        Assert.AreEqual(
            MockUnavailableReason.NoNormalCompletion,
            invocation.Arguments[0].Exit.Unavailable!.Reason);
    }

    /// <summary>A factory exit failure is not relabeled by its cleared continuation.</summary>
    [TestMethod]
    public void ReturnFactoryExitProjectorThrow_CompletesExitStage()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var expected = new InvalidOperationException("factory exit");
        var observedExit = -1;
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgumentOnExit<int, int>(
                1,
                (
                    scoped in value) =>
                    {
                        observedExit = value;
                        throw expected;
                    })
            .ReturnFactory(() => 163);

        var reference = 167;
        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Exchange(
                ref reference,
                out _));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(0, observedExit);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.ExitProjector,
            invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>A managed-ref exit failure clears finalizer state before propagating.</summary>
    [TestMethod]
    public void ManagedRefExitProjectorThrow_CompletesExitStageOnce()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var expected = new InvalidOperationException("managed ref exit");
        Mock.When(
                () => target.Select(
                    ref Arg.AnyRef<int>(0)))
            .SnapshotArgumentOnExit<int, int>(
                0,
                (
                    scoped in _) =>
                    throw expected)
            .Return(173);

        var argument = 179;
        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Select(ref argument));

        Assert.AreSame(expected, actual);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.ExitProjector,
            invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>Exit projectors never run after the selected behavior throws.</summary>
    [TestMethod]
    public void BehaviorThrow_SkipsExitProjector()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var expected = new InvalidOperationException("behavior");
        var exitCalls = 0;
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    {
                        exitCalls++;
                        return value;
                    })
            .Throw(expected);

        var reference = 5;
        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Exchange(
                ref reference,
                out _));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(0, exitCalls);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            invocation.Completion.FailureStage);
        Assert.AreEqual(
            MockUnavailableReason.NoNormalCompletion,
            invocation.Arguments[0].Exit.Unavailable!.Reason);
    }

    /// <summary>A typed return-factory failure skips exit projection and keeps its exact stage.</summary>
    [TestMethod]
    public void ReturnFactoryThrow_SkipsExitProjector()
    {
        var target = Mock.CreateLoose<IProjectionTarget>();
        var expected = new InvalidOperationException("return factory");
        var exitCalls = 0;
        Mock.When(
                () => target.Exchange(
                    ref Arg.AnyRef<int>(0),
                    out _))
            .SnapshotArgumentOnExit(
                0,
                (
                    scoped in int value) =>
                    {
                        exitCalls++;
                        return value;
                    })
            .ReturnFactory(() => throw expected);

        var reference = 157;
        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Exchange(
                ref reference,
                out _));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(0, exitCalls);
        MockInvocation invocation = Single(target);
        Assert.AreEqual(
            MockInvocationFailureStage.ReturnFactory,
            invocation.Completion.FailureStage);
        Assert.AreSame(
            expected,
            invocation.Completion.Exception);
        Assert.AreEqual(
            MockUnavailableReason.NoNormalCompletion,
            invocation.Arguments[0].Exit.Unavailable!.Reason);
    }

    /// <summary>A ref-struct argument without a projector remains explicitly unavailable.</summary>
    [TestMethod]
    public void NoProjector_RetainsUnavailableDescriptor()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();

        Assert.AreEqual(0, target.Observe([71, 73]));

        MockInvocationArgumentSnapshot entry =
            Single(target).Arguments[0].Entry;
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            entry.Kind);
        Assert.AreEqual(
            MockUnavailableReason.ByRefLikeProjectionNotConfigured,
            entry.Unavailable!.Reason);
    }

    /// <summary>Exact span verification compares the stable projected representation.</summary>
    [TestMethod]
    public void Verification_ConsumesProjectedSpanSnapshot()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Any<ReadOnlySpan<int>>(0)))
            .SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) => values.ToArray())
            .Return(79);
        int[] source = [79, 83];

        Assert.AreEqual(79, target.Observe(source));
        source.AsSpan().Fill(0);

        MockVerification mismatch = Mock.Verify(
            () => target.Observe(
                Arg.ReadOnlySpanEqual<int>(
                    0,
                    [79, 89])));
        Assert.Throws<MockException>(mismatch.Once);
        Assert.IsFalse(Single(target).IsVerified);

        Mock.Verify(
                () => target.Observe(
                    Arg.ReadOnlySpanEqual<int>(
                        0,
                        [79, 83])))
            .Once();
        Mock.VerifyNoOtherCalls(target);
    }

    private static MockInvocation Single(object target)
    {
        ReadOnlySpan<MockInvocation> invocations =
            Snapshot(target).Invocations;
        Assert.AreEqual(1, invocations.Length);
        return invocations[0];
    }

    private static MockInvocationLedgerSnapshot Snapshot(
        object target) =>
        Mock.GetMocked(target)!.Invocations.Snapshot();

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference InvokeWithTransientSource(
        IRefStructMatcherTarget target)
    {
        int[] source = [89, 97];
        var reference = new WeakReference(source);
        Assert.AreEqual(7, target.Observe(source));
        return reference;
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

internal interface IProjectionTarget
{
    int Exchange(ref int value, out int output);

    int Mix(ref int projected, ref int shallow);

    ref int Select(ref int value);

    int Ping();
}
