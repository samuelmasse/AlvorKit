namespace AlvorKit;

/// <summary>
/// Resolves loaded metadata tokens into exact constructed signatures without reflection bodies.
/// </summary>
public interface ILoadedOperationMetadataResolver
{
    /// <summary>Attempts to resolve a method, constructor, or MemberRef token.</summary>
    bool TryResolveMethod(
        int metadataToken,
        [NotNullWhen(true)] out LoadedMethodOperand? method);

    /// <summary>Attempts to resolve a field or MemberRef token.</summary>
    bool TryResolveField(
        int metadataToken,
        [NotNullWhen(true)] out LoadedFieldOperand? field);

    /// <summary>Attempts to resolve a constrained-prefix type token.</summary>
    bool TryResolveType(
        int metadataToken,
        [NotNullWhen(true)] out LoadedTypeOperand? type);
}
