namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockOrderTest
{
    /// <summary>Per-mock sequence verification supports repeated patterns and marks every matched call.</summary>
    [TestMethod]
    public void VerifySequence_PerMockRepeatedPatternsMatchExactly()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        mock.ComputeSum(1, 0);
        mock.ComputeSum(2, 0);
        mock.ComputeSum(1, 0);

        session.VerifySequence(
            () => mock.ComputeSum(1, 0),
            () => mock.ComputeSum(2, 0),
            () => mock.ComputeSum(1, 0));

        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Ordered patterns share exact, any, and predicate matching with count verification.</summary>
    [TestMethod]
    public void VerifySequence_MatchersUseSharedCaptureSemantics()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        mock.ComputeSum(1, 10);
        mock.ComputeSum(2, 10);

        session.VerifySequence(
            () => mock.ComputeSum(Arg.Any<int>(), 10),
            () => mock.ComputeSum(
                Arg.Match<int>(value => value == 2),
                10));

        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Cross-mock verification distinguishes receivers that expose the same operation.</summary>
    [TestMethod]
    public void VerifySequence_WrongMockFailsAtFirstDivergenceAndMarksNothing()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        first.GetValue();
        second.GetValue();

        var failure = Assert.Throws<MockException>(
            () => session.VerifySequence(
                () => second.GetValue(),
                () => first.GetValue()));

        StringAssert.Contains(failure.Message, "position 0");
        StringAssert.Contains(
            failure.Message,
            $"mock #{Mock.GetMocked(second)!.Invocations.Id}");
        AssertAllUnverified(first);
        AssertAllUnverified(second);
    }

    /// <summary>Missing and extra calls report the first sequence length divergence.</summary>
    [TestMethod]
    public void VerifySequence_MissingAndExtraCallsReportEnds()
    {
        var missing = Mock.CreateLoose<IMockTarget>();
        using (var session = Mock.Session())
        {
            missing.GetValue();

            var failure = Assert.Throws<MockException>(
                () => session.VerifySequence(
                    () => missing.GetValue(),
                    () => missing.GetValue()));

            StringAssert.Contains(failure.Message, "position 1");
            StringAssert.Contains(
                failure.Message,
                "<end of actual sequence>");
            AssertAllUnverified(missing);
        }

        var extra = Mock.CreateLoose<IMockTarget>();
        using (var session = Mock.Session())
        {
            extra.GetValue();
            extra.GetValue();

            var failure = Assert.Throws<MockException>(
                () => session.VerifySequence(
                    () => extra.GetValue()));

            StringAssert.Contains(failure.Message, "position 1");
            StringAssert.Contains(
                failure.Message,
                "<end of expected sequence>");
            AssertAllUnverified(extra);
        }
    }

    /// <summary>Checkpoint-restricted order marks only calls in its lower-exclusive, upper-inclusive window.</summary>
    [TestMethod]
    public void VerifySequence_CheckpointWindowExcludesOutsideCalls()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        first.ComputeSum(0, 0);
        var before = session.Checkpoint();
        second.ComputeSum(1, 0);
        first.ComputeSum(2, 0);
        var through = session.Checkpoint();
        second.ComputeSum(3, 0);

        session.VerifySequence(
            before,
            through,
            () => second.ComputeSum(1, 0),
            () => first.ComputeSum(2, 0));

        AssertVerifiedState(first, false, true);
        AssertVerifiedState(second, true, false);

        Mock.Verify(() => first.ComputeSum(0, 0)).Once();
        Mock.Verify(() => second.ComputeSum(3, 0)).Once();
        Mock.VerifyNoOtherCalls(first);
        Mock.VerifyNoOtherCalls(second);
    }

    /// <summary>Ordered verification observes only the current history epoch after a clear.</summary>
    [TestMethod]
    public void VerifySequence_ClearRemovesRetiredEpochFromOrder()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        var start = session.Checkpoint();
        mock.ComputeSum(1, 0);
        Mock.ClearInvocations(mock);
        mock.ComputeSum(2, 0);
        var through = session.Checkpoint();

        session.VerifySequence(
            start,
            through,
            () => mock.ComputeSum(2, 0));

        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Concurrent order verification follows assigned logical numbers without physical-time assumptions.</summary>
    [TestMethod]
    public async Task VerifySequence_ConcurrentCallsFollowLogicalOrder()
    {
        const int callCount = 64;
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        var start = session.Checkpoint();
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var callers = new Task[callCount];

        for (var i = 0; i < callers.Length; i++)
        {
            var value = i;
            callers[i] = Task.Run(
                async () =>
                {
                    await release.Task;
                    var mock = (value & 1) == 0 ? first : second;
                    mock.ComputeSum(value, 0);
                });
        }

        release.SetResult();
        await Task.WhenAll(callers);
        var through = session.Checkpoint();
        var actual = session.SnapshotBetween(start, through);
        var firstId = Mock.GetMocked(first)!.Invocations.Id;
        var expected = new Action[actual.Length];

        for (var i = 0; i < actual.Length; i++)
        {
            var value = (int)actual[i].Arguments[0].Entry.Value!;
            expected[i] = actual[i].Identity.Target.OwnerId == firstId
                ? () => first.ComputeSum(value, 0)
                : () => second.ComputeSum(value, 0);
        }

        session.VerifySequence(start, through, expected);
        Mock.VerifyNoOtherCalls(first);
        Mock.VerifyNoOtherCalls(second);
    }

    /// <summary>A matcher exception propagates unchanged and leaves the entire sequence unverified.</summary>
    [TestMethod]
    public void VerifySequence_PredicateFailureMarksNothing()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        mock.ComputeSum(1, 0);
        var expected = new InvalidOperationException("sequence predicate");

        var actual = Assert.Throws<InvalidOperationException>(
            () => session.VerifySequence(
                () => mock.ComputeSum(
                    Arg.Match<int>(_ => throw expected),
                    0)));

        Assert.AreSame(expected, actual);
        AssertAllUnverified(mock);
    }

    /// <summary>Sequence windows reject reversed, foreign, non-current, and null expectations.</summary>
    [TestMethod]
    public void VerifySequence_InvalidScopeOrExpectationThrows()
    {
        MockCheckpoint foreign;
        using (var other = Mock.Session())
            foreign = other.Checkpoint();

        var session = Mock.Session();
        var before = session.Checkpoint();
        var mock = Mock.CreateLoose<IMockTarget>();
        mock.GetValue();
        var through = session.Checkpoint();

        Assert.Throws<MockException>(
            () => session.VerifySequence(
                through,
                before,
                () => mock.GetValue()));
        Assert.Throws<MockException>(
            () => session.VerifySequence(
                before,
                foreign,
                () => mock.GetValue()));
        Assert.Throws<ArgumentException>(
            () => session.VerifySequence([null!]));

        using (Mock.Session())
        {
            Assert.Throws<MockException>(
                () => session.VerifySequence(
                    () => mock.GetValue()));
        }

        session.Dispose();
        Assert.Throws<ObjectDisposedException>(
            () => session.VerifySequence(
                () => mock.GetValue()));
    }

    private static void AssertAllUnverified(object mock) =>
        Assert.IsTrue(
            Mock.GetMocked(mock)!.Invocations
                .Snapshot()
                .Invocations
                .ToArray()
                .All(static invocation => !invocation.IsVerified));

    private static void AssertVerifiedState(
        object mock,
        params bool[] expected)
    {
        var invocations = Mock.GetMocked(mock)!.Invocations
            .Snapshot()
            .Invocations;
        Assert.AreEqual(expected.Length, invocations.Length);
        for (var i = 0; i < invocations.Length; i++)
            Assert.AreEqual(expected[i], invocations[i].IsVerified);
    }
}
