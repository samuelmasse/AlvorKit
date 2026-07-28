namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockBehaviorMatrixTest
{
    /// <summary>
    /// Confirms that strict and loose proxy mocks retain distinct fallback contracts.
    /// </summary>
    [TestMethod]
    public void FallbackModes_ProxyMocksPreserveDistinctContracts()
    {
        var strictInterface = Mock.Create<IMockTarget>();
        Assert.Throws<MockException>(() => strictInterface.GetValue());
        Mock.Verify(strictInterface.GetValue).Once();
        Mock.VerifyNoOtherCalls(strictInterface);

        var looseOrdinary = Mock.CreateLoose<IMockTarget>();
        Assert.AreEqual(0, looseOrdinary.GetValue());
        Mock.Verify(looseOrdinary.GetValue).Once();
        Mock.VerifyNoOtherCalls(looseOrdinary);

    }

    /// <summary>
    /// Confirms that return, throw, sequence, answer, and callback behaviors work across value, property, ref, and out members.
    /// </summary>
    [TestMethod]
    public void ConfiguredBehaviors_MembersAndVerificationShareOneEngine()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("configured");
        var setupReference = 10;

        Mock.When(() => mock.Property).Return(7);
        Mock.When(mock.GetValue).ReturnSequence(1, 2);
        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), Arg.Any<int>()))
            .Answer(call => (10 * call.Argument<int>(0)) + call.Argument<int>(1));
        Mock.When(() => mock.ComputeSum(9, 9)).Throw(expected);
        Mock.When(() => mock.Write(ref setupReference))
            .Do(call => call.SetReference(0, call.Argument<int>(0) + 5));
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(0, 42));

        Assert.AreEqual(7, mock.Property);
        Assert.AreEqual(23, mock.ComputeSum(2, 3));
        CollectionAssert.AreEqual(
            new[] { 1, 2, 2 },
            new[] { mock.GetValue(), mock.GetValue(), mock.GetValue() });
        Assert.AreSame(
            expected,
            Assert.Throws<InvalidOperationException>(() => mock.ComputeSum(9, 9)));

        var reference = 10;
        mock.Write(ref reference);
        Assert.AreEqual(15, reference);

        mock.Read(out var output);
        Assert.AreEqual(42, output);

        Mock.Verify(() => mock.Property).Once();
        Mock.Verify(() => mock.ComputeSum(2, 3)).Once();
        Mock.Verify(() => mock.ComputeSum(9, 9)).Once();
        Mock.Verify(mock.GetValue).Exactly(3);
        var verifiedReference = 10;
        Mock.Verify(() => mock.Write(ref verifiedReference)).Once();
        Mock.Verify(() => mock.Read(out _)).Once();
        Mock.VerifyNoOtherCalls(mock);

        Assert.AreEqual(8, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>
    /// Confirms that event accessors are recorded while event-raise capture remains absent from the invocation ledger.
    /// </summary>
    [TestMethod]
    public void EventAccessors_AreRecordedAndRaiseCaptureIsExcluded()
    {
        var mock = Mock.Create<IMockTarget>();
        var observed = 0;
        void handler(int value) => observed += value;

        mock.OnActionEvent += handler;
        Mock.Raise(() => mock.OnActionEvent += null!, 5);
        mock.OnActionEvent -= handler;
        Mock.Raise(() => mock.OnActionEvent += null!, 7);

        Assert.AreEqual(5, observed);
        Assert.AreEqual(2, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);

        Mock.Verify(() => mock.OnActionEvent += handler).Once();
        Mock.Verify(() => mock.OnActionEvent -= handler).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>
    /// Confirms that setup and verification capture neither trigger strict fallback nor create ledger entries.
    /// </summary>
    [TestMethod]
    public void StrictCapture_SetupAndVerificationDoNotRecord()
    {
        var mock = Mock.Create<IMockTarget>();

        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), 5)).Return(9);
        Assert.AreEqual(0, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);

        Mock.Verify(() => mock.ComputeSum(3, 5)).Never();
        Assert.AreEqual(0, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);

        Assert.AreEqual(9, mock.ComputeSum(3, 5));
        Mock.Verify(() => mock.ComputeSum(3, 5)).Once();
        Mock.VerifyNoOtherCalls(mock);

        Assert.AreEqual(1, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>
    /// Confirms that an answer can safely reenter the same mock and that both invocations remain independently verifiable.
    /// </summary>
    [TestMethod]
    public void Answer_ReentersSameMock_RecordsOuterAndNestedCalls()
    {
        var mock = Mock.Create<IMockTarget>();

        Mock.When(mock.GetValue).Return(7);
        Mock.When(() => mock.ComputeSum(1, 2))
            .Answer(call => mock.GetValue() + call.Argument<int>(0) + call.Argument<int>(1));

        Assert.AreEqual(10, mock.ComputeSum(1, 2));
        Assert.AreEqual(2, Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);

        Mock.Verify(() => mock.ComputeSum(1, 2)).Once();
        Mock.Verify(mock.GetValue).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>
    /// Confirms that no-other-calls failures report the deterministic sequence of every remaining invocation.
    /// </summary>
    [TestMethod]
    public void VerifyNoOtherCalls_ReportsRemainingCallsInLogicalOrder()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).Return(1);
        Mock.When(() => mock.ComputeSum(1, 2)).Return(3);

        Assert.AreEqual(1, mock.GetValue());
        Assert.AreEqual(3, mock.ComputeSum(1, 2));
        Assert.AreEqual(3, mock.ComputeSum(1, 2));

        Mock.Verify(mock.GetValue).Once();
        var error = Assert.Throws<MockException>(() => Mock.VerifyNoOtherCalls(mock));

        StringAssert.Contains(error.Message, "ComputeSum");
        Assert.IsTrue(error.Message.IndexOf("#2", StringComparison.Ordinal) >= 0);
        Assert.IsTrue(
            error.Message.IndexOf("#3", StringComparison.Ordinal) >
            error.Message.IndexOf("#2", StringComparison.Ordinal));

        Mock.Verify(() => mock.ComputeSum(1, 2)).Exactly(2);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>
    /// Confirms that joined concurrent callers consume each sequence value once and produce an exact invocation ledger.
    /// </summary>
    [TestMethod]
    public void ConcurrentSequenceAndLedger_ExactMultisetAndCount()
    {
        const int callerCount = 32;
        var mock = Mock.Create<IMockTarget>();
        var configured = new int[callerCount];
        var results = new int[callerCount];
        for (var index = 0; index < callerCount; index++)
        {
            configured[index] = index;
        }

        Mock.When(mock.GetValue).ReturnSequence(configured);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
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
        Task.WaitAll(callers);

        CollectionAssert.AreEquivalent(configured, results);
        Assert.AreEqual(
            callerCount,
            Mock.GetMocked(mock)!.Invocations.Snapshot().Invocations.Length);
        Mock.Verify(mock.GetValue).Exactly(callerCount);
        Mock.VerifyNoOtherCalls(mock);
    }
}
