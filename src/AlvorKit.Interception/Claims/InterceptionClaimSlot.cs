namespace AlvorKit.Interception;

/// <summary>Stable registry slot holding one active physical claim.</summary>
public sealed class InterceptionClaimSlot
{
    private InterceptionClaim claim;

    internal InterceptionClaimSlot(
        ulong slotId,
        InterceptionClaim claim)
    {
        SlotId = slotId;
        this.claim = claim;
    }

    /// <summary>Gets the registry-local stable slot ID.</summary>
    public ulong SlotId { get; }

    /// <summary>Gets the immutable claim occupying this slot.</summary>
    public InterceptionClaim Claim => Volatile.Read(ref claim);

    internal void UpdateSelector(string selector)
    {
        var current = Claim;
        Volatile.Write(
            ref claim,
            new(
                current.Method,
                current.Region,
                new(current.Owner.Consumer, selector),
                current.LogicalOperand));
    }
}
