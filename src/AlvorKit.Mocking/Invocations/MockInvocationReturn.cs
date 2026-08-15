namespace AlvorKit;

/// <summary>Stores heap-safe outcome metadata for a normal invocation return.</summary>
internal sealed class MockInvocationReturn
{
    private MockInvocationReturn(
        MockInvocationReturnKind kind,
        Type declaredType,
        object? value,
        MockUnavailableReason? unavailableReason)
    {
        Kind = kind;
        DeclaredType = declaredType;
        Value = value;
        UnavailableReason = unavailableReason;
    }

    /// <summary>Gets whether the return is void, shallow, or unavailable.</summary>
    internal MockInvocationReturnKind Kind { get; }

    /// <summary>Gets the exact declared return type.</summary>
    internal Type DeclaredType { get; }

    /// <summary>Gets an ordinary shallow return value.</summary>
    internal object? Value { get; }

    /// <summary>Gets why a return was not retained.</summary>
    internal MockUnavailableReason? UnavailableReason { get; }

    /// <summary>Creates a void return outcome.</summary>
    internal static MockInvocationReturn Void() =>
        new(MockInvocationReturnKind.Void, typeof(void), null, null);

    /// <summary>Creates a shallow ordinary return outcome.</summary>
    internal static MockInvocationReturn Shallow(Type declaredType, object? value)
    {
        ArgumentNullException.ThrowIfNull(declaredType);

        if (declaredType == typeof(void) ||
            declaredType.IsByRef ||
            declaredType.IsByRefLike)
        {
            throw new ArgumentException(
                "A borrowed return cannot be retained as a shallow value.",
                nameof(declaredType));
        }

        return new(MockInvocationReturnKind.Shallow, declaredType, value, null);
    }

    /// <summary>Creates unavailable borrowed-return metadata.</summary>
    internal static MockInvocationReturn Unavailable(
        Type declaredType,
        MockUnavailableReason reason = MockUnavailableReason.BorrowedReturnNotRetained)
    {
        ArgumentNullException.ThrowIfNull(declaredType);

        return new(MockInvocationReturnKind.Unavailable, declaredType, null, reason);
    }
}
