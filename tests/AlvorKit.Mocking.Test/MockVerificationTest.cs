namespace AlvorKit;

[TestClass]
public sealed class MockVerificationTest
{
    /// <summary>Every count form succeeds with exact, any, and predicate matching.</summary>
    [TestMethod]
    public void Verify_CountFormsUseSharedMatcherSemantics()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), 10)).Return(20);

        Assert.AreEqual(20, mock.ComputeSum(1, 10));
        Assert.AreEqual(20, mock.ComputeSum(1, 10));
        Assert.AreEqual(20, mock.ComputeSum(2, 10));

        Mock.Verify(() => mock.ComputeSum(1, 10)).Exactly(2);
        Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), 10)).AtLeast(3);
        Mock.Verify(() => mock.ComputeSum(Arg.Match<int>(value => value > 1), 10)).Once();
        Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), 10)).AtMost(3);
        Mock.Verify(() => mock.ComputeSum(9, 9)).Never();
        Mock.VerifyNoOtherCalls(mock);

        Assert.AreEqual(3, Snapshot(mock).Invocations.Length);
    }

    /// <summary>Void, getter, setter, ref, and normalized out captures match entry history.</summary>
    [TestMethod]
    public void Verify_ActionAndValueMembersUseDeclaredArgumentOrder()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        var reference = 44;

        mock.Read(out _);
        mock.Write(ref reference);
        mock.RaiseEvent();
        _ = mock.Property;
        mock["key"] = 7;

        Mock.Verify(() => mock.Read(out _)).Once();
        var expectedReference = 44;
        Mock.Verify(() => mock.Write(ref expectedReference)).Once();
        Mock.Verify(mock.RaiseEvent).Once();
        Mock.Verify(() => mock.Property).Once();
        Mock.Verify(() => mock["key"] = 7).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Ref-struct reference positions remain matchable through declared-index Any.</summary>
    [TestMethod]
    public void Verify_RefStructReferenceWithoutCarrierSlotMatchesHistory()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        Span<int> actual = stackalloc int[1];

        mock.ComputeSumWithSpanRef(1, 2, ref actual);
        Mock.Verify(() =>
            mock.ComputeSumWithSpanRef(
                1,
                2,
                ref Arg.AnyRef<Span<int>>(2)))
            .Once();

        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Verification preserves configured semantics on proxy-owned calls.</summary>
    [TestMethod]
    public void Verify_MockShapesShareBehavior()
    {
        var interfaceMock = Mock.CreateLoose<IMockTarget>();
        interfaceMock.GetValue();

        Mock.Verify(interfaceMock.GetValue).Once();

        Mock.VerifyNoOtherCalls(interfaceMock);
    }

    /// <summary>Capturing verification on a strict mock neither falls back nor records history.</summary>
    [TestMethod]
    public void Verify_StrictCaptureDoesNotDispatchOrRecord()
    {
        var mock = Mock.Create<IMockTarget>();

        Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), 10)).Never();

        Assert.AreEqual(0, Snapshot(mock).Invocations.Length);
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Every failing count form throws and leaves the real call unverified.</summary>
    [TestMethod]
    public void Verify_FailingCountFormsMarkNothing()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        mock.ComputeSum(1, 1);

        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(1, 1)).Never());
        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(2, 2)).Once());
        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(1, 1)).Exactly(2));
        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(1, 1)).AtLeast(2));
        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(1, 1)).AtMost(0));

        Assert.IsFalse(Snapshot(mock).Invocations[0].IsVerified);
        Assert.Throws<MockException>(() => Mock.VerifyNoOtherCalls(mock));
    }

    /// <summary>A failed count consumes no calls and remaining-call diagnostics stay sequence ordered.</summary>
    [TestMethod]
    public void Verify_FailedCountLeavesDeterministicRemainingCalls()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        mock.ComputeSum(3, 4);
        mock.ComputeSum(3, 4);
        mock.GetValue();

        Assert.Throws<MockException>(
            () => Mock.Verify(() => mock.ComputeSum(3, 4)).Exactly(3));

        var allRemaining = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));
        Assert.IsTrue(allRemaining.Message.IndexOf("\n  #1 ", StringComparison.Ordinal) <
            allRemaining.Message.IndexOf("\n  #2 ", StringComparison.Ordinal));
        Assert.IsTrue(allRemaining.Message.IndexOf("\n  #2 ", StringComparison.Ordinal) <
            allRemaining.Message.IndexOf("\n  #3 ", StringComparison.Ordinal));

        Mock.Verify(() => mock.ComputeSum(3, 4)).Exactly(2);
        var oneRemaining = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));
        StringAssert.Contains(oneRemaining.Message, "\n  #3 ");
        Assert.IsFalse(oneRemaining.Message.Contains("\n  #1 ", StringComparison.Ordinal));

        Mock.Verify(mock.GetValue).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>Negative counts fail before a stored predicate can inspect history.</summary>
    [TestMethod]
    public void Verify_NegativeCountsDoNotEvaluateMatchers()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        mock.ComputeSum(1, 10);
        var predicateCalls = 0;
        var verification = Mock.Verify(
            () => mock.ComputeSum(
                Arg.Match<int>(_ =>
                {
                    predicateCalls++;
                    return true;
                }),
                10));

        Assert.Throws<ArgumentOutOfRangeException>(() => verification.Exactly(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => verification.AtLeast(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => verification.AtMost(-1));
        Assert.AreEqual(0, predicateCalls);
        Assert.IsFalse(Snapshot(mock).Invocations[0].IsVerified);
    }

    /// <summary>A throwing predicate propagates unchanged and marks no invocation.</summary>
    [TestMethod]
    public void Verify_PredicateExceptionPropagatesUnchanged()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        mock.ComputeSum(1, 10);
        var expected = new InvalidOperationException("predicate");
        var verification = Mock.Verify(
            () => mock.ComputeSum(
                Arg.Match<int>(_ => throw expected),
                10));

        var actual = Assert.Throws<InvalidOperationException>(
            verification.Once);

        Assert.AreSame(expected, actual);
        Assert.IsFalse(Snapshot(mock).Invocations[0].IsVerified);
    }

    /// <summary>Verification capture requires exactly one mocked call and validates null delegates.</summary>
    [TestMethod]
    public void Verify_InvalidCaptureThrowsAndCleansUp()
    {
        Assert.Throws<ArgumentNullException>(() => Mock.Verify((Action)null!));
        Assert.Throws<ArgumentNullException>(() => Mock.Verify<int>(null!));
        Assert.Throws<MockException>(() => Mock.Verify(() => GC.KeepAlive(null)));
        Assert.Throws<MockException>(() => Mock.Verify(() => 42));

        var mock = Mock.CreateLoose<IMockTarget>();
        Assert.Throws<MockException>(
            () => Mock.Verify(
                () => mock.ComputeSum(mock.GetValue(), 1)));

        Assert.IsFalse(Capture.Context.IsActive);
        Mock.Verify(mock.GetValue).Never();
    }

    /// <summary>Nested capture is rejected without clearing the outer capture or the next operation.</summary>
    [TestMethod]
    public void Capture_NestedSetupIsRejectedAndThreadStateRecovers()
    {
        var mock = Mock.CreateLoose<IMockTarget>();

        Assert.Throws<MockException>(
            () => Mock.When(() =>
            {
                _ = Mock.When(mock.GetValue);
                return mock.GetValue();
            }));

        Assert.IsFalse(Capture.Context.IsActive);
        Mock.When(mock.GetValue).Return(8);
        Assert.AreEqual(8, mock.GetValue());
        Mock.Verify(mock.GetValue).Once();
    }

    /// <summary>Changed receiver during matcher replay fails without formatting either mock.</summary>
    [TestMethod]
    public void Capture_DisambiguationRequiresSameReceiver()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        var pass = 0;

        Assert.Throws<MockException>(
            () => Mock.Verify(
                () => (++pass == 1 ? first : second)
                    .ComputeSum(Arg.Any<int>(), 1)));

        pass = 0;
        Assert.Throws<MockException>(
            () => Mock.Verify(
                () => ++pass == 1
                    ? first.ComputeSum(Arg.Any<int>(), 1)
                    : first.GetValue()));

        Assert.IsFalse(Capture.Context.IsActive);
        Mock.Verify(first.GetValue).Never();
    }

    /// <summary>Changed matcher counts fail deterministically and leave the next capture clean.</summary>
    [TestMethod]
    public void Capture_DisambiguationRequiresConsistentMatchers()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        var pass = 0;

        var error = Assert.Throws<MockException>(
            () => Mock.Verify(
                () => mock.ComputeSum(
                    ++pass == 1 ? Arg.Any<int>() : 1,
                    10)));

        StringAssert.Contains(error.Message, "changed from 1 to 0");
        Assert.IsFalse(Capture.Context.IsActive);
        Mock.Verify(mock.GetValue).Never();
    }

    /// <summary>User exceptions during capture leave no matcher or active-operation state behind.</summary>
    [TestMethod]
    public void Capture_UserExceptionCleansThreadState()
    {
        var expected = new InvalidOperationException("capture");

        var actual = Assert.Throws<InvalidOperationException>(
            () => Mock.Verify<int>(() => throw expected));

        Assert.AreSame(expected, actual);
        Assert.IsFalse(Capture.Context.IsActive);

        var mock = Mock.CreateLoose<IMockTarget>();
        Mock.Verify(mock.GetValue).Never();
    }

    /// <summary>Parallel setup and verification captures keep thread-local matcher state isolated.</summary>
    [TestMethod]
    public void Capture_ParallelOperationsRemainIsolated()
    {
        const int count = 12;
        var mocks = new IMockTarget[count];
        for (var i = 0; i < mocks.Length; i++)
            mocks[i] = Mock.CreateLoose<IMockTarget>();

        Parallel.For(
            0,
            count,
            i =>
            {
                var mock = mocks[i];
                Mock.When(() => mock.ComputeSum(Arg.Any<int>(), i)).Return(i);
                Assert.AreEqual(i, mock.ComputeSum(100 + i, i));
                Mock.Verify(() => mock.ComputeSum(Arg.Any<int>(), i)).Once();
                Mock.VerifyNoOtherCalls(mock);
            });
    }

    /// <summary>No-other-calls validates null and objects outside the mocking runtime.</summary>
    [TestMethod]
    public void VerifyNoOtherCalls_InvalidTargetThrows()
    {
        Assert.Throws<ArgumentNullException>(
            () => Mock.VerifyNoOtherCalls(null!));
        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(new object()));

        var target = Mock.CreateLoose<IMockTarget>();
        var other = Mock.CreateLoose<IMockTarget>();
        target.GetValue();
        other.GetValue();

        Mock.Verify(target.GetValue).Once();
        Mock.VerifyNoOtherCalls(target);
        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(other));
    }

    private static MockInvocationLedgerSnapshot Snapshot(object mock) =>
        Mock.GetMocked(mock)!.Invocations.Snapshot();
}
