/// <summary>Runs multi-patch, replacement, inliner, and removal proofs without opening a window.</summary>
internal static class LivePatchProof
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>Proves two simultaneous targets, replacement, inliner repair, and restoration.</summary>
    public static int Run()
    {
        var profiler = InterceptionProfiler.Connect();
        var sceneMethod = Method(nameof(LivePatchTarget.SceneMode));
        var inlineMethod = Method(nameof(LivePatchTarget.InlineMode));
        var sceneTarget = InterceptionTarget.FromMethod(sceneMethod);
        var inlineTarget = InterceptionTarget.FromMethod(inlineMethod);

        Console.WriteLine("ALVORKIT LIVE PATCH MULTI-PATCH PROOF");
        PrintCapabilities(profiler);
        ExactPatchProof.Run(profiler);
        ScopedLivePatchProof.Run(profiler);
        PrintTarget("scene", sceneTarget);
        PrintTarget("inlined", inlineTarget);

        AssertValue("original scene", LivePatchTarget.SceneMode(), 0);
        for (var index = 0; index < 50_000; index++)
            _ = LivePatchTarget.ReadInlineMode();
        AssertValue("original already-inlined caller", LivePatchTarget.ReadInlineMode(), 11);

        var scene = profiler.Install(ConstantInt32Plan(sceneMethod, 1));
        var inlined = profiler.Install(ConstantInt32Plan(inlineMethod, 20));
        PrintSend("install", scene);
        PrintSend("install", inlined);
        PrintCompletion("RECEIVE", WaitFor(profiler, scene.LastRequestId, LivePatchTarget.SceneMode));
        PrintCompletion("RECEIVE", WaitFor(profiler, inlined.LastRequestId, LivePatchTarget.ReadInlineMode));
        AssertValue("patched scene", LivePatchTarget.SceneMode(), 1);
        AssertValue("patched already-inlined caller", LivePatchTarget.ReadInlineMode(), 21);
        Console.WriteLine($"STATE {JsonSerializer.Serialize(profiler.GetState())}");

        var replaceRequest = scene.Replace(ConstantInt32Plan(sceneMethod, 2));
        Console.WriteLine($"SEND replace {{ patch: {scene.PatchId}, request: {replaceRequest}, value: 2 }}");
        PrintCompletion("RECEIVE", WaitFor(profiler, replaceRequest, LivePatchTarget.SceneMode));
        AssertValue("replaced scene", LivePatchTarget.SceneMode(), 2);

        var sceneRemove = scene.Remove();
        var inlineRemove = inlined.Remove();
        Console.WriteLine($"SEND remove {{ patch: {scene.PatchId}, request: {sceneRemove} }}");
        Console.WriteLine($"SEND remove {{ patch: {inlined.PatchId}, request: {inlineRemove} }}");
        PrintCompletion("RECEIVE", WaitFor(profiler, sceneRemove, LivePatchTarget.SceneMode));
        PrintCompletion("RECEIVE", WaitFor(profiler, inlineRemove, LivePatchTarget.ReadInlineMode));
        AssertValue("restored scene", LivePatchTarget.SceneMode(), 0);
        AssertValue("restored already-inlined caller", LivePatchTarget.ReadInlineMode(), 11);
        Console.WriteLine($"STATE {JsonSerializer.Serialize(profiler.GetState())}");
        Console.WriteLine("PROOF PASS");
        return 0;
    }

    internal static void PrintCompletion(
        string direction,
        InterceptionCompletion completion) =>
        Console.WriteLine(
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

    private static void PrintCapabilities(InterceptionProfiler profiler) =>
        Console.WriteLine($"CAPABILITIES {JsonSerializer.Serialize(profiler.Capabilities)}");

    private static void PrintTarget(string name, InterceptionTarget target) =>
        Console.WriteLine(
            $"TARGET {name} {{ mvid: {target.ModuleMvid}, token: 0x{target.MethodToken:X8}, " +
            $"signature: 0x{target.SignatureHash:X16}, method: {target.DisplayName} }}");

    private static void PrintSend(
        string operation,
        IInterceptionPatchHandle handle) =>
        Console.WriteLine(
            $"SEND {operation} {{ patch: {handle.PatchId}, request: {handle.LastRequestId}, " +
            $"mvid: {handle.Target.ModuleMvid}, token: 0x{handle.Target.MethodToken:X8}, " +
            $"signature: 0x{handle.Target.SignatureHash:X16} }}");

    private static InterceptionCompletion WaitFor(
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
        PrintCompletion("TIMEOUT", timedOut);
        throw new TimeoutException(
            $"Interception request {requestId} did not finish within {Timeout}.");
    }

    private static void AssertValue(string label, int actual, int expected)
    {
        Console.WriteLine($"EXECUTE {label} => {actual}");
        if (actual != expected)
            throw new InvalidOperationException($"Expected {label} value {expected}, received {actual}.");
    }
}
