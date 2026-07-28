namespace AlvorKit.Interception;

/// <summary>Exact managed dispatch requested for one method while preserving its original IL fallback.</summary>
public sealed class InterceptionDispatchPlan
{
    private InterceptionDispatchPlan(
        InterceptionTarget target,
        ulong slotId,
        nint resolverPointer,
        InterceptionPatchFlags flags)
    {
        if (!target.IsValid)
            throw new ArgumentException("A valid interception target is required.", nameof(target));
        if (slotId == 0)
            throw new ArgumentOutOfRangeException(nameof(slotId));
        if (resolverPointer == 0)
            throw new ArgumentOutOfRangeException(nameof(resolverPointer));
        if ((flags & ~InterceptionPatchFlags.DisableInlining) != 0)
            throw new ArgumentOutOfRangeException(nameof(flags));

        Target = target;
        SlotId = slotId;
        ResolverPointer = resolverPointer;
        Flags = flags;
    }

    /// <summary>Gets the exact method receiving the dispatch wrapper.</summary>
    public InterceptionTarget Target { get; }

    /// <summary>Gets the stable managed resolver slot.</summary>
    public ulong SlotId { get; }

    /// <summary>Gets the prepared managed resolver function pointer.</summary>
    public nint ResolverPointer { get; }

    /// <summary>Gets the requested code-generation policy.</summary>
    public InterceptionPatchFlags Flags { get; }

    /// <summary>Creates a dispatch plan for an exact loaded method.</summary>
    public static InterceptionDispatchPlan ForMethod(
        MethodInfo target,
        ulong slotId,
        nint resolverPointer) =>
        new(
            InterceptionTarget.FromMethod(target),
            slotId,
            resolverPointer,
            InterceptionPatchFlags.DisableInlining);

    public static InterceptionDispatchPlan ForTarget(
        InterceptionTarget target,
        ulong slotId,
        nint resolverPointer,
        InterceptionPatchFlags flags = InterceptionPatchFlags.DisableInlining) =>
        new(target, slotId, resolverPointer, flags);
}
