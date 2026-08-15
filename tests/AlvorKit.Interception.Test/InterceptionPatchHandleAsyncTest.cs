namespace AlvorKit;

/// <summary>Verifies the explicit asynchronous patch-retirement contract.</summary>
[TestClass]
public sealed class InterceptionPatchHandleAsyncTest
{
    /// <summary>Removal is submitted once and completes only after terminal restoration is observed.</summary>
    [TestMethod]
    public async Task RemoveAsync_RequestsAndObservesTerminalRemoval()
    {
        var implementation = new DeferredRemovalPatchHandle();
        IInterceptionPatchHandle handle = implementation;

        var completion = await handle.RemoveAsync(
            TimeSpan.FromSeconds(1),
            TimeSpan.Zero);

        Assert.AreEqual(1, implementation.RemoveCalls);
        Assert.IsTrue(implementation.CompletionReads >= 2);
        Assert.AreEqual(InterceptionState.Removed, completion.State);
    }
}

internal sealed class DeferredRemovalPatchHandle : IInterceptionPatchHandle
{
    private readonly InterceptionTarget target = InterceptionTarget.FromMethod(
        typeof(DeferredRemovalPatchHandle).GetMethod(
            nameof(TargetMethod),
            BindingFlags.NonPublic | BindingFlags.Static)!);
    private int completionReads;

    public ulong PatchId => 41;
    public InterceptionTarget Target => target;
    public ulong LastRequestId => 73;
    internal int RemoveCalls { get; private set; }
    internal int CompletionReads => Volatile.Read(ref completionReads);

    public ulong Replace(InterceptionPlan plan) =>
        throw new NotSupportedException();

    public ulong Replace(InterceptionDispatchPlan plan) =>
        throw new NotSupportedException();

    public ulong Remove()
    {
        RemoveCalls++;
        return LastRequestId;
    }

    public InterceptionCompletion GetCompletion()
    {
        var reads = Interlocked.Increment(ref completionReads);
        return new(
            LastRequestId,
            PatchId,
            InterceptionOperation.Remove,
            reads == 1
                ? InterceptionState.Removing
                : InterceptionState.Removed,
            0,
            InterceptionPatchFlags.DisableInlining,
            Target,
            0,
            0,
            0,
            0,
            TimeSpan.Zero);
    }

    public InterceptionCompletion WaitFor(
        TimeSpan timeout,
        TimeSpan? pollInterval = null) =>
        throw new NotSupportedException();

    public void Dispose()
    {
        _ = Remove();
    }

    private static int TargetMethod() => 1;
}
