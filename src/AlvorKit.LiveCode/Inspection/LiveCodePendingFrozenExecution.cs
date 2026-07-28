namespace AlvorKit.LiveCode;

/// <summary>One ordinary scoped command waiting for the dedicated frozen-game execution thread.</summary>
internal sealed class LiveCodePendingFrozenExecution(
    long scopeId,
    string entryType,
    byte[] assembly,
    byte[]? symbols)
{
    internal readonly long ScopeId = scopeId;
    internal readonly string EntryType = entryType;
    internal readonly byte[] Assembly = assembly;
    internal readonly byte[]? Symbols = symbols;
    internal readonly TaskCompletionSource<LiveCodeFrozenInspectionExecutionResult> Completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal void Cancel(
        string error,
        LiveCodeFrozenInspectionSnapshot snapshot) =>
        Completion.TrySetResult(new(
            snapshot,
            snapshot,
            new(
                LiveCodeExecutionStatus.Failed,
                ScopeId,
                [],
                [],
                0,
                error,
                null,
                null)));
}
