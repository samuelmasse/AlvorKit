namespace AlvorKit;

/// <summary>Identifies the mock or interception call site that owns an invocation.</summary>
internal sealed record MockInvocationTarget
{
    private MockInvocationTarget(
        MockInvocationTargetKind kind,
        long ownerId,
        Type targetType,
        Guid moduleVersionId,
        int containingMethodToken,
        int ilOffset,
        MockInvocationOperationKind operationKind)
    {
        Kind = kind;
        OwnerId = ownerId;
        TargetType = targetType;
        ModuleVersionId = moduleVersionId;
        ContainingMethodToken = containingMethodToken;
        IlOffset = ilOffset;
        OperationKind = operationKind;
    }

    /// <summary>Gets whether this identifies a mock or call site.</summary>
    internal MockInvocationTargetKind Kind { get; }

    /// <summary>Gets the runtime-assigned mock or session owner ID.</summary>
    internal long OwnerId { get; }

    /// <summary>Gets the target or declaring type.</summary>
    internal Type TargetType { get; }

    /// <summary>Gets the interception module identity, or an empty value for instance mocks.</summary>
    internal Guid ModuleVersionId { get; }

    /// <summary>Gets the containing method token for a interception call site.</summary>
    internal int ContainingMethodToken { get; }

    /// <summary>Gets the original IL offset for a interception call site.</summary>
    internal int IlOffset { get; }

    /// <summary>Gets the operation kind.</summary>
    internal MockInvocationOperationKind OperationKind { get; }

    /// <summary>Creates an instance-mock target identity.</summary>
    internal static MockInvocationTarget ForMock(long mockId, Type targetType)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mockId);
        ArgumentNullException.ThrowIfNull(targetType);

        return new(
            MockInvocationTargetKind.Mock,
            mockId,
            targetType,
            Guid.Empty,
            0,
            -1,
            MockInvocationOperationKind.InstanceMethod);
    }

    /// <summary>Creates a stable receiver-free interception call-site identity.</summary>
    internal static MockInvocationTarget ForCallSite(
        long sessionId,
        Type targetType,
        Guid moduleVersionId,
        int containingMethodToken,
        int ilOffset,
        MockInvocationOperationKind operationKind)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(containingMethodToken);
        ArgumentOutOfRangeException.ThrowIfNegative(ilOffset);

        if (moduleVersionId == Guid.Empty)
            throw new ArgumentException("A interception call site requires a module identity.", nameof(moduleVersionId));
        if (operationKind == MockInvocationOperationKind.InstanceMethod)
            throw new ArgumentException("A interception call site requires a receiver-free operation kind.", nameof(operationKind));

        return new(
            MockInvocationTargetKind.CallSite,
            sessionId,
            targetType,
            moduleVersionId,
            containingMethodToken,
            ilOffset,
            operationKind);
    }
}
