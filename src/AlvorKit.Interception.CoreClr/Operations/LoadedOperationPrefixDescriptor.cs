namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Describes one accepted prefix at its immutable baseline coordinate.</summary>
public sealed class LoadedOperationPrefixDescriptor
{
    /// <summary>The accepted prefix kind.</summary>
    private readonly LoadedOperationPrefixKind kind;

    /// <summary>The original prefix instruction offset.</summary>
    private readonly int baselineOffset;

    /// <summary>The unresolved constrained type token, or zero for <c>volatile.</c>.</summary>
    private readonly int metadataToken;

    /// <summary>The canonical constrained type signature, or an empty string.</summary>
    private readonly string operandSignature;

    /// <summary>Creates one validated accepted-prefix descriptor.</summary>
    internal LoadedOperationPrefixDescriptor(
        LoadedOperationPrefixKind kind,
        int baselineOffset,
        int metadataToken,
        string operandSignature)
    {
        this.kind = kind;
        this.baselineOffset = baselineOffset;
        this.metadataToken = metadataToken;
        this.operandSignature = operandSignature;
    }

    /// <summary>Gets the accepted prefix kind.</summary>
    public LoadedOperationPrefixKind Kind => kind;

    /// <summary>Gets the original prefix instruction offset.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the unresolved constrained type token, or zero.</summary>
    public int MetadataToken => metadataToken;

    /// <summary>Gets the canonical constrained type signature, or an empty string.</summary>
    public string OperandSignature => operandSignature;
}
