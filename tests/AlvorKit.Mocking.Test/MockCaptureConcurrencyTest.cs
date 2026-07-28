namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockCaptureConcurrencyTest
{
    /// <summary>Parallel setup and verification captures retain their own receiver, method, and matchers.</summary>
    [TestMethod]
    public void ParallelCapture_KeepsPerThreadStateIsolated()
    {
        const int workerCount = 24;
        var mocks = new IMockTarget[workerCount];
        var results = new int[workerCount];

        Parallel.For(
            0,
            workerCount,
            i =>
            {
                var mock = Mock.Create<IMockTarget>();
                mocks[i] = mock;
                Mock.When(() => mock.ComputeSum(i, Arg.Any<int>()))
                    .Return(i + 100);
                results[i] = mock.ComputeSum(i, -i);
                Mock.Verify(() => mock.ComputeSum(i, Arg.Any<int>()))
                    .Once();
                AssertCaptureStateClean();
            });

        for (var i = 0; i < workerCount; i++)
        {
            Assert.AreEqual(i + 100, results[i]);
            Mock.VerifyNoOtherCalls(mocks[i]);
        }
    }

    /// <summary>A failed capture leaves the reused worker thread clean for the next setup.</summary>
    [TestMethod]
    public async Task FailedCapture_CleansStateBeforeThreadReuse()
    {
        var mock = Mock.Create<IMockTarget>();

        await Task.Run(
            () =>
            {
                Assert.Throws<MockException>(
                    () => Mock.When(static () => { }));
                AssertCaptureStateClean();

                Mock.When(() => mock.GetValue()).Return(37);
                Assert.AreEqual(37, mock.GetValue());
                AssertCaptureStateClean();
            });
    }

    /// <summary>A nested capture is rejected without ending or corrupting its outer capture state.</summary>
    [TestMethod]
    public void NestedCapture_IsRejectedAndOuterCleanupIsUnconditional()
    {
        var mock = Mock.Create<IMockTarget>();
        MockException? nestedFailure = null;

        Mock.When(
                () =>
                {
                    nestedFailure = Assert.Throws<MockException>(
                        () => Mock.When(
                                () => mock.GetValue())
                            .Return(1));
                    Assert.IsTrue(Capture.Context.IsActive);
                    Assert.AreEqual(
                        CaptureOperation.Setup,
                        Capture.Context.Operation);
                    mock.RaiseEvent();
                })
            .Do(_ => { });

        Assert.IsNotNull(nestedFailure);
        StringAssert.Contains(
            nestedFailure.Message,
            "capture while setup capture is active");
        AssertCaptureStateClean();
        mock.RaiseEvent();
        Mock.Verify(mock.RaiseEvent).Once();
        Mock.When(() => mock.GetValue()).Return(41);
        Assert.AreEqual(41, mock.GetValue());
        AssertCaptureStateClean();
    }

    /// <summary>A throwing matcher completes its invocation and cannot contaminate later capture state.</summary>
    [TestMethod]
    public void MatcherFailure_DoesNotLeakCaptureOrMatcherState()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("predicate");
        Mock.When(
                () => mock.ComputeSum(
                    Arg.Match<int>(_ => throw expected),
                    2))
            .Return(1);

        var actual = Assert.Throws<InvalidOperationException>(
            () => mock.ComputeSum(1, 2));
        Assert.AreSame(expected, actual);
        AssertCaptureStateClean();

        Mock.When(() => mock.ComputeSum(1, 2)).Return(43);
        Assert.AreEqual(43, mock.ComputeSum(1, 2));
        Mock.Verify(() => mock.ComputeSum(1, 2)).Exactly(2);
        AssertCaptureStateClean();
    }

    /// <summary>A reentrant callback may publish a later setup after the original capture has ended.</summary>
    [TestMethod]
    public void CallbackTriggeredSetup_IsReentrantAndStartsFreshCapture()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(() => mock.ComputeSum(1, 2))
            .Answer(
                _ =>
                {
                    AssertCaptureStateClean();
                    Mock.When(() => mock.GetValue()).Return(44);
                    AssertCaptureStateClean();
                    return 3;
                });

        Assert.AreEqual(3, mock.ComputeSum(1, 2));
        Assert.AreEqual(44, mock.GetValue());
        AssertCaptureStateClean();
    }

    /// <summary>Failures before, during, and after matcher replay clear every thread-local capture component.</summary>
    [TestMethod]
    public void CaptureFailures_ClearStateUnconditionally()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("capture");

        var firstPass = Assert.Throws<InvalidOperationException>(
            () => Mock.When(
                () =>
                {
                    mock.ComputeSum(
                        Arg.Any<int>(),
                        2);
                    throw expected;
                }));
        Assert.AreSame(expected, firstPass);
        AssertCaptureStateClean();

        var pass = 0;
        var secondPass = Assert.Throws<InvalidOperationException>(
            () => Mock.When(
                () =>
                {
                    var result = mock.ComputeSum(
                        Arg.Any<int>(),
                        2);
                    pass++;
                    if (pass == 2)
                        throw expected;

                    return result;
                }));
        Assert.AreSame(expected, secondPass);
        AssertCaptureStateClean();

        var replay = 0;
        Assert.Throws<MockException>(
            () => Mock.When(
                () =>
                {
                    if (replay++ == 0)
                    {
                        mock.ComputeSum(
                            Arg.Any<int>(),
                            2);
                    }
                    else
                    {
                        mock.ComputeSum(0, 2);
                    }
                }));
        AssertCaptureStateClean();
    }

    /// <summary>Failed capture releases boxed arguments and predicate closures from reusable thread-local storage.</summary>
    [TestMethod]
    public void FailedCapture_ReleasesThreadLocalTestValues()
    {
        var retained = CreateFailedCaptureReferences();

        CollectGarbage();

        Assert.IsFalse(retained.Argument.IsAlive);
        Assert.IsFalse(retained.Matcher.IsAlive);
        AssertCaptureStateClean();
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices
            .MethodImplOptions.NoInlining)]
    private static (
        WeakReference Argument,
        WeakReference Matcher)
        CreateFailedCaptureReferences()
    {
        var argument = new object();
        var matcherValue = new object();
        var argumentReference =
            new WeakReference(argument);
        var matcherReference =
            new WeakReference(matcherValue);
        var mock =
            Mock.Create<IDiagnosticMockTarget>();
        var expected =
            new InvalidOperationException("abandon capture");

        var actual = Assert.Throws<InvalidOperationException>(
            () => Mock.When(
                () =>
                {
                    mock.Accept(argument);
                    Arg.Match<int>(
                        _ => matcherValue.GetHashCode() != 0);
                    throw expected;
                }));
        Assert.AreSame(expected, actual);
        AssertCaptureStateClean();
        return (argumentReference, matcherReference);
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static void AssertCaptureStateClean()
    {
        Assert.AreEqual(
            default,
            Capture.Context);
        Assert.AreEqual(0, Capture.FirstMatchers.Count);
        Assert.AreEqual(0, Capture.SecondMatchers.Count);
        Assert.AreEqual(0, Capture.FirstIndexedMatchers.Count);
        Assert.AreEqual(0, Capture.SecondIndexedMatchers.Count);
    }
}
