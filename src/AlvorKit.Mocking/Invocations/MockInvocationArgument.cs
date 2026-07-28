namespace AlvorKit.Mocking;

/// <summary>Contains the retained entry and exit state of one declared argument.</summary>
internal sealed record MockInvocationArgument
{
    /// <summary>Creates one declared-order invocation argument.</summary>
    internal MockInvocationArgument(
        int declaredIndex,
        Type declaredType,
        MockInvocationArgumentSnapshot entry,
        MockInvocationArgumentSnapshot exit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(declaredIndex);
        ArgumentNullException.ThrowIfNull(declaredType);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(exit);

        if (entry.DeclaredIndex != declaredIndex ||
            exit.DeclaredIndex != declaredIndex ||
            entry.DeclaredType != declaredType ||
            exit.DeclaredType != declaredType ||
            entry.Phase != MockSnapshotPhase.Entry ||
            exit.Phase != MockSnapshotPhase.Exit)
        {
            throw new ArgumentException(
                "Argument snapshots must match the declared index, type, and phase.");
        }

        DeclaredIndex = declaredIndex;
        DeclaredType = declaredType;
        Entry = entry;
        Exit = exit;
    }

    /// <summary>Gets the zero-based declared parameter index.</summary>
    internal int DeclaredIndex { get; }

    /// <summary>Gets the exact declared parameter type.</summary>
    internal Type DeclaredType { get; }

    /// <summary>Gets the retained entry representation.</summary>
    internal MockInvocationArgumentSnapshot Entry { get; }

    /// <summary>Gets the retained normal-exit representation.</summary>
    internal MockInvocationArgumentSnapshot Exit { get; }
}
