/// <summary>Runs multi-patch, replacement, inliner, and removal proofs without opening a window.</summary>
internal static class LivePatchProof
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>Proves two simultaneous targets, replacement, inliner repair, and restoration.</summary>
    public static int Run(Log log)
    {
        var profiler = InterceptionProfiler.Connect();
        var sceneMethod = Method(nameof(LivePatchTarget.SceneMode));
        var inlineMethod = Method(nameof(LivePatchTarget.InlineMode));
        var sceneTarget = InterceptionTarget.FromMethod(sceneMethod);
        var inlineTarget = InterceptionTarget.FromMethod(inlineMethod);

        log.Raw("ALVORKIT LIVE PATCH MULTI-PATCH PROOF");
        PrintCapabilities(log, profiler);
        ExactPatchProof.Run(log, profiler);
        ScopedLivePatchProof.Run(log, profiler);
        PrintTarget(log, "scene", sceneTarget);
        PrintTarget(log, "inlined", inlineTarget);

        AssertValue(log, "original scene", LivePatchTarget.SceneMode(), 0);
        for (var index = 0; index < 50_000; index++)
            _ = LivePatchTarget.ReadInlineMode();
        AssertValue(log, "original already-inlined caller", LivePatchTarget.ReadInlineMode(), 11);

        var scene = profiler.Install(ConstantInt32Plan(sceneMethod, 1));
        var inlined = profiler.Install(ConstantInt32Plan(inlineMethod, 20));
        PrintSend(log, "install", scene);
        PrintSend(log, "install", inlined);
        PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, scene.LastRequestId, LivePatchTarget.SceneMode));
        PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, inlined.LastRequestId, LivePatchTarget.ReadInlineMode));
        AssertValue(log, "patched scene", LivePatchTarget.SceneMode(), 1);
        AssertValue(log, "patched already-inlined caller", LivePatchTarget.ReadInlineMode(), 21);
        log.Raw($"STATE {JsonSerializer.Serialize(profiler.GetState())}");

        var replaceRequest = scene.Replace(ConstantInt32Plan(sceneMethod, 2));
        log.Raw($"SEND replace {{ patch: {scene.PatchId}, request: {replaceRequest}, value: 2 }}");
        PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, replaceRequest, LivePatchTarget.SceneMode));
        AssertValue(log, "replaced scene", LivePatchTarget.SceneMode(), 2);

        var sceneRemove = scene.Remove();
        var inlineRemove = inlined.Remove();
        log.Raw($"SEND remove {{ patch: {scene.PatchId}, request: {sceneRemove} }}");
        log.Raw($"SEND remove {{ patch: {inlined.PatchId}, request: {inlineRemove} }}");
        PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, sceneRemove, LivePatchTarget.SceneMode));
        PrintCompletion(log, "RECEIVE", WaitFor(log, profiler, inlineRemove, LivePatchTarget.ReadInlineMode));
        AssertValue(log, "restored scene", LivePatchTarget.SceneMode(), 0);
        AssertValue(log, "restored already-inlined caller", LivePatchTarget.ReadInlineMode(), 11);
        log.Raw($"STATE {JsonSerializer.Serialize(profiler.GetState())}");
        log.Raw("PROOF PASS");
        return 0;
    }

    internal static void PrintCompletion(
        Log log,
        string direction,
        InterceptionCompletion completion) =>
        log.Raw(
            $"{direction} {{ request: {completion.RequestId}, patch: {completion.PatchId}, " +
            $"operation: {completion.Operation}, state: {completion.State}, hresult: 0x{completion.HResult:X8}, " +
            $"target: {{ mvid: {completion.Target.ModuleMvid}, token: 0x{completion.Target.MethodToken:X8}, " +
            $"signature: 0x{completion.Target.SignatureHash:X16} }}, elapsedUs: {completion.Elapsed.TotalMicroseconds:F0}, " +
            $"callbacks: {{ started: {completion.RejitStartedCallbacks}, parameters: {completion.ParameterCallbacks}, " +
            $"finished: {completion.RejitFinishedCallbacks}, errors: {completion.RejitErrorCallbacks} }} }}");

    private static MethodInfo Method(string name) =>
        typeof(LivePatchTarget).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Static)
        ?? throw new MissingMethodException(typeof(LivePatchTarget).FullName, name);

    internal static InterceptionPlan ConstantInt32Plan(
        MethodInfo method,
        int value)
    {
        Span<byte> body = stackalloc byte[7];
        body[0] = (6 << 2) | 0x02;
        body[1] = 0x20;
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(
            body[2..6],
            value);
        body[6] = 0x2A;
        return new(
            InterceptionTarget.FromMethod(method),
            InterceptionMethodBody.FromRaw(body));
    }

    private static void PrintCapabilities(Log log, InterceptionProfiler profiler) =>
        log.Raw($"CAPABILITIES {JsonSerializer.Serialize(profiler.Capabilities)}");

    private static void PrintTarget(Log log, string name, InterceptionTarget target) =>
        log.Raw(
            $"TARGET {name} {{ mvid: {target.ModuleMvid}, token: 0x{target.MethodToken:X8}, " +
            $"signature: 0x{target.SignatureHash:X16}, method: {target.DisplayName} }}");

    private static void PrintSend(
        Log log,
        string operation,
        IInterceptionPatchHandle handle) =>
        log.Raw(
            $"SEND {operation} {{ patch: {handle.PatchId}, request: {handle.LastRequestId}, " +
            $"mvid: {handle.Target.ModuleMvid}, token: 0x{handle.Target.MethodToken:X8}, " +
            $"signature: 0x{handle.Target.SignatureHash:X16} }}");

    private static InterceptionCompletion WaitFor(
        Log log,
        InterceptionProfiler profiler,
        ulong requestId,
        Func<int> trigger)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < Timeout)
        {
            _ = trigger();
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1));
        }

        var timedOut = profiler.GetCompletion(requestId);
        PrintCompletion(log, "TIMEOUT", timedOut);
        throw new TimeoutException(
            $"Interception request {requestId} did not finish within {Timeout}.");
    }

    private static void AssertValue(Log log, string label, int actual, int expected)
    {
        log.Raw($"EXECUTE {label} => {actual}");
        if (actual != expected)
            throw new InvalidOperationException($"Expected {label} value {expected}, received {actual}.");
    }
}
