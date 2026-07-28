namespace AlvorKit.Interception.CallerProof.Test;

[TestClass]
public sealed class CallerBridgeProfilerTest
{
    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(10);

    [TestMethod]
    public void PlainCallerHasNoProfilerDependency()
    {
        CallerBridgeTarget.RoutePointer = 0;
        Assert.AreEqual(4, CallerBridgeTarget.Caller(3));
    }

    [TestMethod]
    public void RejittedCallerUsesInertOriginalAndExactCalliPaths()
    {
        RequireProfiledHost();

        var caller = typeof(CallerBridgeTarget).GetMethod(
            nameof(CallerBridgeTarget.Caller),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var template = typeof(CallerBridgeTarget).GetMethod(
            nameof(CallerBridgeTarget.RoutedTemplate),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var replacement = typeof(CallerBridgeTarget).GetMethod(
            nameof(CallerBridgeTarget.Replacement),
            BindingFlags.NonPublic | BindingFlags.Static)!;

        RuntimeHelpers.PrepareMethod(replacement.MethodHandle);
        CallerBridgeTarget.RoutePointer = 0;
        Assert.AreEqual(4, CallerBridgeTarget.Caller(3));

        var profiler = InterceptionProfiler.Connect();
        using var patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(template)));
        var install = WaitFor(
            profiler,
            patch.LastRequestId,
            static () => CallerBridgeTarget.Caller(3));
        Assert.AreEqual(InterceptionState.Active, install.State);
        Assert.IsTrue(install.ParameterCallbacks >= 1);

        Assert.AreEqual(
            4,
            CallerBridgeTarget.Caller(3),
            "An inert route must execute the original operation.");

        CallerBridgeTarget.RoutePointer =
            replacement.MethodHandle.GetFunctionPointer();
        Assert.AreEqual(
            30,
            CallerBridgeTarget.Caller(3),
            "An active route must use the exact managed calli target.");

        CallerBridgeTarget.RoutePointer = 0;
        Assert.AreEqual(
            4,
            CallerBridgeTarget.Caller(3),
            "Retiring the managed route must not require another ReJIT.");

        var removeRequest = patch.Remove();
        var removal = WaitFor(
            profiler,
            removeRequest,
            static () => CallerBridgeTarget.Caller(3));
        Assert.AreEqual(InterceptionState.Removed, removal.State);
        Assert.AreEqual(4, CallerBridgeTarget.Caller(3));
    }

    [TestMethod]
    public void WarmInertAndActiveCallerRoutesAllocateNothing()
    {
        RequireProfiledHost();
        CallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(CallerBridgeTarget),
            nameof(CallerBridgeTarget.Caller),
            nameof(CallerBridgeTarget.RoutedTemplate),
            static () => _ = CallerBridgeTarget.Caller(3));

        _ = MeasureCallerBridgeAllocations();
        Assert.AreEqual(0L, MeasureCallerBridgeAllocations());

        CallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(CallerBridgeTarget),
            nameof(CallerBridgeTarget.Replacement));
        _ = MeasureCallerBridgeAllocations();
        Assert.AreEqual(0L, MeasureCallerBridgeAllocations());

        CallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () => _ = CallerBridgeTarget.Caller(3));
    }

    [TestMethod]
    public void RejittedCallerPreservesRefOutAndWideReturn()
    {
        RequireProfiledHost();
        RefOutCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(RefOutCallerBridgeTarget),
            nameof(RefOutCallerBridgeTarget.Caller),
            nameof(RefOutCallerBridgeTarget.RoutedTemplate),
            static () =>
            {
                var value = 1;
                _ = RefOutCallerBridgeTarget.Caller(ref value, out _);
            });

        var originalValue = 3;
        var originalResult = RefOutCallerBridgeTarget.Caller(
            ref originalValue,
            out var originalObserved);
        Assert.AreEqual(5, originalValue);
        Assert.AreEqual(5, originalObserved);
        Assert.AreEqual(10_000_000_005L, originalResult);

        RefOutCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(RefOutCallerBridgeTarget),
            nameof(RefOutCallerBridgeTarget.Replacement));
        var replacedValue = 3;
        var replacedResult = RefOutCallerBridgeTarget.Caller(
            ref replacedValue,
            out var replacedObserved);
        Assert.AreEqual(8, replacedValue);
        Assert.AreEqual(-8, replacedObserved);
        Assert.AreEqual(20_000_000_008L, replacedResult);

        RefOutCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () =>
            {
                var value = 1;
                _ = RefOutCallerBridgeTarget.Caller(ref value, out _);
            });
    }

    [TestMethod]
    public void RejittedCallerPreservesVoidSignature()
    {
        RequireProfiledHost();
        VoidCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(VoidCallerBridgeTarget),
            nameof(VoidCallerBridgeTarget.Caller),
            nameof(VoidCallerBridgeTarget.RoutedTemplate),
            static () =>
            {
                var value = 0;
                VoidCallerBridgeTarget.Caller(ref value);
            });

        var original = 2;
        VoidCallerBridgeTarget.Caller(ref original);
        Assert.AreEqual(3, original);

        VoidCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(VoidCallerBridgeTarget),
            nameof(VoidCallerBridgeTarget.Replacement));
        var replaced = 2;
        VoidCallerBridgeTarget.Caller(ref replaced);
        Assert.AreEqual(12, replaced);

        VoidCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () =>
            {
                var value = 0;
                VoidCallerBridgeTarget.Caller(ref value);
            });
    }

    [TestMethod]
    public void RejittedCallerPreservesRefStructIngress()
    {
        RequireProfiledHost();
        SpanCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(SpanCallerBridgeTarget),
            nameof(SpanCallerBridgeTarget.Caller),
            nameof(SpanCallerBridgeTarget.RoutedTemplate),
            static () =>
            {
                Span<int> values = stackalloc int[1];
                _ = SpanCallerBridgeTarget.Caller(values);
            });

        Span<int> original = [3];
        Assert.AreEqual(4, SpanCallerBridgeTarget.Caller(original));
        Assert.AreEqual(4, original[0]);

        SpanCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(SpanCallerBridgeTarget),
            nameof(SpanCallerBridgeTarget.Replacement));
        Span<int> replaced = [3];
        Assert.AreEqual(13, SpanCallerBridgeTarget.Caller(replaced));
        Assert.AreEqual(13, replaced[0]);

        SpanCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () =>
            {
                Span<int> values = stackalloc int[1];
                _ = SpanCallerBridgeTarget.Caller(values);
            });
    }

    [TestMethod]
    public void RejittedCallerPreservesReadonlyByrefs()
    {
        RequireProfiledHost();
        ReadonlyCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(ReadonlyCallerBridgeTarget),
            nameof(ReadonlyCallerBridgeTarget.Caller),
            nameof(ReadonlyCallerBridgeTarget.RoutedTemplate),
            static () =>
            {
                var receiver = new ReadonlyCallerBridgeValue(1);
                var delta = 2;
                _ = ReadonlyCallerBridgeTarget.Caller(
                    in receiver,
                    in delta);
            });

        var originalReceiver = new ReadonlyCallerBridgeValue(3);
        var originalDelta = 4;
        Assert.AreEqual(
            7,
            ReadonlyCallerBridgeTarget.Caller(
                in originalReceiver,
                in originalDelta));

        ReadonlyCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(ReadonlyCallerBridgeTarget),
            nameof(ReadonlyCallerBridgeTarget.Replacement));
        Assert.AreEqual(
            34,
            ReadonlyCallerBridgeTarget.Caller(
                in originalReceiver,
                in originalDelta));

        ReadonlyCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            () => _ = ReadonlyCallerBridgeTarget.Caller(
                in originalReceiver,
                in originalDelta));
    }

    [TestMethod]
    public void RejittedCallerPreservesLiveStructAndExceptions()
    {
        RequireProfiledHost();
        StructCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(StructCallerBridgeTarget),
            nameof(StructCallerBridgeTarget.Caller),
            nameof(StructCallerBridgeTarget.RoutedTemplate),
            static () =>
            {
                var counter = new CallerBridgeCounter();
                _ = StructCallerBridgeTarget.Caller(ref counter, 1);
            });

        var original = new CallerBridgeCounter { Value = 2 };
        Assert.AreEqual(
            5,
            StructCallerBridgeTarget.Caller(ref original, 3));
        Assert.AreEqual(5, original.Value);

        StructCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(StructCallerBridgeTarget),
            nameof(StructCallerBridgeTarget.Replacement));
        var replaced = new CallerBridgeCounter { Value = 2 };
        Assert.AreEqual(
            32,
            StructCallerBridgeTarget.Caller(ref replaced, 3));
        Assert.AreEqual(32, replaced.Value);
        Assert.ThrowsExactly<CallerBridgeException>(
            () => StructCallerBridgeTarget.Caller(ref replaced, -1));

        StructCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () =>
            {
                var counter = new CallerBridgeCounter();
                _ = StructCallerBridgeTarget.Caller(ref counter, 1);
            });
    }

    [TestMethod]
    public void RejittedCallerPreservesManagedReferenceAlias()
    {
        RequireProfiledHost();
        RefReturnCallerBridgeTarget.RoutePointer = 0;
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(RefReturnCallerBridgeTarget),
            nameof(RefReturnCallerBridgeTarget.Caller),
            nameof(RefReturnCallerBridgeTarget.RoutedTemplate),
            static () => _ = RefReturnCallerBridgeTarget.Caller());

        ref var original = ref RefReturnCallerBridgeTarget.Caller();
        original = 17;
        Assert.AreEqual(17, RefReturnCallerBridgeTarget.OriginalStorage);
        Assert.AreEqual(11, RefReturnCallerBridgeTarget.ReplacementStorage);

        RefReturnCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(RefReturnCallerBridgeTarget),
            nameof(RefReturnCallerBridgeTarget.Replacement));
        ref var replacement = ref RefReturnCallerBridgeTarget.Caller();
        replacement = 23;
        Assert.AreEqual(17, RefReturnCallerBridgeTarget.OriginalStorage);
        Assert.AreEqual(23, RefReturnCallerBridgeTarget.ReplacementStorage);

        RefReturnCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () => _ = RefReturnCallerBridgeTarget.Caller());
    }

    [TestMethod]
    public void RejittedGenericCallerUsesConstructionSpecificRoutes()
    {
        RequireProfiledHost();
        GenericCallerRoute<string>.Pointer = 0;
        GenericCallerRoute<object>.Pointer = 0;
        GenericCallerRoute<int>.Pointer = 0;
        var profiler = InterceptionProfiler.Connect();
        var callerDefinition = Method(
            typeof(GenericCallerBridgeTarget),
            nameof(GenericCallerBridgeTarget.Caller));
        var templateDefinition = Method(
            typeof(GenericCallerBridgeTarget),
            nameof(GenericCallerBridgeTarget.RoutedTemplate));
        var caller = callerDefinition.MakeGenericMethod(typeof(string));
        var template = templateDefinition.MakeGenericMethod(typeof(string));
        using var patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(template)));
        var install = WaitFor(
            profiler,
            patch.LastRequestId,
            static () => _ = GenericCallerBridgeTarget.Caller("warm"));
        Assert.AreEqual(InterceptionState.Active, install.State);

        GenericCallerRoute<string>.Pointer = FunctionPointer(
            typeof(GenericCallerBridgeTarget),
            nameof(GenericCallerBridgeTarget.ReplaceString));
        GenericCallerRoute<object>.Pointer = FunctionPointer(
            typeof(GenericCallerBridgeTarget),
            nameof(GenericCallerBridgeTarget.ReplaceObject));
        GenericCallerRoute<int>.Pointer = FunctionPointer(
            typeof(GenericCallerBridgeTarget),
            nameof(GenericCallerBridgeTarget.ReplaceInt32));

        Assert.AreEqual(
            "value:string",
            GenericCallerBridgeTarget.Caller("value"));
        Assert.AreSame(
            GenericCallerBridgeTarget.ObjectSentinel,
            GenericCallerBridgeTarget.Caller(
                GenericCallerBridgeTarget.StringSentinel));
        Assert.AreEqual(
            103,
            GenericCallerBridgeTarget.Caller(3),
            "A value-type construction first used after activation needs its own route.");

        GenericCallerRoute<string>.Pointer = 0;
        GenericCallerRoute<object>.Pointer = 0;
        GenericCallerRoute<int>.Pointer = 0;
        Remove(
            profiler,
            patch,
            static () => _ = GenericCallerBridgeTarget.Caller("warm"));
        Assert.AreEqual("value", GenericCallerBridgeTarget.Caller("value"));
        Assert.AreEqual(3, GenericCallerBridgeTarget.Caller(3));
    }

    [TestMethod]
    public void ActiveWrapperCanChoosePrivateOriginalAfterMatching()
    {
        RequireProfiledHost();
        MatchingCallerBridgeTarget.RoutePointer = 0;
        MatchingCallerBridgeTarget.OriginalPointer = FunctionPointer(
            typeof(MatchingCallerBridgeTarget),
            "PrivateOriginal");
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(MatchingCallerBridgeTarget),
            nameof(MatchingCallerBridgeTarget.Caller),
            nameof(MatchingCallerBridgeTarget.RoutedTemplate),
            static () => _ = MatchingCallerBridgeTarget.Caller(1));

        MatchingCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(MatchingCallerBridgeTarget),
            nameof(MatchingCallerBridgeTarget.MatchingWrapper));
        Assert.AreEqual(
            30,
            MatchingCallerBridgeTarget.Caller(3),
            "A handled match must use the active wrapper result.");
        Assert.AreEqual(
            5,
            MatchingCallerBridgeTarget.Caller(4),
            "An unmatched active wrapper must invoke its exact original delegate.");
        var thrown = Assert.ThrowsExactly<OriginalCallerBridgeException>(
            () => MatchingCallerBridgeTarget.Caller(-2));
        Assert.AreSame(OriginalCallerBridgeException.Instance, thrown);

        MatchingCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () => _ = MatchingCallerBridgeTarget.Caller(1));
    }

    [TestMethod]
    public void ActiveWrapperPreservesCallvirtNullCheckAndOriginalFallback()
    {
        RequireProfiledHost();
        CallvirtCallerBridgeTarget.RoutePointer = 0;
        CallvirtCallerBridgeTarget.OriginalPointer = FunctionPointer(
            typeof(CallvirtCallerBridgeTarget),
            nameof(CallvirtCallerBridgeTarget.OriginalBridge));
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(CallvirtCallerBridgeTarget),
            nameof(CallvirtCallerBridgeTarget.Caller),
            nameof(CallvirtCallerBridgeTarget.RoutedTemplate),
            static () => _ = CallvirtCallerBridgeTarget.Caller(
                new(),
                1));

        CallvirtCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(CallvirtCallerBridgeTarget),
            nameof(CallvirtCallerBridgeTarget.MatchingWrapper));
        var receiver = new CallvirtCallerBridgeReceiver();
        Assert.AreEqual(
            30,
            CallvirtCallerBridgeTarget.Caller(receiver, 3));
        Assert.AreEqual(
            5,
            CallvirtCallerBridgeTarget.Caller(receiver, 4));
        Assert.ThrowsExactly<NullReferenceException>(
            () => CallvirtCallerBridgeTarget.Caller(null!, 4));

        CallvirtCallerBridgeTarget.RoutePointer = 0;
        Remove(
            profiler,
            patch,
            static () => _ = CallvirtCallerBridgeTarget.Caller(
                new(),
                1));
    }

    [TestMethod]
    public async Task RemovalPreservesAnInFlightRoutedInvocation()
    {
        RequireProfiledHost();
        BlockingCallerBridgeTarget.Reset();
        var profiler = InterceptionProfiler.Connect();
        using var patch = Install(
            profiler,
            typeof(BlockingCallerBridgeTarget),
            nameof(BlockingCallerBridgeTarget.Caller),
            nameof(BlockingCallerBridgeTarget.RoutedTemplate),
            static () => _ = BlockingCallerBridgeTarget.Caller(1));

        BlockingCallerBridgeTarget.RoutePointer = FunctionPointer(
            typeof(BlockingCallerBridgeTarget),
            nameof(BlockingCallerBridgeTarget.BlockingReplacement));
        Task<int> invocation = Task.Run(
            static () => BlockingCallerBridgeTarget.Caller(3));
        Assert.IsTrue(
            BlockingCallerBridgeTarget.WaitUntilEntered(RequestTimeout),
            "The routed invocation did not enter its managed handler.");

        BlockingCallerBridgeTarget.RoutePointer = 0;
        var removeRequest = patch.Remove();
        var removal = WaitFor(
            profiler,
            removeRequest,
            static () => _ = BlockingCallerBridgeTarget.Caller(1));
        Assert.AreEqual(InterceptionState.Removed, removal.State);

        BlockingCallerBridgeTarget.ReleaseInvocation();
        Assert.AreEqual(30, await invocation);
        Assert.AreEqual(4, BlockingCallerBridgeTarget.Caller(3));
    }

    private static IInterceptionPatchHandle Install(
        IInterceptionBackend profiler,
        Type targetType,
        string callerName,
        string templateName,
        Action trigger)
    {
        var caller = Method(targetType, callerName);
        var template = Method(targetType, templateName);
        var patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(template)));
        var install = WaitFor(
            profiler,
            patch.LastRequestId,
            trigger);
        Assert.AreEqual(InterceptionState.Active, install.State);
        Assert.IsTrue(install.ParameterCallbacks >= 1);
        return patch;
    }

    private static void Remove(
        IInterceptionBackend profiler,
        IInterceptionPatchHandle patch,
        Action trigger)
    {
        var request = patch.Remove();
        var removal = WaitFor(profiler, request, trigger);
        Assert.AreEqual(InterceptionState.Removed, removal.State);
    }

    private static nint FunctionPointer(Type type, string name)
    {
        var method = Method(type, name);
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    private static long MeasureCallerBridgeAllocations()
    {
        var sum = 0;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10_000; index++)
            sum += CallerBridgeTarget.Caller(index);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(sum);
        return allocated;
    }

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void RequireProfiledHost()
    {
        if (Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable) is null)
        {
            Assert.Inconclusive(
                "Run through AlvorKit.Script.TestInterception.");
        }
    }

    private static InterceptionCompletion WaitFor(
        IInterceptionBackend profiler,
        ulong requestId,
        Action trigger)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < RequestTimeout)
        {
            trigger();
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1));
        }

        var timedOut = profiler.GetCompletion(requestId);
        Assert.Fail(
            $"Request {requestId} timed out in {timedOut.State}; " +
            $"started={timedOut.RejitStartedCallbacks}, " +
            $"parameters={timedOut.ParameterCallbacks}, " +
            $"finished={timedOut.RejitFinishedCallbacks}, " +
            $"errors={timedOut.RejitErrorCallbacks}, " +
            $"hresult=0x{timedOut.HResult:X8}.");
        return default;
    }
}
