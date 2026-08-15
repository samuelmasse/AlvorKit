namespace AlvorKit;

/// <summary>Creates an intentional game-loop stall that remains releasable from the frozen LiveCode lane.</summary>
[Root]
public sealed class ObservatoryFreeze(Log log)
{
    private readonly ManualResetEventSlim released = new(true);
    private long requestedAtTick;
    private int requested;
    private int frozen;
    private int gameThreadId;

    /// <summary>Gets whether the game thread is currently waiting for an out-of-band release.</summary>
    public bool IsFrozen => Volatile.Read(ref frozen) != 0;

    /// <summary>Gets the game-loop thread blocked by the demonstration freeze.</summary>
    public int GameThreadId => Volatile.Read(ref gameThreadId);

    /// <summary>Gets the universe tick at which the freeze was requested.</summary>
    public long RequestedAtTick => Interlocked.Read(ref requestedAtTick);

    /// <summary>Requests a stall at the end of the current update.</summary>
    public void Request(long tick = 0)
    {
        if (Interlocked.CompareExchange(ref requested, 1, 0) != 0 || IsFrozen)
            return;

        Interlocked.Exchange(ref requestedAtTick, tick);
        released.Reset();
        log.Warn("FREEZE REQUESTED: the game loop will stop while the LiveCode listener remains responsive.");
    }

    /// <summary>Blocks the game-loop thread when a freeze was requested.</summary>
    public void BlockIfRequested()
    {
        if (Interlocked.Exchange(ref requested, 0) == 0)
            return;

        Volatile.Write(ref gameThreadId, Environment.CurrentManagedThreadId);
        Volatile.Write(ref frozen, 1);
        log.Warn("GAME LOOP FROZEN on managed thread {0}.", GameThreadId);
        released.Wait();
        Volatile.Write(ref frozen, 0);
        log.Info("GAME LOOP RELEASED by out-of-band LiveCode.");
    }

    /// <summary>Releases an intentional demonstration freeze from any thread.</summary>
    public void Release() => released.Set();
}
