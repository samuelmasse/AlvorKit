using System.Runtime.CompilerServices;

namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockRefStructTest
{
    /// <summary>Live predicates discriminate read-only and mutable spans without a carrier value.</summary>
    [TestMethod]
    public void SpanPredicates_DiscriminateLiveValues()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        values => values.SequenceEqual([2, 3, 5]))))
            .Return(11);
        Mock.When(
                () => target.Transform(
                    Arg.Match<Span<int>>(
                        0,
                        values => values.Length == 2 && values[0] == 7)))
            .Return(13);

        Assert.AreEqual(11, target.Observe([2, 3, 5]));
        Assert.AreEqual(0, target.Observe([2, 3, 6]));
        Assert.AreEqual(13, target.Transform([7, 8]));
        Assert.AreEqual(0, target.Transform([8, 7]));
    }

    /// <summary>Copied span conveniences retain setup-time contents exactly once.</summary>
    [TestMethod]
    public void SpanEqual_CopiesExpectedContentsOnce()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        int[] expectedReadOnly = [2, 3, 5];
        int[] expectedMutable = [7, 11];
        Mock.When(
                () => target.Observe(
                    Arg.ReadOnlySpanEqual<int>(0, expectedReadOnly)))
            .Return(17);
        Mock.When(
                () => target.Transform(
                    Arg.SpanEqual<int>(0, expectedMutable)))
            .Return(19);

        expectedReadOnly[0] = 99;
        expectedMutable[0] = 99;

        Assert.AreEqual(17, target.Observe([2, 3, 5]));
        Assert.AreEqual(0, target.Observe(expectedReadOnly));
        Assert.AreEqual(19, target.Transform([7, 11]));
        Assert.AreEqual(0, target.Transform(expectedMutable));
    }

    /// <summary>Read-only and mutable managed references match their live entry values.</summary>
    [TestMethod]
    public void InAndRefPredicates_MatchEntryValues()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Inspect(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        values => values.Length == 3 && values[2] == 13)))
            .Return(23);
        Mock.When(
                () => target.Mutate(
                    ref Arg.Match<Span<int>>(
                        0,
                        (
                            scoped in values) =>
                            values.Length == 2 && values[0] == 29)))
            .Return(31);

        ReadOnlySpan<int> input = [3, 8, 13];
        Span<int> mutable = [29, 31];
        Assert.AreEqual(23, target.Inspect(in input));
        Assert.AreEqual(31, target.Mutate(ref mutable));

        input = [3, 8, 14];
        mutable = [30, 31];
        Assert.AreEqual(0, target.Inspect(in input));
        Assert.AreEqual(0, target.Mutate(ref mutable));
    }

    /// <summary>Arbitrary readonly and mutable ref structs use the same declared-index path.</summary>
    [TestMethod]
    public void ArbitraryRefStructs_MatchLiveValues()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Borrow(
                    Arg.Match<BorrowedMatcherWindow>(
                        0,
                        window =>
                            window.Values.Length == 2 &&
                            window.Values[0] == 37)))
            .Return(41);
        Mock.When(
                () => target.Change(
                    ref Arg.Match<MutableMatcherWindow>(
                        0,
                        (
                            scoped in window) =>
                            window.Values.Length == 2 &&
                            window.Values[1] == 43)))
            .Return(47);

        var borrowed = new BorrowedMatcherWindow([37, 41]);
        var mutable = new MutableMatcherWindow([41, 43]);
        Assert.AreEqual(41, target.Borrow(borrowed));
        Assert.AreEqual(47, target.Change(ref mutable));

        borrowed = new BorrowedMatcherWindow([38, 41]);
        mutable = new MutableMatcherWindow([41, 44]);
        Assert.AreEqual(0, target.Borrow(borrowed));
        Assert.AreEqual(0, target.Change(ref mutable));
    }

    /// <summary>Any matchers cover live by-value and mutable-reference shapes without reading placeholders.</summary>
    [TestMethod]
    public void AnyMatchers_AcceptByValueAndRefInputs()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Borrow(
                    Arg.Any<BorrowedMatcherWindow>(0)))
            .Return(53);
        Mock.When(
                () => target.Mutate(
                    ref Arg.AnyRef<Span<int>>(0)))
            .Return(59);

        var borrowed = new BorrowedMatcherWindow([1]);
        Span<int> mutable = [2];
        Assert.AreEqual(53, target.Borrow(borrowed));
        Assert.AreEqual(59, target.Mutate(ref mutable));
    }

    /// <summary>An out ref struct has no input matcher and is initialized by configured dispatch.</summary>
    [TestMethod]
    public void OutRefStruct_HasNoInputMatcher()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(() => target.Produce(out _)).Return(61);

        Assert.AreEqual(61, target.Produce(out Span<int> produced));
        Assert.IsTrue(produced.IsEmpty);

        var error = Assert.Throws<MockException>(
            () => Mock.When(
                () => target.Produce(
                    out Arg.AnyRef<Span<int>>(0))));
        StringAssert.Contains(error.Message, "has no entry value");
    }

    /// <summary>Ambiguous exact ref-struct captures require an explicit declared-index matcher.</summary>
    [TestMethod]
    public void UnindexedRefStructCapture_IsRejected()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();

        var error = Assert.Throws<MockException>(
            () => Mock.When(
                () => target.Observe([1, 2, 3])));

        StringAssert.Contains(error.Message, "parameter 0");
        StringAssert.Contains(error.Message, "declared-index matcher");
        Assert.IsFalse(Capture.Context.IsActive);
    }

    /// <summary>A live predicate can re-enter the same mock because no setup-state lock is held.</summary>
    [TestMethod]
    public void Predicate_ReentrantMockCallCompletes()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(target.Ping).Return(67);
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        values =>
                            values.Length == 1 &&
                            target.Ping() == 67)))
            .Return(71);

        Assert.AreEqual(71, target.Observe([1]));
        Assert.AreEqual(
            2,
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
    }

    /// <summary>A predicate exception escapes unchanged and completes one ledger entry at matcher stage.</summary>
    [TestMethod]
    public void PredicateException_CompletesOneMatcherFailure()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        var expected = new InvalidOperationException("typed predicate");
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        _ => throw expected)))
            .Return(73);

        var actual = Assert.Throws<InvalidOperationException>(
            () => target.Observe([1]));

        Assert.AreSame(expected, actual);
        ReadOnlySpan<MockInvocation> invocations =
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations;
        Assert.AreEqual(1, invocations.Length);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocations[0].Completion.Kind);
        Assert.AreEqual(
            MockInvocationFailureStage.Matcher,
            invocations[0].Completion.FailureStage);
        Assert.AreSame(
            expected,
            invocations[0].Completion.Exception);
    }

    /// <summary>Diagnostics describe a predicate without invoking it again.</summary>
    [TestMethod]
    public void StrictFormatting_DoesNotRerunPredicate()
    {
        var target = Mock.Create<IRefStructMatcherTarget>();
        var predicateCalls = 0;
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        _ =>
                        {
                            predicateCalls++;
                            return false;
                        })))
            .Return(79);

        Assert.Throws<MockException>(
            () => target.Observe([1]));
        Assert.AreEqual(1, predicateCalls);
    }

    /// <summary>Heap and live predicates run once within one invocation selection.</summary>
    [TestMethod]
    public void MixedPredicates_RunOnceAndRecordOnce()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        var ordinaryCalls = 0;
        var liveCalls = 0;
        Mock.When(
                () => target.Mixed(
                    Arg.Match<int>(
                        value =>
                        {
                            ordinaryCalls++;
                            return value == 83;
                        }),
                    Arg.Match<ReadOnlySpan<int>>(
                        1,
                        values =>
                        {
                            liveCalls++;
                            return values.Length == 1;
                        })))
            .Return(89);

        Assert.AreEqual(89, target.Mixed(83, [1]));
        Assert.AreEqual(1, ordinaryCalls);
        Assert.AreEqual(1, liveCalls);
        Assert.AreEqual(
            1,
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
    }

    /// <summary>Invocation history never retains a live span's backing array.</summary>
    [TestMethod]
    public void Invocation_DoesNotRetainLiveSpanStorage()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        values => values.Length == 3)))
            .Return(97);

        WeakReference storage = InvokeWithTransientStorage(target);
        ForceCollection();

        Assert.IsFalse(storage.IsAlive);
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            invocation.Arguments[0].Entry.Kind);
    }

    /// <summary>Typed matcher caches retain neither the mock nor its captured predicate owner.</summary>
    [TestMethod]
    public void MatcherPath_DoesNotRootMockOrPredicateOwner()
    {
        (WeakReference mock, WeakReference owner) =
            ConfigureCollectibleMatcher();

        ForceCollection();

        Assert.IsFalse(mock.IsAlive);
        Assert.IsFalse(owner.IsAlive);
    }

    /// <summary>Concurrent live values remain isolated across typed matcher evaluations.</summary>
    [TestMethod]
    public void ConcurrentPredicates_DoNotCrossLiveValues()
    {
        const int count = 128;
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        values =>
                            values.Length == 1 &&
                            (values[0] & 1) == 0)))
            .Return(101);

        Parallel.For(
            0,
            count,
            index =>
            {
                int actual = target.Observe([index]);
                Assert.AreEqual(
                    (index & 1) == 0 ? 101 : 0,
                    actual);
            });

        Assert.AreEqual(
            count,
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference InvokeWithTransientStorage(
        IRefStructMatcherTarget target)
    {
        int[] storage = [1, 2, 3];
        var reference = new WeakReference(storage);
        Assert.AreEqual(97, target.Observe(storage));
        return reference;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (
        WeakReference Mock,
        WeakReference Owner) ConfigureCollectibleMatcher()
    {
        var owner = new MatcherPredicateOwner(103);
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        Mock.When(
                () => target.Observe(
                    Arg.Match<ReadOnlySpan<int>>(
                        0,
                        owner.Accepts)))
            .Return(107);
        Assert.AreEqual(107, target.Observe([103]));

        return (
            new WeakReference(target),
            new WeakReference(owner));
    }

    private static void ForceCollection()
    {
        for (var index = 0; index < 3; index++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}

internal interface IRefStructMatcherTarget
{
    int Observe(ReadOnlySpan<int> values);

    int Transform(Span<int> values);

    int Inspect(scoped in ReadOnlySpan<int> values);

    int Mutate(scoped ref Span<int> values);

    int Produce(scoped out Span<int> values);

    int Borrow(BorrowedMatcherWindow window);

    int Change(scoped ref MutableMatcherWindow window);

    int Mixed(int key, ReadOnlySpan<int> values);

    int Ping();
}

internal readonly ref struct BorrowedMatcherWindow(
    ReadOnlySpan<int> values)
{
    internal ReadOnlySpan<int> Values { get; } = values;
}

internal readonly ref struct MutableMatcherWindow(
    Span<int> values)
{
    internal Span<int> Values { get; } = values;
}

internal sealed class MatcherPredicateOwner(int expected)
{
    internal bool Accepts(ReadOnlySpan<int> values) =>
        values.Length == 1 &&
        values[0] == expected;
}
