namespace AlvorKit.Interception;

public sealed partial class InterceptionProfiler
{
    /// <summary>Reads cold-path queue and active-patch diagnostics.</summary>
    public InterceptionBackendState GetState()
    {
        Marshal.ThrowExceptionForHR(api.GetProfilerState(out var value));
        if (value.AbiVersion != AbiVersion)
            throw new InvalidOperationException("The profiler returned state for a different ABI.");

        return new(
            value.Ready != 0,
            value.Stopping != 0,
            value.PendingRequests,
            value.ActivePatches,
            value.RetainedCompletions,
            value.LastRequestId);
    }

    /// <summary>Reads one retained request completion without blocking.</summary>
    public InterceptionCompletion GetCompletion(ulong requestId)
    {
        if (requestId == 0)
            throw new ArgumentOutOfRangeException(nameof(requestId));

        Marshal.ThrowExceptionForHR(api.GetCompletion(requestId, out var value));
        var target = knownTargets.TryGetValue(value.PatchId, out var known)
            ? known
            : FromNative(value.Target);
        var completion = new InterceptionCompletion(
            value.RequestId,
            value.PatchId,
            (InterceptionOperation)value.Operation,
            (InterceptionState)value.State,
            value.Hresult,
            (InterceptionPatchFlags)value.PatchFlags,
            target,
            value.RejitStartedCallbacks,
            value.ParameterCallbacks,
            value.RejitFinishedCallbacks,
            value.RejitErrorCallbacks,
            TimeSpan.FromMicroseconds(((long)value.ElapsedMicroseconds)));
        if (completion.State is InterceptionState.Failed or
                InterceptionState.Removed)
        {
            _ = knownTargets.TryRemove(value.PatchId, out _);
        }
        return completion;
    }

    /// <summary>Reads ABI v3 generation-specific completion evidence.</summary>
    public InterceptionGenerationCompletion GetGenerationCompletion(
        ulong requestId)
    {
        if (requestId == 0)
            throw new ArgumentOutOfRangeException(nameof(requestId));

        Marshal.ThrowExceptionForHR(
            api.GetGenerationCompletion(requestId, out var value));
        if (value.AbiVersion != AbiVersion)
            throw new InvalidOperationException("The profiler returned a different generation ABI.");
        return new(
            value.RequestId,
            value.PatchId,
            value.GenerationId,
            value.PriorGenerationId,
            (InterceptionState)value.State,
            value.Hresult,
            (InterceptionGenerationFailureStage)value.FailureStage,
            value.FailureRelocationIndex == uint.MaxValue
                ? null
                : value.FailureRelocationIndex,
            value.RequestedRelocations,
            value.AppliedRelocations,
            value.RequestedIlMapEntries,
            value.AppliedIlMapEntries,
            value.TargetRejitId);
    }

    /// <summary>Reads the token and HRESULT produced for one generation relocation.</summary>
    public InterceptionGenerationRelocationResult GetRelocationResult(
        ulong requestId,
        uint relocationIndex)
    {
        if (requestId == 0)
            throw new ArgumentOutOfRangeException(nameof(requestId));

        Marshal.ThrowExceptionForHR(
            api.GetRelocationResult(
                requestId,
                relocationIndex,
                out var value));
        if (value.AbiVersion != AbiVersion)
            throw new InvalidOperationException("The profiler returned a different relocation ABI.");
        return new(
            value.RequestId,
            value.GenerationId,
            value.RelocationIndex,
            (InterceptionGenerationRelocationKind)value.Kind,
            value.MetadataToken,
            value.Hresult);
    }

    /// <summary>Waits for a request's terminal completion.</summary>
    public InterceptionCompletion WaitFor(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var interval = pollInterval ?? TimeSpan.FromMilliseconds(5);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            var completion = GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(interval);
        }

        throw new TimeoutException(
            $"Interception request {requestId} did not finish within {timeout}.");
    }

    /// <summary>Asynchronously waits for a request's terminal completion.</summary>
    public async ValueTask<InterceptionCompletion> WaitForAsync(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var interval = pollInterval ?? TimeSpan.FromMilliseconds(5);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var completion = GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            await Task.Delay(interval, cancellationToken);
        }

        throw new TimeoutException(
            $"Interception request {requestId} did not finish within {timeout}.");
    }
}
