namespace AlvorKit;

/// <summary>Describes one argument value that invocation history cannot retain.</summary>
internal sealed record MockUnavailableSnapshot
{
    /// <summary>Creates unavailable argument metadata.</summary>
    internal MockUnavailableSnapshot(
        int declaredIndex,
        Type declaredType,
        MockSnapshotPhase phase,
        MockUnavailableReason reason)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(declaredIndex);
        ArgumentNullException.ThrowIfNull(declaredType);

        DeclaredIndex = declaredIndex;
        DeclaredType = declaredType;
        Phase = phase;
        Reason = reason;
    }

    /// <summary>Gets the zero-based declared parameter index.</summary>
    internal int DeclaredIndex { get; }

    /// <summary>Gets the exact declared parameter type.</summary>
    internal Type DeclaredType { get; }

    /// <summary>Gets the observation phase.</summary>
    internal MockSnapshotPhase Phase { get; }

    /// <summary>Gets why no value was retained.</summary>
    internal MockUnavailableReason Reason { get; }
}
