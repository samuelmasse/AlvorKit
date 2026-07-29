/// <summary>Proves exact arguments, reference receivers, scope-style selection, and original fallback.</summary>
internal static class ExactPatchProof
{
    private const ulong SlotId = 7001;
    private static ExactPatchTarget? selected;
    private static IInterceptionHandlerTrampoline? trampoline;

    /// <summary>Installs one exact handler while a sibling receiver continues through original IL.</summary>
    internal static void Run(Log log, InterceptionProfiler profiler)
    {
        var method = typeof(ExactPatchTarget).GetMethod(
            nameof(ExactPatchTarget.Calculate))!;
        var methodBody = method.GetMethodBody()!;
        log.Raw(
            $"TARGET IL {{ bytes: {methodBody.GetILAsByteArray()!.Length}, maxStack: {methodBody.MaxStackSize}, " +
            $"locals: {methodBody.LocalVariables.Count}, initLocals: {methodBody.InitLocals}, " +
            $"signature: {Convert.ToHexString(method.Module.ResolveSignature(method.MetadataToken))} }}");
        var ember = new ExactPatchTarget(2);
        var tide = new ExactPatchTarget(5);
        AssertCall(log, "exact original ember", ember, 4, 6, 12);
        AssertCall(log, "exact original tide", tide, 4, 9, 18);

        var handler = new FasterExactOrbit();
        trampoline = ((IInterceptionBackend)profiler).CreateHandlerTrampoline(
            method,
            handler,
            typeof(FasterExactOrbit).GetMethod(
                nameof(FasterExactOrbit.Invoke))!,
            InterceptionHandlerExceptionPolicy.Propagate);
        selected = ember;
        var resolver = typeof(ExactPatchProof).GetMethod(
            nameof(Resolve),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(resolver.MethodHandle);
        var plan = InterceptionDispatchPlan.ForMethod(
            method,
            SlotId,
            resolver.MethodHandle.GetFunctionPointer());
        var patch = profiler.Install(plan);
        log.Raw(
            $"SEND install-exact {{ patch: {patch.PatchId}, request: {patch.LastRequestId}, slot: {SlotId}, " +
            $"target: {plan.Target.DisplayName}, signature: 0x{plan.Target.SignatureHash:X16} }}");
        var completion = WaitFor(log, profiler, patch.LastRequestId, ember);
        LivePatchProof.PrintCompletion(log, "RECEIVE", completion);

        AssertCall(log, "exact patched ember", ember, 4, 104, 312);
        AssertCall(log, "exact original sibling tide", tide, 4, 9, 18);

        selected = null;
        var acquired = trampoline;
        trampoline = null;
        acquired.Dispose();
        AssertCall(log, "exact immediate managed removal", ember, 4, 6, 12);

        var remove = patch.Remove();
        log.Raw($"SEND remove-exact {{ patch: {patch.PatchId}, request: {remove} }}");
        LivePatchProof.PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, remove, ember));
        AssertCall(log, "exact reverted ember", ember, 4, 6, 12);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint Resolve(ulong slotId, object? receiver)
    {
        var current = Volatile.Read(ref trampoline);
        return slotId == SlotId &&
            ReferenceEquals(Volatile.Read(ref selected), receiver) &&
            current is not null &&
            current.TryAcquire(out var entryPoint)
                ? entryPoint
                : 0;
    }

    private static InterceptionCompletion WaitFor(
        Log log,
        InterceptionProfiler profiler,
        ulong requestId,
        ExactPatchTarget trigger)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            var value = 1;
            _ = trigger.Calculate(1, ref value);
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                if (completion.State == InterceptionState.Failed)
                    LivePatchProof.PrintCompletion(log, "RECEIVE FAILED", completion);
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(1);
        }

        throw new TimeoutException($"Exact interception request {requestId} timed out.");
    }

    private static void AssertCall(
        Log log,
        string label,
        ExactPatchTarget target,
        int input,
        int expectedObserved,
        int expectedResult)
    {
        var observed = -1;
        var result = target.Calculate(input, ref observed);
        log.Raw(
            $"EXECUTE {label} => {{ observed: {observed}, result: {result} }}");
        if (observed != expectedObserved || result != expectedResult)
        {
            throw new InvalidOperationException(
                $"Expected {label} observed/result {expectedObserved}/{expectedResult}, " +
                $"received {observed}/{result}.");
        }
    }
}

/// <summary>Ordinary unannotated instance target with a real argument and ref write-back.</summary>
internal sealed class ExactPatchTarget(int bias)
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Calculate(int value, ref int observed)
    {
        observed = value + bias;
        return observed * 2;
    }
}

/// <summary>Submitted-handler-shaped exact behavior with an explicit receiver and declared arguments.</summary>
internal sealed class FasterExactOrbit
{
    public int Invoke(
        ExactPatchTarget receiver,
        int value,
        ref int observed)
    {
        _ = receiver;
        observed = value + 100;
        return observed * 3;
    }
}
