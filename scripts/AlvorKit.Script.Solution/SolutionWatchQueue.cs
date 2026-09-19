namespace AlvorKit;

/// <summary>Coalesces notifications without losing changes that arrive while a graph is being evaluated.</summary>
internal class SolutionWatchQueue
{
    /// <summary>Coalesces owner requests while preserving arrivals during evaluation.</summary>
    private readonly ConcurrentDictionary<SolutionWatchRequest, byte> pending = [];
    /// <summary>Wakes the single consumer without retaining redundant notification signals.</summary>
    private readonly Channel<bool> signal = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
    });
    /// <summary>Preserves the first terminal notification failure for the consumer.</summary>
    private Exception? failure;

    /// <summary>Records the affected owner before signaling the single evaluation consumer.</summary>
    public void Add(SolutionWatchRequest request)
    {
        pending.TryAdd(request, 0);
        signal.Writer.TryWrite(true);
    }

    /// <summary>Fails the session if native notifications become unreliable, including buffer overflow.</summary>
    public void Fail(Exception exception)
    {
        Interlocked.CompareExchange(ref failure, exception, null);
        signal.Writer.TryComplete();
    }

    /// <summary>Preserves the native failure message and prevents further work after notifications are lost.</summary>
    public void ThrowIfFailed()
    {
        if (Volatile.Read(ref failure) is { } exception)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(exception);
    }

    /// <summary>Waits only for real events, with a short debounce bounded against continuous checkout traffic.</summary>
    public async Task<IReadOnlySet<SolutionWatchRequest>> ReadAsync(CancellationToken cancellation)
    {
        await signal.Reader.WaitToReadAsync(cancellation);
        ThrowIfFailed();
        signal.Reader.TryRead(out _);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Task.Delay(200, cancellation);

            if (!signal.Reader.TryRead(out _))
                break;
        }

        var batch = pending.Keys.ToHashSet();
        ThrowIfFailed();

        foreach (var request in batch)
            pending.TryRemove(request, out _);

        return batch;
    }
}
