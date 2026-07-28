namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockInvocationTest
{
    /// <summary>Configured calls produce one completed record while setup capture produces none.</summary>
    [TestMethod]
    public void ConfiguredCall_RecordsOnceAndCaptureIsExcluded()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(() => mock.ComputeSum(3, 4)).Return(7);

        Assert.AreEqual(0, Snapshot(mock).Invocations.Length);

        Assert.AreEqual(7, mock.ComputeSum(3, 4));

        var invocation = AssertSingle(mock);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreEqual(7, invocation.Completion.Return!.Value);
        Assert.AreEqual(3, invocation.Arguments[0].Entry.Value);
        Assert.AreEqual(4, invocation.Arguments[1].Entry.Value);
    }

    /// <summary>Strict failures retain the original exception on the existing invocation record.</summary>
    [TestMethod]
    public void StrictFailure_RecordsExactException()
    {
        var mock = Mock.Create<IMockTarget>();

        var expected = Assert.Throws<MockException>(
            () => mock.ComputeSum(3, 4));

        var invocation = AssertSingle(mock);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.StrictFailure,
            invocation.Completion.Source);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>Reference history normalizes out entry state and records its normal exit value.</summary>
    [TestMethod]
    public void OutCall_RecordsUnavailableEntryAndShallowExit()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(0, 42));

        mock.Read(out var value);

        Assert.AreEqual(42, value);
        var argument = AssertSingle(mock).Arguments[0];
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Unavailable,
            argument.Entry.Kind);
        Assert.AreEqual(
            MockUnavailableReason.OutHasNoEntryValue,
            argument.Entry.Unavailable!.Reason);
        Assert.AreEqual(
            MockInvocationArgumentSnapshotKind.Shallow,
            argument.Exit.Kind);
        Assert.AreEqual(42, argument.Exit.Value);
    }

    /// <summary>Clearing starts a new epoch for one mock without removing its setups.</summary>
    [TestMethod]
    public void ClearInvocations_ClearsOneMockAndKeepsSetups()
    {
        var first = Mock.Create<IMockTarget>();
        var second = Mock.Create<IMockTarget>();
        Mock.When(first.GetValue).Return(10);
        Mock.When(second.GetValue).Return(20);
        Assert.AreEqual(10, first.GetValue());
        Assert.AreEqual(20, second.GetValue());

        Mock.ClearInvocations(first);

        Assert.AreEqual(0, Snapshot(first).Invocations.Length);
        Assert.AreEqual(1, Snapshot(second).Invocations.Length);
        Assert.AreEqual(10, first.GetValue());
        Assert.AreEqual(1, Snapshot(first).Invocations.Length);
    }

    /// <summary>Clearing validates null and non-mock arguments before accessing history.</summary>
    [TestMethod]
    public void ClearInvocations_InvalidTarget_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => Mock.ClearInvocations(null!));
        Assert.Throws<MockException>(
            () => Mock.ClearInvocations(new object()));
    }

    /// <summary>Joined concurrent callers append exactly one record per intercepted call.</summary>
    [TestMethod]
    public void LooseCalls_ConcurrentAppendCountIsExact()
    {
        const int callerCount = 256;
        var mock = Mock.CreateLoose<IMockTarget>();

        Parallel.For(
            0,
            callerCount,
            _ => mock.GetValue());

        Assert.AreEqual(callerCount, Snapshot(mock).Invocations.Length);
    }

    private static MockInvocationLedgerSnapshot Snapshot(object mock) =>
        Mock.GetMocked(mock)!.Invocations.Snapshot();

    private static MockInvocation AssertSingle(object mock)
    {
        var invocations = Snapshot(mock).Invocations;
        Assert.AreEqual(1, invocations.Length);
        return invocations[0];
    }
}
