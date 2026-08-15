namespace AlvorKit;

/// <summary>Owns immutable exact delegates until every acquired route call drains.</summary>
internal sealed class ProfiledDelegateRouteBinding<TDelegate>(
    MockInterceptionRoute route,
    TDelegate original,
    TDelegate wrapper)
    where TDelegate : Delegate
{
    private readonly ManualResetEventSlim drained = new(true);
    private TDelegate? activeWrapper = wrapper;
    private int accepting = 1;
    private int inFlight;

    internal MockInterceptionRoute Route { get; } = route;

    internal TDelegate Original { get; } = original;

    internal TDelegate Wrapper =>
        Volatile.Read(ref activeWrapper) ??
        throw new InvalidOperationException(
            "The profiled delegate route is retired.");

    /// <summary>Acquires one immutable binding snapshot before delegate selection.</summary>
    internal bool TryAcquire(out IDisposable? lease)
    {
        if (Volatile.Read(ref accepting) == 0)
        {
            lease = null;
            return false;
        }

        if (Interlocked.Increment(ref inFlight) == 1)
            drained.Reset();
        if (Volatile.Read(ref accepting) != 0)
        {
            lease = new Lease(this);
            return true;
        }

        Release();
        lease = null;
        return false;
    }

    /// <summary>Stops new acquisitions and waits for already selected delegates.</summary>
    internal void RetireAndDrain()
    {
        Volatile.Write(ref accepting, 0);
        if (Volatile.Read(ref inFlight) == 0)
            drained.Set();
        if (!drained.Wait(TimeSpan.FromSeconds(10)))
        {
            throw new TimeoutException(
                "Timed out draining a retired profiled delegate route.");
        }

        Volatile.Write(ref activeWrapper, null);
    }

    private void Release()
    {
        if (Interlocked.Decrement(ref inFlight) == 0)
            drained.Set();
    }

    private sealed class Lease(
        ProfiledDelegateRouteBinding<TDelegate> owner) :
        IDisposable
    {
        private ProfiledDelegateRouteBinding<TDelegate>? binding = owner;

        public void Dispose() =>
            Interlocked.Exchange(ref binding, null)?.Release();
    }
}
