namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockDiagnosticsTest
{
    private static readonly MethodInfo Method =
        typeof(MockDiagnosticsTest).GetMethod(
            nameof(Target),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Strict failures include the target, exact signature, backend, and ordinary values.</summary>
    [TestMethod]
    public void StrictFailure_ContainsActionableInvocationDetails()
    {
        var mock = Mock.Create<IMockTarget>();
        using var session = Mock.Session();

        var exception = Assert.Throws<MockException>(
            () => mock.ComputeSum(3, 4));

        StringAssert.Contains(exception.Message, typeof(IMockTarget).FullName);
        StringAssert.Contains(exception.Message, "ComputeSum(System.Int32, System.Int32)");
        StringAssert.Contains(exception.Message, "dynamic instance backend");
        StringAssert.Contains(exception.Message, "session #");
        StringAssert.Contains(exception.Message, "timeline #");
        StringAssert.Contains(exception.Message, "Received: 3, 4");
    }

    /// <summary>Diagnostic value formatting never invokes a user ToString override.</summary>
    [TestMethod]
    public void UnexpectedInvocation_DoesNotInvokeUserFormatting()
    {
        var mocked = new Mocked(
            MockFallbackBehavior.Strict,
            Types.Get(typeof(MockDiagnosticsTest)));

        var message = MockDiagnostics.UnexpectedInvocation(
            mocked,
            Method,
            [new DiagnosticValue()]);

        StringAssert.Contains(message, typeof(DiagnosticValue).FullName);
    }

    /// <summary>Strict and remaining-call failures cannot reenter mocking through user formatting.</summary>
    [TestMethod]
    public void InvocationFailures_DoNotInvokeReentrantFormatting()
    {
        var target = Mock.Create<IDiagnosticMockTarget>();
        var nested = Mock.Create<IMockTarget>();
        var value = new ReentrantDiagnosticValue(
            () => nested.GetValue());

        var strict = Assert.Throws<MockException>(
            () => target.Accept(value));
        var remaining = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(target));

        Assert.AreEqual(0, value.FormattingCount);
        Assert.AreEqual(
            0,
            Mock.GetMocked(nested)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            Mock.GetMocked(target)!.Invocations
                .Snapshot()
                .Invocations[0]
                .Completion
                .Kind);
        StringAssert.Contains(
            strict.Message,
            typeof(ReentrantDiagnosticValue).FullName);
        StringAssert.Contains(
            remaining.Message,
            typeof(ReentrantDiagnosticValue).FullName);
    }

    /// <summary>Formatting a mocked argument uses attached type metadata without invoking the mock.</summary>
    [TestMethod]
    public void UnexpectedInvocation_DoesNotInvokeMockedArgument()
    {
        var target = Mock.Create<IDiagnosticMockTarget>();
        var argument = Mock.Create<IMockTarget>();

        var failure = Assert.Throws<MockException>(
            () => target.Accept(argument));

        StringAssert.Contains(
            failure.Message,
            $"<mock {typeof(IMockTarget).FullName}>");
        Assert.AreEqual(
            0,
            Mock.GetMocked(argument)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
    }

    /// <summary>Repeated formatting is byte-for-byte stable and bounds large or unsafe values.</summary>
    [TestMethod]
    public void UnexpectedInvocation_IsDeterministicAndBounded()
    {
        var mocked = new Mocked(
            MockFallbackBehavior.Strict,
            Types.Get(typeof(MockDiagnosticsTest)));
        var method = typeof(MockDiagnosticsTest).GetMethod(
            nameof(Values),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var text = new string('a', 90) + "\n\"tail";
        object?[] arguments =
        [
            text,
            Enumerable.Range(0, 100).ToArray(),
            new DiagnosticValue()
        ];

        var first = MockDiagnostics.UnexpectedInvocation(mocked, method, arguments);
        var second = MockDiagnostics.UnexpectedInvocation(mocked, method, arguments);

        Assert.AreEqual(first, second);
        StringAssert.Contains(first, new string('a', 80) + "…\"");
        StringAssert.Contains(first, "<System.Int32[] length=100>");
        StringAssert.Contains(first, typeof(DiagnosticValue).FullName);
        Assert.IsFalse(first.Contains('\n'));
        Assert.IsTrue(first.Length < 400);
    }

    /// <summary>Projected and unavailable borrowed argument history remain visibly distinct.</summary>
    [TestMethod]
    public void ArgumentSnapshot_DistinguishesProjectedAndUnavailableValues()
    {
        var projected = MockInvocationArgumentSnapshot.Projected(
            0,
            typeof(ReadOnlySpan<int>),
            MockSnapshotPhase.Entry,
            new[] { 1, 2, 3 });
        var unavailable = MockInvocationArgumentSnapshot.UnavailableValue(
            new(
                0,
                typeof(ReadOnlySpan<int>),
                MockSnapshotPhase.Entry,
                MockUnavailableReason.ByRefLikeProjectionNotConfigured));

        var projectedText = MockDiagnostics.ArgumentSnapshot(projected);
        var unavailableText = MockDiagnostics.ArgumentSnapshot(unavailable);

        StringAssert.Contains(projectedText, "<projected>");
        StringAssert.Contains(projectedText, "<System.Int32[] length=3>");
        StringAssert.Contains(unavailableText, "<unavailable: ByRefLikeProjectionNotConfigured>");
        Assert.AreNotEqual(projectedText, unavailableText);
    }

    /// <summary>Candidate lists retain deterministic order while bounding diagnostic size.</summary>
    [TestMethod]
    public void AppendSequences_BoundsCandidateLists()
    {
        var message = new StringBuilder();
        long[] sequences = [.. Enumerable.Range(1, 20).Select(static value => (long)value)];

        MockDiagnostics.AppendSequences(message, sequences);

        Assert.AreEqual(
            " 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, … (+8 more)",
            message.ToString());
    }

    /// <summary>Count and remaining-call failures are stable, bounded, and never consume candidates.</summary>
    [TestMethod]
    public void VerificationFailures_AreStableBoundedAndStateNeutral()
    {
        const int candidateCount = 20;
        var mock = Mock.CreateLoose<IMockTarget>();
        for (var i = 0; i < candidateCount; i++)
            mock.ComputeSum(i, 10);

        var firstCount = Assert.Throws<MockException>(
            () => Mock.Verify(
                    () => mock.ComputeSum(
                        Arg.Any<int>(),
                        10))
                .Exactly(candidateCount + 1));
        var secondCount = Assert.Throws<MockException>(
            () => Mock.Verify(
                    () => mock.ComputeSum(
                        Arg.Any<int>(),
                        10))
                .Exactly(candidateCount + 1));
        var firstRemaining = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));
        var secondRemaining = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));

        Assert.AreEqual(firstCount.Message, secondCount.Message);
        Assert.AreEqual(
            firstRemaining.Message,
            secondRemaining.Message);
        StringAssert.Contains(
            firstCount.Message,
            "patterns=[any, exact 10]");
        StringAssert.Contains(
            firstCount.Message,
            "... (+8 more)");
        StringAssert.Contains(
            firstRemaining.Message,
            "... (+8 more)");
        Assert.IsTrue(firstCount.Message.Length < 4096);
        Assert.IsTrue(firstRemaining.Message.Length < 4096);
        Assert.IsTrue(
            Mock.GetMocked(mock)!.Invocations
                .Snapshot()
                .Invocations
                .ToArray()
                .All(static invocation => !invocation.IsVerified));
    }

    /// <summary>Invocation candidates bound strings and arrays without formatting hostile values.</summary>
    [TestMethod]
    public void CandidateValues_AreBoundedAndNonReentrant()
    {
        var mock = Mock.CreateLoose<IDiagnosticMockTarget>();
        var nested = Mock.Create<IMockTarget>();
        var value = new ReentrantDiagnosticValue(
            () => nested.GetValue());
        var text = new string('b', 120) + "\nend";
        var values = Enumerable.Range(0, 1000).ToArray();
        mock.Values(text, values, value);

        var failure = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));

        StringAssert.Contains(
            failure.Message,
            new string('b', 80) + "…\"");
        StringAssert.Contains(
            failure.Message,
            "<System.Int32[] length=1000>");
        StringAssert.Contains(
            failure.Message,
            typeof(ReentrantDiagnosticValue).FullName);
        Assert.AreEqual(0, value.FormattingCount);
        Assert.AreEqual(
            0,
            Mock.GetMocked(nested)!.Invocations
                .Snapshot()
                .Invocations
                .Length);
        Assert.IsTrue(failure.Message.Length < 4096);
    }

    /// <summary>Count diagnostics describe a predicate without evaluating it a second time.</summary>
    [TestMethod]
    public void CountFailure_DoesNotReevaluatePredicateForFormatting()
    {
        const int candidateCount = 3;
        var mock = Mock.CreateLoose<IMockTarget>();
        for (var i = 0; i < candidateCount; i++)
            mock.ComputeSum(i, 10);

        var predicateCalls = 0;
        var failure = Assert.Throws<MockException>(
            () => Mock.Verify(
                    () => mock.ComputeSum(
                        Arg.Match<int>(_ =>
                        {
                            predicateCalls++;
                            return true;
                        }),
                        10))
                .Exactly(candidateCount + 1));

        Assert.AreEqual(candidateCount, predicateCalls);
        StringAssert.Contains(failure.Message, "predicate");
        Assert.IsTrue(
            Mock.GetMocked(mock)!.Invocations
                .Snapshot()
                .Invocations
                .ToArray()
                .All(static invocation => !invocation.IsVerified));
    }

    /// <summary>Remaining-call diagnostics inspect history without replaying configured callbacks.</summary>
    [TestMethod]
    public void NoOtherCalls_DoesNotReplayCallback()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        var callbackCalls = 0;
        Mock.When(mock.RaiseEvent)
            .Do(_ => callbackCalls++);
        mock.RaiseEvent();

        Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(mock));

        Assert.AreEqual(1, callbackCalls);
        Assert.IsFalse(
            Mock.GetMocked(mock)!.Invocations
                .Snapshot()
                .Invocations[0]
                .IsVerified);
    }

    /// <summary>Order, signature, and lifecycle failures remain byte-for-byte stable.</summary>
    [TestMethod]
    public void CrossSurfaceFailures_AreByteStable()
    {
        var first = Mock.CreateLoose<IMockTarget>();
        var second = Mock.CreateLoose<IMockTarget>();
        using var session = Mock.Session();
        first.RaiseEvent();
        second.RaiseEvent();

        var firstOrder = Assert.Throws<MockException>(
            () => session.VerifySequence(
                second.RaiseEvent,
                first.RaiseEvent));
        var secondOrder = Assert.Throws<MockException>(
            () => session.VerifySequence(
                second.RaiseEvent,
                first.RaiseEvent));
        Assert.AreEqual(
            firstOrder.Message,
            secondOrder.Message);
        Assert.IsTrue(
            Mock.GetMocked(first)!.Invocations
                .Snapshot()
                .Invocations
                .ToArray()
                .All(static invocation => !invocation.IsVerified));

        var backend = new MockBackendIdentity(
            MockBackendKind.Proxy,
            1);
        var firstRejection = MockSignatureValidator.Validate(
            Method,
            backend,
            MockOperationKind.StaticMethod)
            .Rejection!;
        var secondRejection = MockSignatureValidator.Validate(
            Method,
            backend,
            MockOperationKind.StaticMethod)
            .Rejection!;
        Assert.AreEqual(
            firstRejection.Message,
            secondRejection.Message);
        StringAssert.Contains(
            firstRejection.Message,
            "Proxy ABI 1");
        StringAssert.Contains(
            firstRejection.Message,
            "[UnsupportedOperation]");

        var firstLifecycle = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(new object()));
        var secondLifecycle = Assert.Throws<MockException>(
            () => Mock.VerifyNoOtherCalls(new object()));
        Assert.AreEqual(
            firstLifecycle.Message,
            secondLifecycle.Message);
    }

    private static void Target(DiagnosticValue value)
    {
        _ = value;
    }

    private static void Values(
        string text,
        int[] values,
        DiagnosticValue diagnostic)
    {
        _ = text;
        _ = values;
        _ = diagnostic;
    }
}
