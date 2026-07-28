namespace AlvorKit.LiveCode;

/// <summary>One structured bridge invocation waiting for execution by the game-thread pump.</summary>
internal sealed class LiveCodePendingBridge(
    string bridge,
    int version,
    JsonElement payload) : LiveCodePendingWork
{
    internal readonly string Bridge = bridge;
    internal readonly int Version = version;
    internal readonly JsonElement Payload = payload;
    internal readonly TaskCompletionSource<LiveCodeBridgeExecutionResult> Completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal override void Cancel(string error) =>
        Completion.TrySetResult(new(
            LiveCodeBridgeExecutionStatus.Failed,
            Bridge,
            Version,
            [],
            [],
            [],
            0,
            error,
            null,
            null));
}
