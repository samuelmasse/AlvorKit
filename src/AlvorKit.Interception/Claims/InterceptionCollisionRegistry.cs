namespace AlvorKit.Interception;

/// <summary>Owns physical claim slots and rejects implicit cross-consumer composition.</summary>
public sealed class InterceptionCollisionRegistry
{
    private readonly Lock gate = new();
    private readonly Dictionary<ulong, InterceptionClaimSlot> slots = [];
    private long nextSlotId;

    /// <summary>Gets the number of active physical claim slots.</summary>
    public int Count
    {
        get
        {
            lock (gate)
                return slots.Count;
        }
    }

    /// <summary>Acquires one physical claim or throws an order-independent collision diagnostic.</summary>
    public InterceptionClaimLease Acquire(InterceptionClaim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        lock (gate)
        {
            InterceptionCollision? selected = null;
            foreach (var slot in slots.Values)
            {
                var collision = Collision(slot.Claim, claim);
                if (collision is null)
                    continue;
                if (selected is null ||
                    string.CompareOrdinal(
                        collision.Message,
                        selected.Message) < 0)
                {
                    selected = collision;
                }
            }

            if (selected is not null)
                throw new InterceptionCollisionException(selected);

            var slotId = checked(
                (ulong)Interlocked.Increment(ref nextSlotId));
            var added = new InterceptionClaimSlot(slotId, claim);
            slots.Add(slotId, added);
            return new(this, added);
        }
    }

    /// <summary>Returns a stable snapshot of active claims ordered by slot ID.</summary>
    public InterceptionClaim[] Snapshot()
    {
        lock (gate)
        {
            return
            [
                .. slots
                    .OrderBy(static pair => pair.Key)
                    .Select(static pair => pair.Value.Claim)
            ];
        }
    }

    internal void Release(InterceptionClaimSlot slot)
    {
        lock (gate)
        {
            if (slots.TryGetValue(slot.SlotId, out var active) &&
                ReferenceEquals(active, slot))
            {
                slots.Remove(slot.SlotId);
            }
        }
    }

    internal void UpdateSelector(
        InterceptionClaimSlot slot,
        string selector)
    {
        lock (gate)
        {
            if (!slots.TryGetValue(slot.SlotId, out var active) ||
                !ReferenceEquals(active, slot))
            {
                throw new ObjectDisposedException(
                    nameof(InterceptionClaimLease));
            }

            slot.UpdateSelector(selector);
        }
    }

    private static InterceptionCollision? Collision(
        InterceptionClaim existing,
        InterceptionClaim incoming)
    {
        if (existing.Method == incoming.Method &&
            existing.Region.Overlaps(incoming.Region))
        {
            return InterceptionCollision.Create(
                InterceptionCollisionReason.PhysicalRegion,
                existing,
                incoming);
        }

        if (!ReferenceEquals(
                existing.Owner.Consumer,
                incoming.Owner.Consumer) &&
            existing.LogicalOperand is { } existingOperand &&
            incoming.LogicalOperand is { } incomingOperand &&
            existingOperand == incomingOperand)
        {
            return InterceptionCollision.Create(
                InterceptionCollisionReason.LogicalOperand,
                existing,
                incoming);
        }

        return null;
    }
}
