namespace AlvorKit;

/// <summary>Reversible ownership lease for one neutral physical-claim slot.</summary>
public sealed class InterceptionClaimLease : IDisposable
{
    private readonly InterceptionCollisionRegistry registry;
    private int active = 1;

    internal InterceptionClaimLease(
        InterceptionCollisionRegistry registry,
        InterceptionClaimSlot slot)
    {
        this.registry = registry;
        Slot = slot;
    }

    /// <summary>Gets the stable slot occupied by this lease.</summary>
    public InterceptionClaimSlot Slot { get; }

    /// <summary>Gets whether this lease still owns its physical claim.</summary>
    public bool IsActive => Volatile.Read(ref active) != 0;

    /// <summary>Updates selector metadata without changing physical or logical claim identity.</summary>
    public void UpdateSelector(string selector)
    {
        ObjectDisposedException.ThrowIf(!IsActive, this);
        registry.UpdateSelector(Slot, selector);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref active, 0) != 0)
            registry.Release(Slot);
    }
}
