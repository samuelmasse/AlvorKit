namespace AlvorKit;

/// <summary>Exact target plus replacement method body submitted atomically to a backend.</summary>
public sealed class InterceptionPlan
{
    /// <summary>Creates one validated replacement plan.</summary>
    public InterceptionPlan(
        InterceptionTarget target,
        InterceptionMethodBody methodBody,
        InterceptionPatchFlags flags = InterceptionPatchFlags.DisableInlining)
    {
        if (!target.IsValid)
            throw new ArgumentException("A valid interception target is required.", nameof(target));
        ArgumentNullException.ThrowIfNull(methodBody);
        if ((flags & ~InterceptionPatchFlags.DisableInlining) != 0)
            throw new ArgumentOutOfRangeException(nameof(flags));

        Target = target;
        MethodBody = methodBody;
        Flags = flags;
    }

    /// <summary>Gets the exact method receiving the replacement body.</summary>
    public InterceptionTarget Target { get; }

    /// <summary>Gets the complete validated replacement body.</summary>
    public InterceptionMethodBody MethodBody { get; }

    /// <summary>Gets the requested code-generation policy.</summary>
    public InterceptionPatchFlags Flags { get; }
}
