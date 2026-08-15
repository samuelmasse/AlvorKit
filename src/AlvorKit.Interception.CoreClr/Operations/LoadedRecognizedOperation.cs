using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Transfers one internally validated operation into a stable site descriptor.</summary>
internal sealed class LoadedRecognizedOperation
{
    /// <summary>The recognized operation kind.</summary>
    private readonly LoadedOperationKind kind;

    /// <summary>The unresolved operation metadata token.</summary>
    private readonly int metadataToken;

    /// <summary>The canonical exact constructed operand signature.</summary>
    private readonly string canonicalSignature;

    /// <summary>The accepted prefix sequence.</summary>
    private readonly ImmutableArray<LoadedOperationPrefixDescriptor> prefixes;

    /// <summary>Creates one internally validated operation.</summary>
    internal LoadedRecognizedOperation(
        LoadedOperationKind kind,
        int metadataToken,
        string canonicalSignature,
        ImmutableArray<LoadedOperationPrefixDescriptor> prefixes)
    {
        this.kind = kind;
        this.metadataToken = metadataToken;
        this.canonicalSignature = canonicalSignature;
        this.prefixes = prefixes;
    }

    /// <summary>Gets the recognized operation kind.</summary>
    internal LoadedOperationKind Kind => kind;

    /// <summary>Gets the unresolved operation metadata token.</summary>
    internal int MetadataToken => metadataToken;

    /// <summary>Gets the canonical exact constructed operand signature.</summary>
    internal string CanonicalSignature => canonicalSignature;

    /// <summary>Gets the accepted prefix sequence.</summary>
    internal ImmutableArray<LoadedOperationPrefixDescriptor> Prefixes =>
        prefixes;
}
