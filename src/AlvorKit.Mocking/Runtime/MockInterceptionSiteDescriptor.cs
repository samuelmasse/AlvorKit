namespace AlvorKit.Mocking;

/// <summary>
/// Identifies one original operation independently of rewritten instruction
/// offsets.
/// </summary>
internal readonly record struct MockInterceptionSiteDescriptor
{
    /// <summary>Creates one validated immutable interception-site identity.</summary>
    internal MockInterceptionSiteDescriptor(
        Guid moduleVersionId,
        int containingMethodToken,
        int originalIlOffset,
        MockInvocationOperationKind operationKind)
    {
        if (moduleVersionId == Guid.Empty)
        {
            throw new ArgumentException(
                "An interception site requires a non-empty module identity.",
                nameof(moduleVersionId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            containingMethodToken);
        ArgumentOutOfRangeException.ThrowIfNegative(originalIlOffset);

        ModuleVersionId = moduleVersionId;
        ContainingMethodToken = containingMethodToken;
        OriginalIlOffset = originalIlOffset;
        OperationKind = operationKind;
    }

    /// <summary>Gets the source module MVID used by the stable site ID.</summary>
    internal Guid ModuleVersionId { get; }

    /// <summary>Gets the metadata token of the original containing method.</summary>
    internal int ContainingMethodToken { get; }

    /// <summary>Gets the original call instruction offset.</summary>
    internal int OriginalIlOffset { get; }

    /// <summary>Gets the intercepted operation shape.</summary>
    internal MockInvocationOperationKind OperationKind { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{ModuleVersionId:N}:" +
        $"0x{unchecked((uint)ContainingMethodToken):x8}:" +
        $"IL_{OriginalIlOffset:x4}:{OperationKind}";
}
