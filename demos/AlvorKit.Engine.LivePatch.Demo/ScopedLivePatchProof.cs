/// <summary>Proves two handlers on the same method selected by authoritative injector ownership.</summary>
internal static class ScopedLivePatchProof
{
    /// <summary>Runs exact-scope, sibling, automatic scope-end, and final native-removal evidence.</summary>
    internal static void Run(InterceptionProfiler profiler)
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector, "proof");
        var emberScope = graph.Scope<ScopedProofScope>(injector, "Ember");
        var tideScope = graph.Scope<ScopedProofScope>(injector, "Tide");
        var ember = emberScope.Get<ScopedOrbitLaw>();
        var tide = tideScope.Get<ScopedOrbitLaw>();
        ember.Bias = 2;
        tide.Bias = 5;
        var emberId = graph.GetId(emberScope);
        var tideId = graph.GetId(tideScope);
        if (!graph.TryGetOwner(ember, out var observedEmber) ||
            !graph.TryGetOwner(tide, out var observedTide) ||
            observedEmber != emberId ||
            observedTide != tideId)
        {
            throw new InvalidOperationException("Injector instance provenance did not identify both sibling scopes.");
        }

        using var session = new LivePatchSession(profiler, graph);
        var method = typeof(ScopedOrbitLaw).GetMethod(
            nameof(ScopedOrbitLaw.Calculate))!;
        using var emberPatch = session.InstallReplace(
            method,
            LivePatchSelector.ExactScope(emberId),
            new EmberOrbitHandler(),
            typeof(EmberOrbitHandler).GetMethod(nameof(EmberOrbitHandler.Invoke))!,
            "ember-fast-orbit");
        using var tidePatch = session.InstallReplace(
            method,
            LivePatchSelector.ExactScope(tideId),
            new TideOrbitHandler(),
            typeof(TideOrbitHandler).GetMethod(nameof(TideOrbitHandler.Invoke))!,
            "tide-reverse-orbit");
        Console.WriteLine(
            $"SEND scoped-install {{ emberPatch: {emberPatch.PatchId}, emberScope: {emberId}, " +
            $"tidePatch: {tidePatch.PatchId}, tideScope: {tideId}, target: {method.DeclaringType!.FullName}.{method.Name} }}");
        PumpUntilActive(session, ember, tide, emberPatch, tidePatch);
        AssertCall("scope-patched ember", ember, 4, 104, 312);
        AssertCall("scope-patched tide", tide, 4, -4, -12);

        graph.End(emberScope);
        AssertCall("ended ember falls through original", ember, 4, 6, 12);
        AssertCall("active tide remains patched", tide, 4, -4, -12);
        Console.WriteLine(
            $"RECEIVE scope-ended {{ patch: {emberPatch.PatchId}, state: {emberPatch.Snapshot().State}, scope: {emberId} }}");

        tidePatch.Dispose();
        PumpUntilRemoved(session, tide, tidePatch.PatchId);
        AssertCall("final scope removal restored original", tide, 4, 9, 18);
        Console.WriteLine(
            $"RECEIVE scoped-remove {{ patch: {tidePatch.PatchId}, state: {session.Get(tidePatch.PatchId).State} }}");

        var exceptionTarget = tideScope.Get<ScopedExceptionLaw>();
        using var exceptionPatch = session.InstallReplace(
            typeof(ScopedExceptionLaw).GetMethod(
                nameof(ScopedExceptionLaw.Calculate))!,
            LivePatchSelector.ExactScope(tideId),
            new ExceptionRegionHandler(),
            typeof(ExceptionRegionHandler).GetMethod(
                nameof(ExceptionRegionHandler.Run))!,
            "exception-region-proof");
        PumpUntil(
            session,
            exceptionPatch.PatchId,
            LivePatchState.Active,
            () => _ = exceptionTarget.Calculate(1));
        var finallyBeforeHit = exceptionTarget.FinallyCount;
        var patchedExceptionResult = exceptionTarget.Calculate(5);
        Console.WriteLine(
            $"EXECUTE exception-region patched => {{ result: {patchedExceptionResult}, finallyCount: {exceptionTarget.FinallyCount} }}");
        if (patchedExceptionResult != 905 ||
            exceptionTarget.FinallyCount != finallyBeforeHit)
        {
            throw new InvalidOperationException(
                "The exact hit did not bypass the original protected body cleanly.");
        }

        exceptionPatch.Dispose();
        PumpUntil(
            session,
            exceptionPatch.PatchId,
            LivePatchState.Removed,
            () => _ = exceptionTarget.Calculate(1));
        var finallyBeforeOriginal = exceptionTarget.FinallyCount;
        var restoredExceptionResult = exceptionTarget.Calculate(5);
        Console.WriteLine(
            $"EXECUTE exception-region restored => {{ result: {restoredExceptionResult}, finallyCount: {exceptionTarget.FinallyCount} }}");
        if (restoredExceptionResult != 7 ||
            exceptionTarget.FinallyCount != finallyBeforeOriginal + 1)
        {
            throw new InvalidOperationException(
                "The original exception region was not preserved after revert.");
        }

        graph.End(tideScope);
    }

    private static void PumpUntilActive(
        LivePatchSession session,
        ScopedOrbitLaw ember,
        ScopedOrbitLaw tide,
        params LivePatchLease[] patches)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            var value = 0;
            _ = ember.Calculate(0, ref value);
            _ = tide.Calculate(0, ref value);
            _ = session.Pump();
            if (patches.All(x => x.Snapshot().State == LivePatchState.Active))
            {
                foreach (var patch in patches)
                    Console.WriteLine($"RECEIVE scoped-active {JsonSerializer.Serialize(patch.Snapshot())}");
                return;
            }

            Thread.Sleep(1);
        }

        throw new TimeoutException("Scoped LivePatch wrappers did not activate.");
    }

    private static void PumpUntilRemoved(
        LivePatchSession session,
        ScopedOrbitLaw trigger,
        ulong patchId)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            var value = 0;
            _ = trigger.Calculate(0, ref value);
            _ = session.Pump();
            if (session.Get(patchId).State == LivePatchState.Removed)
                return;
            Thread.Sleep(1);
        }

        throw new TimeoutException("Scoped LivePatch wrapper did not revert.");
    }

    private static void PumpUntil(
        LivePatchSession session,
        ulong patchId,
        LivePatchState expected,
        Action trigger)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            trigger();
            _ = session.Pump();
            if (session.Get(patchId).State == expected)
                return;
            Thread.Sleep(1);
        }

        throw new TimeoutException(
            $"LivePatch {patchId} did not reach {expected}.");
    }

    private static void AssertCall(
        string label,
        ScopedOrbitLaw target,
        int input,
        int expectedObserved,
        int expectedResult)
    {
        var observed = -1;
        var result = target.Calculate(input, ref observed);
        Console.WriteLine(
            $"EXECUTE {label} => {{ observed: {observed}, result: {result} }}");
        if (observed != expectedObserved || result != expectedResult)
        {
            throw new InvalidOperationException(
                $"Expected {label} {expectedObserved}/{expectedResult}, received {observed}/{result}.");
        }
    }
}

/// <summary>Marks proof services owned by one sibling scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class ScopedProofAttribute : InjectorAttribute;

/// <summary>Proof scope with independent cached receivers.</summary>
[ScopedProof]
internal sealed class ScopedProofScope : InjectorScope<ScopedProofAttribute>;

/// <summary>Ordinary injected method shared by multiple simultaneous scope instances.</summary>
[ScopedProof]
internal sealed class ScopedOrbitLaw
{
    internal int Bias { get; set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Calculate(int value, ref int observed)
    {
        observed = value + Bias;
        return observed * 2;
    }
}

internal sealed class EmberOrbitHandler
{
    [LivePatchHandler]
    public int Invoke(
        ScopedOrbitLaw receiver,
        int value,
        ref int observed)
    {
        _ = receiver;
        observed = value + 100;
        return observed * 3;
    }
}

internal sealed class TideOrbitHandler
{
    [LivePatchHandler]
    public int Invoke(
        ScopedOrbitLaw receiver,
        int value,
        ref int observed)
    {
        _ = receiver;
        observed = -value;
        return observed * 3;
    }
}

/// <summary>Ordinary method whose original IL contains a real finally region.</summary>
[ScopedProof]
internal sealed class ScopedExceptionLaw
{
    internal int FinallyCount { get; private set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Calculate(int value)
    {
        try
        {
            return value + 2;
        }
        finally
        {
            FinallyCount++;
        }
    }
}

internal sealed class ExceptionRegionHandler
{
    [LivePatchHandler]
    public int Run(ScopedExceptionLaw receiver, int value)
    {
        _ = receiver;
        return value + 900;
    }
}
