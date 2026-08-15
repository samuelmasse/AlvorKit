namespace AlvorKit;

/// <summary>Measures direct and profiler-routed caller costs without activating a mocking adapter.</summary>
internal static class InterceptionPerformanceFixture
{
    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(10);

    /// <summary>Runs one cold patch lifecycle and the warm direct, inert, and exact route cases.</summary>
    internal static InterceptionPerformanceReport Run()
    {
        RequireProfiledHost();
        InterceptionPerformanceTarget.RoutePointer = 0;
        EnsureResult(4, InterceptionPerformanceTarget.Caller(3), "direct");
        InterceptionPerformanceMeasurement.WarmCaller();
        _ = GC.GetAllocatedBytesForCurrentThread();
        var direct = InterceptionPerformanceMeasurement.MeasureWarm(
            "warm-direct");

        var profiler = InterceptionProfiler.Connect();
        var caller = Method(nameof(InterceptionPerformanceTarget.Caller));
        var template = Method(
            nameof(InterceptionPerformanceTarget.RoutedTemplate));
        var replacement = Method(
            nameof(InterceptionPerformanceTarget.Replacement));
        var swappedReplacement = Method(
            nameof(InterceptionPerformanceTarget.SwappedReplacement));
        RuntimeHelpers.PrepareMethod(replacement.MethodHandle);
        RuntimeHelpers.PrepareMethod(swappedReplacement.MethodHandle);

        var installClock = Stopwatch.StartNew();
        using var patch = profiler.Install(
            new InterceptionPlan(
                InterceptionTarget.FromMethod(caller),
                ReflectionMethodBodyEncoder.Read(template)));
        var install = WaitFor(profiler, patch.LastRequestId);
        installClock.Stop();
        var coldInstall = Cold(
            "cold-install",
            installClock.Elapsed,
            install);

        EnsureResult(4, InterceptionPerformanceTarget.Caller(3), "inert");
        InterceptionPerformanceMeasurement.WarmCaller();
        var inert = InterceptionPerformanceMeasurement.MeasureWarm(
            "warm-inert-route");

        InterceptionPerformanceTarget.RoutePointer =
            replacement.MethodHandle.GetFunctionPointer();
        EnsureResult(30, InterceptionPerformanceTarget.Caller(3), "active");
        InterceptionPerformanceMeasurement.WarmCaller();
        var active = InterceptionPerformanceMeasurement.MeasureWarm(
            "warm-active-exact");

        InterceptionBackendState beforeSwap = profiler.GetState();
        InterceptionPerformanceTarget.RoutePointer =
            swappedReplacement.MethodHandle.GetFunctionPointer();
        EnsureResult(300, InterceptionPerformanceTarget.Caller(3), "swapped");
        InterceptionBackendState afterSwap = profiler.GetState();
        var handlerSwap = HandlerSwap(beforeSwap, afterSwap);
        InterceptionPerformanceMeasurement.WarmCaller();
        var swapped = InterceptionPerformanceMeasurement.MeasureWarm(
            "warm-swapped-exact");

        InterceptionPerformanceTarget.RoutePointer = 0;
        var removeClock = Stopwatch.StartNew();
        var removeRequest = patch.Remove();
        var removal = WaitFor(profiler, removeRequest);
        removeClock.Stop();
        var coldRemove = Cold(
            "cold-remove",
            removeClock.Elapsed,
            removal);
        EnsureResult(4, InterceptionPerformanceTarget.Caller(3), "removed");

        return new(
            "alvorkit-interception-performance-v2",
            DateTimeOffset.UtcNow,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture.ToString(),
            InterceptionPerformanceMeasurement.TierWarmupIterations,
            InterceptionPerformanceMeasurement.TimedIterations,
            InterceptionPerformanceMeasurement.TimedSamples,
            InterceptionPerformanceMeasurement.AllocationIterations,
            coldInstall,
            direct,
            inert,
            active,
            swapped,
            handlerSwap,
            coldRemove);
    }

    /// <summary>Fails when either warmed routed path allocates on the current thread.</summary>
    internal static void AssertAllocationInvariants(
        InterceptionPerformanceReport report)
    {
        if (report.WarmInertRoute.AllocatedBytes != 0 ||
            report.WarmActiveExact.AllocatedBytes != 0 ||
            report.WarmSwappedExact.AllocatedBytes != 0)
        {
            throw new InvalidOperationException(
                "Warm routed calls must allocate 0 B: " +
                $"inert={report.WarmInertRoute.AllocatedBytes} B, " +
                $"active={report.WarmActiveExact.AllocatedBytes} B, " +
                $"swapped={report.WarmSwappedExact.AllocatedBytes} B.");
        }
    }

    private static HandlerSwapInterceptionEvidence HandlerSwap(
        InterceptionBackendState before,
        InterceptionBackendState after)
    {
        if (after.LastRequestId != before.LastRequestId ||
            after.PendingRequests != before.PendingRequests ||
            after.ActivePatches != before.ActivePatches)
        {
            throw new InvalidOperationException(
                "A managed handler swap must not enqueue profiler work or " +
                "change native patch ownership.");
        }

        return new(
            before.LastRequestId,
            after.LastRequestId,
            before.PendingRequests,
            after.PendingRequests,
            before.ActivePatches,
            after.ActivePatches);
    }

    private static ColdInterceptionMeasurement Cold(
        string name,
        TimeSpan wall,
        InterceptionCompletion completion) =>
        new(
            name,
            wall.TotalMilliseconds,
            completion.Elapsed.TotalMilliseconds,
            completion.RejitStartedCallbacks,
            completion.ParameterCallbacks,
            completion.RejitFinishedCallbacks);

    private static MethodInfo Method(string name) =>
        typeof(InterceptionPerformanceTarget).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void RequireProfiledHost()
    {
        if (Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable) is null)
        {
            throw new InvalidOperationException(
                "Run through AlvorKit.Script.TestInterception --exec-project.");
        }
    }

    private static InterceptionCompletion WaitFor(
        InterceptionProfiler profiler,
        ulong requestId)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < RequestTimeout)
        {
            _ = InterceptionPerformanceTarget.Caller(1);
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1));
        }

        var timedOut = profiler.GetCompletion(requestId);
        throw new TimeoutException(
            $"Request {requestId} timed out in {timedOut.State}; " +
            $"started={timedOut.RejitStartedCallbacks}, " +
            $"parameters={timedOut.ParameterCallbacks}, " +
            $"finished={timedOut.RejitFinishedCallbacks}, " +
            $"errors={timedOut.RejitErrorCallbacks}.");
    }

    private static void EnsureResult(
        int expected,
        int actual,
        string route)
    {
        if (actual != expected)
        {
            throw new InvalidOperationException(
                $"The {route} route returned {actual}; expected {expected}.");
        }
    }
}
