namespace AlvorKit.LiveCode;

/// <summary>One compiled command waiting for execution by the game-thread pump.</summary>
internal sealed class LiveCodePendingExecution(
    long scopeId,
    string entryType,
    byte[] assembly,
    byte[]? symbols) : LiveCodePendingWork
{
    internal readonly long ScopeId = scopeId;
    internal readonly string EntryType = entryType;
    internal readonly byte[] Assembly = assembly;
    internal readonly byte[]? Symbols = symbols;
    internal readonly TaskCompletionSource<LiveCodeExecutionResult> Completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal override void Cancel(string error) =>
        Completion.TrySetResult(new(
            LiveCodeExecutionStatus.Failed,
            ScopeId,
            [],
            [],
            0,
            error,
            null,
            null));
}
