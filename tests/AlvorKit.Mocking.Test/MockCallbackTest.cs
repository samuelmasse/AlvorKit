namespace AlvorKit;

[TestClass]
public sealed class MockCallbackTest
{
    private static readonly TimeSpan CoordinationTimeout =
        TimeSpan.FromMilliseconds(750);

    /// <summary>A calculated answer exposes its receiver, method, and declared-order arguments and records one configured completion.</summary>
    [TestMethod]
    public void Answer_CalculatedReturn_ExposesCallContextAndRecordsCompletion()
    {
        var mock = Mock.Create<IMockTarget>();
        object? observedInstance = null;
        MethodInfo? observedMethod = null;
        Mock.When(() => mock.ComputeSum(
                Arg.Any<int>(),
                Arg.Any<int>()))
            .Answer(call =>
            {
                observedInstance = call.Instance;
                observedMethod = call.Method;
                return call.Argument<int>(0) * 10 +
                    call.Argument<int>(1);
            });

        var result = mock.ComputeSum(3, 4);

        Assert.AreEqual(34, result);
        Assert.AreSame(mock, observedInstance);
        Assert.AreEqual(
            nameof(IMockTarget.ComputeSum),
            observedMethod!.Name);
        var invocation = AssertSingle(mock);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreEqual(34, invocation.Completion.Return!.Value);
    }

    /// <summary>A void callback observes its receiver and records one configured void completion.</summary>
    [TestMethod]
    public void Do_VoidCall_ObservesInvocationAndRecordsCompletion()
    {
        var mock = Mock.Create<IMockTarget>();
        var callbackCount = 0;
        Mock.When(mock.RaiseEvent)
            .Do(call =>
            {
                Assert.AreSame(mock, call.Instance);
                Assert.AreEqual(
                    nameof(IMockTarget.RaiseEvent),
                    call.Method.Name);
                callbackCount++;
            });

        mock.RaiseEvent();

        Assert.AreEqual(1, callbackCount);
        var invocation = AssertSingle(mock);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationReturnKind.Void,
            invocation.Completion.Return!.Kind);
    }

    /// <summary>Callbacks read ref entry values and write invocation-local ref and out exits.</summary>
    [TestMethod]
    public void Do_RefAndOutParameters_ReadsAndWritesDeclaredValues()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(() => mock.Write(ref Arg.AnyRef<int>(0)))
            .Do(call =>
            {
                var input = call.Argument<int>(0);
                call.SetReference(0, input + 1);
            });
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(0, 42));

        var written = 10;
        mock.Write(ref written);
        mock.Read(out var read);

        Assert.AreEqual(11, written);
        Assert.AreEqual(42, read);
    }

    /// <summary>A callback exception escapes unchanged and completes one configured throwing record.</summary>
    [TestMethod]
    public void Answer_CallbackThrows_PreservesExceptionIdentityAndLedger()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("callback failed");
        Mock.When(mock.GetValue)
            .Answer(_ => throw expected);

        var actual = Assert.Throws<InvalidOperationException>(
            () => mock.GetValue());

        Assert.AreSame(expected, actual);
        var invocation = AssertSingle(mock);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>A callback can invoke another configured member on the same mock without deadlock.</summary>
    [TestMethod]
    public void Answer_ReentersSameMock_ReturnsNestedResult()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).Return(7);
        Mock.When(() => mock.ComputeSum(1, 2))
            .Answer(call =>
                mock.GetValue() +
                call.Argument<int>(0) +
                call.Argument<int>(1));

        var result = mock.ComputeSum(1, 2);

        Assert.AreEqual(10, result);
        Assert.AreEqual(2, Snapshot(mock).Invocations.Length);
    }

    /// <summary>Concurrent callbacks retain isolated ref entry and writeback state for every call.</summary>
    [TestMethod]
    public void Do_ConcurrentRefCalls_KeepPerCallStateIsolated()
    {
        const int callerCount = 8;
        var mock = Mock.Create<IMockTarget>();
        using var callbackBarrier = new Barrier(callerCount);
        Mock.When(() => mock.Write(ref Arg.AnyRef<int>(0)))
            .Do(call =>
            {
                if (!callbackBarrier.SignalAndWait(
                        TimeSpan.FromMilliseconds(500)))
                {
                    throw new TimeoutException(
                        "Callbacks did not execute concurrently.");
                }

                var input = call.Argument<int>(0);
                call.SetReference(0, input + 100);
            });

        var results = new int[callerCount];
        var failures = new Exception?[callerCount];
        var callers = new Thread[callerCount];
        for (var i = 0; i < callerCount; i++)
        {
            var callIndex = i;
            callers[i] = new(() =>
            {
                try
                {
                    var value = callIndex;
                    mock.Write(ref value);
                    results[callIndex] = value;
                }
                catch (Exception exception)
                {
                    failures[callIndex] = exception;
                }
            });
            callers[i].Start();
        }

        for (var i = 0; i < callers.Length; i++)
        {
            Assert.IsTrue(
                callers[i].Join(CoordinationTimeout),
                $"Callback caller {i} did not finish within the test bound.");
        }

        for (var i = 0; i < callerCount; i++)
        {
            Assert.IsNull(failures[i]);
            Assert.AreEqual(i + 100, results[i]);
        }

        Assert.AreEqual(
            callerCount,
            Snapshot(mock).Invocations.Length);
    }

    /// <summary>Value and void setup clauses reject null callbacks immediately.</summary>
    [TestMethod]
    public void Callback_Null_Throws()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<ArgumentNullException>(
            () => Mock.When(mock.GetValue).Answer(null!));
        Assert.Throws<ArgumentNullException>(
            () => Mock.When(mock.RaiseEvent).Do(null!));
    }

    /// <summary>The public callback surface exposes no Task or ValueTask callback overload.</summary>
    [TestMethod]
    public void Callback_PublicSurface_HasNoAsyncOverload()
    {
        var methods = typeof(MockSetupClause).GetMethods(
            BindingFlags.Instance | BindingFlags.Public);
        var callbackMethods = methods
            .Where(method => method.Name == nameof(MockSetupClause.Do))
            .ToArray();

        Assert.HasCount(4, callbackMethods);
        Assert.AreEqual(
            1,
            callbackMethods.Count(method =>
                !method.IsGenericMethod &&
                method.GetParameters()[0].ParameterType ==
                    typeof(Action<MockCall>)));
        Assert.AreEqual(
            1,
            callbackMethods.Count(method =>
                !method.IsGenericMethod &&
                method.GetParameters()[0].ParameterType ==
                    typeof(Delegate)));
        Assert.AreEqual(
            1,
            callbackMethods.Count(method =>
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters()[0].ParameterType
                    .GetGenericTypeDefinition() ==
                    typeof(Action<>)));
        Assert.AreEqual(
            1,
            callbackMethods.Count(method =>
                method.GetGenericArguments().Length == 2 &&
                method.GetParameters()[0].ParameterType
                    .GetGenericTypeDefinition() ==
                    typeof(Action<,>)));
        Assert.IsTrue(callbackMethods.All(method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 1));
        Assert.IsFalse(methods.Any(HasAsyncShape));
    }

    private static MockInvocationLedgerSnapshot Snapshot(object mock) =>
        Mock.GetMocked(mock)!.Invocations.Snapshot();

    private static bool HasAsyncShape(MethodInfo method)
    {
        if (method.Name.Contains(
                "Async",
                StringComparison.Ordinal))
        {
            return true;
        }

        if (IsTaskLike(method.ReturnType))
            return true;

        return method.GetParameters()
            .Any(parameter => IsTaskLike(parameter.ParameterType));
    }

    private static bool IsTaskLike(Type type)
    {
        if (type == typeof(Task) ||
            type == typeof(ValueTask))
        {
            return true;
        }

        if (!type.IsGenericType)
            return false;

        Type definition = type.GetGenericTypeDefinition();
        return definition == typeof(Task<>) ||
            definition == typeof(ValueTask<>);
    }

    private static MockInvocation AssertSingle(object mock)
    {
        var invocations = Snapshot(mock).Invocations;
        Assert.AreEqual(1, invocations.Length);
        return invocations[0];
    }
}
