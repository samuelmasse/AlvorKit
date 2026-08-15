namespace AlvorKit;

/// <summary>Stores one heap-safe retained representation of an argument.</summary>
internal sealed class MockInvocationArgumentSnapshot
{
    private MockInvocationArgumentSnapshot(
        int declaredIndex,
        Type declaredType,
        MockSnapshotPhase phase,
        MockInvocationArgumentSnapshotKind kind,
        object? value,
        MockUnavailableSnapshot? unavailable)
    {
        DeclaredIndex = declaredIndex;
        DeclaredType = declaredType;
        Phase = phase;
        Kind = kind;
        Value = value;
        Unavailable = unavailable;
    }

    /// <summary>Gets the zero-based declared parameter index.</summary>
    internal int DeclaredIndex { get; }

    /// <summary>Gets the exact declared parameter type.</summary>
    internal Type DeclaredType { get; }

    /// <summary>Gets the observation phase.</summary>
    internal MockSnapshotPhase Phase { get; }

    /// <summary>Gets the retained representation kind.</summary>
    internal MockInvocationArgumentSnapshotKind Kind { get; }

    /// <summary>Gets the retained shallow or projected value.</summary>
    internal object? Value { get; }

    /// <summary>Gets unavailable metadata when no value was retained.</summary>
    internal MockUnavailableSnapshot? Unavailable { get; }

    /// <summary>Creates a shallow ordinary argument snapshot.</summary>
    internal static MockInvocationArgumentSnapshot Shallow(
        int declaredIndex,
        Type declaredType,
        MockSnapshotPhase phase,
        object? value)
    {
        Validate(declaredIndex, declaredType);

        var valueType = declaredType.IsByRef
            ? declaredType.GetElementType()!
            : declaredType;
        if (valueType.IsByRefLike)
        {
            throw new ArgumentException(
                "A byref-like value cannot be retained as a shallow snapshot.",
                nameof(declaredType));
        }

        return new(
            declaredIndex,
            declaredType,
            phase,
            MockInvocationArgumentSnapshotKind.Shallow,
            value,
            null);
    }

    /// <summary>Creates a heap-safe projector result snapshot.</summary>
    internal static MockInvocationArgumentSnapshot Projected(
        int declaredIndex,
        Type declaredType,
        MockSnapshotPhase phase,
        object? value)
    {
        Validate(declaredIndex, declaredType);

        return new(
            declaredIndex,
            declaredType,
            phase,
            MockInvocationArgumentSnapshotKind.Projected,
            value,
            null);
    }

    /// <summary>Creates an unavailable argument snapshot.</summary>
    internal static MockInvocationArgumentSnapshot UnavailableValue(
        MockUnavailableSnapshot unavailable)
    {
        ArgumentNullException.ThrowIfNull(unavailable);

        return new(
            unavailable.DeclaredIndex,
            unavailable.DeclaredType,
            unavailable.Phase,
            MockInvocationArgumentSnapshotKind.Unavailable,
            null,
            unavailable);
    }

    private static void Validate(int declaredIndex, Type declaredType)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(declaredIndex);
        ArgumentNullException.ThrowIfNull(declaredType);
    }
}
