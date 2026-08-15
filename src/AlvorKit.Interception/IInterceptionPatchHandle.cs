namespace AlvorKit;

/// <summary>Runtime-neutral ownership of one installed, replaceable, and removable method patch.</summary>
public interface IInterceptionPatchHandle : IDisposable
{
    /// <summary>Gets the stable patch ID retained across replacements.</summary>
    ulong PatchId { get; }

    /// <summary>Gets the exact method owned by this handle.</summary>
    InterceptionTarget Target { get; }

    /// <summary>Gets the most recently enqueued request ID.</summary>
    ulong LastRequestId { get; }

    /// <summary>Requests another method body for the same patch and target.</summary>
    ulong Replace(InterceptionPlan plan);

    /// <summary>Requests another exact dispatch plan for the same patch and target.</summary>
    ulong Replace(InterceptionDispatchPlan plan);

    /// <summary>Requests restoration of original IL and prevents later replacement.</summary>
    ulong Remove();

    /// <summary>Requests restoration and asynchronously observes terminal removal.</summary>
    async ValueTask<InterceptionCompletion> RemoveAsync(
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        _ = Remove();
        return await WaitForAsync(
            timeout,
            pollInterval,
            cancellationToken);
    }

    /// <summary>Reads the latest request completion.</summary>
    InterceptionCompletion GetCompletion();

    /// <summary>Waits for the latest request to reach a terminal completion.</summary>
    InterceptionCompletion WaitFor(
        TimeSpan timeout,
        TimeSpan? pollInterval = null);

    /// <summary>Asynchronously waits for the latest request to reach a terminal completion.</summary>
    async ValueTask<InterceptionCompletion> WaitForAsync(
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
            var completion = GetCompletion();
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            await Task.Delay(interval, cancellationToken);
        }

        throw new TimeoutException(
            $"Interception patch {PatchId} did not finish within {timeout}.");
    }

    /// <summary>
    /// Requests removal without waiting. Use <see cref="RemoveAsync"/> when
    /// original IL must be restored before control returns.
    /// </summary>
    new void Dispose();

}
