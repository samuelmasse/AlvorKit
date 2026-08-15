namespace AlvorKit;

/// <summary>
/// Reviewed exact operation signature plus the ownership of its hidden receiver.
/// </summary>
public sealed class InterceptionCallShape
{
    private InterceptionCallShape(
        MethodInfo operation,
        Type? receiverType,
        InterceptionReceiverOwnership receiverOwnership)
    {
        Operation = operation;
        ReceiverType = receiverType;
        ReceiverOwnership = receiverOwnership;
    }

    /// <summary>Gets the declared operation whose arguments and return are preserved.</summary>
    public MethodInfo Operation { get; }

    /// <summary>Gets the concrete hidden receiver type, or null for a static call.</summary>
    public Type? ReceiverType { get; }

    /// <summary>Gets how the exact call carries the hidden receiver.</summary>
    public InterceptionReceiverOwnership ReceiverOwnership { get; }

    /// <summary>Creates the ordinary static or reference-receiver call shape.</summary>
    public static InterceptionCallShape FromMethod(
        MethodInfo operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (operation.IsStatic)
        {
            return new(
                operation,
                null,
                InterceptionReceiverOwnership.None);
        }

        Type declaringType = operation.DeclaringType ??
            throw new NotSupportedException(
                "An instance operation must have a declaring type.");
        if (declaringType.IsValueType)
        {
            throw new NotSupportedException(
                "Value-type receivers require explicit managed-reference " +
                "ownership selection.");
        }

        return new(
            operation,
            declaringType,
            InterceptionReceiverOwnership.Reference);
    }

    /// <summary>
    /// Creates a call shape whose hidden receiver is a managed reference to
    /// one closed, non-ref-like concrete value type.
    /// </summary>
    public static InterceptionCallShape ForManagedReferenceReceiver(
        MethodInfo operation,
        Type receiverType) =>
        ForValueReceiver(
            operation,
            receiverType,
            InterceptionReceiverOwnership.ManagedReference);

    /// <summary>
    /// Creates a call shape whose hidden receiver is a readonly managed
    /// reference to one closed, non-ref-like concrete value type.
    /// </summary>
    public static InterceptionCallShape ForReadOnlyManagedReferenceReceiver(
        MethodInfo operation,
        Type receiverType) =>
        ForValueReceiver(
            operation,
            receiverType,
            InterceptionReceiverOwnership.ReadOnlyManagedReference);

    private static InterceptionCallShape ForValueReceiver(
        MethodInfo operation,
        Type receiverType,
        InterceptionReceiverOwnership ownership)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(receiverType);
        if (operation.IsStatic)
        {
            throw new ArgumentException(
                "A static operation has no receiver ownership.",
                nameof(operation));
        }
        if (operation.ContainsGenericParameters ||
            operation.DeclaringType?.ContainsGenericParameters == true)
        {
            throw new NotSupportedException(
                "Managed-reference receiver operations must be fully closed.");
        }
        if (receiverType.IsByRef)
        {
            throw new ArgumentException(
                "Pass the concrete receiver element type; ownership already " +
                "selects a managed reference.",
                nameof(receiverType));
        }
        ValidateReceiverConstruction(receiverType);
        if (!receiverType.IsValueType)
        {
            throw new ArgumentException(
                "A managed-reference receiver must be a value type.",
                nameof(receiverType));
        }

        Type declaringType = operation.DeclaringType ??
            throw new NotSupportedException(
                "An instance operation must have a declaring type.");
        bool matches = declaringType.IsValueType
            ? declaringType == receiverType
            : declaringType.IsInterface &&
                declaringType.IsAssignableFrom(receiverType);
        if (!matches)
        {
            throw new ArgumentException(
                $"Receiver '{receiverType}' does not match declared " +
                $"operation owner '{declaringType}'.",
                nameof(receiverType));
        }

        return new(
            operation,
            receiverType,
            ownership);
    }

    private static void ValidateReceiverConstruction(Type receiverType)
    {
        if (receiverType.ContainsGenericParameters)
        {
            throw new NotSupportedException(
                "Managed-reference receiver types must be fully closed.");
        }
        if (receiverType.IsByRefLike ||
            receiverType.IsPointer ||
            receiverType.IsFunctionPointer)
        {
            throw new NotSupportedException(
                $"Receiver type '{receiverType}' has an unsupported shape.");
        }
        if (receiverType.HasElementType)
        {
            ValidateReceiverConstruction(receiverType.GetElementType()!);
            return;
        }

        foreach (Type argument in receiverType.GetGenericArguments())
            ValidateReceiverConstruction(argument);
    }
}
