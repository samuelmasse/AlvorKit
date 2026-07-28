using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Describes one exact supported operation in an immutable loaded baseline.</summary>
public sealed class LoadedOperationSiteDescriptor
{
    /// <summary>The versioned deterministic identity of this exact site.</summary>
    private readonly string stableId;

    /// <summary>The loaded module version containing the body.</summary>
    private readonly Guid moduleVersionId;

    /// <summary>The MethodDef token containing the operation.</summary>
    private readonly int containingMethodToken;

    /// <summary>The exact constructed declaring-type and method context.</summary>
    private readonly string constructedContext;

    /// <summary>The identity of the immutable baseline body.</summary>
    private readonly LoadedMethodBodyIdentity bodyIdentity;

    /// <summary>The original operation instruction offset.</summary>
    private readonly int baselineOffset;

    /// <summary>The exact original operation opcode value.</summary>
    private readonly ushort opCodeValue;

    /// <summary>The recognized operation shape.</summary>
    private readonly LoadedOperationKind kind;

    /// <summary>The unresolved method, constructor, or field token.</summary>
    private readonly int metadataToken;

    /// <summary>The canonical exact constructed operand signature.</summary>
    private readonly string canonicalSignature;

    /// <summary>The accepted prefix sequence in original order.</summary>
    private readonly ImmutableArray<LoadedOperationPrefixDescriptor> prefixes;

    /// <summary>Creates one fully validated exact operation descriptor.</summary>
    internal LoadedOperationSiteDescriptor(
        string stableId,
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        LoadedMethodBodyIdentity bodyIdentity,
        int baselineOffset,
        ushort opCodeValue,
        LoadedOperationKind kind,
        int metadataToken,
        string canonicalSignature,
        ImmutableArray<LoadedOperationPrefixDescriptor> prefixes)
    {
        this.stableId = stableId;
        this.moduleVersionId = moduleVersionId;
        this.containingMethodToken = containingMethodToken;
        this.constructedContext = constructedContext;
        this.bodyIdentity = bodyIdentity;
        this.baselineOffset = baselineOffset;
        this.opCodeValue = opCodeValue;
        this.kind = kind;
        this.metadataToken = metadataToken;
        this.canonicalSignature = canonicalSignature;
        this.prefixes = prefixes;
    }

    /// <summary>Gets the versioned deterministic identity of this exact site.</summary>
    public string StableId => stableId;

    /// <summary>Gets the loaded module version containing the body.</summary>
    public Guid ModuleVersionId => moduleVersionId;

    /// <summary>Gets the MethodDef token containing the operation.</summary>
    public int ContainingMethodToken => containingMethodToken;

    /// <summary>Gets the exact constructed declaring-type and method context.</summary>
    public string ConstructedContext => constructedContext;

    /// <summary>Gets the identity of the immutable baseline body.</summary>
    public LoadedMethodBodyIdentity BodyIdentity => bodyIdentity;

    /// <summary>Gets the original operation instruction offset.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the exact original operation opcode value.</summary>
    public ushort OpCodeValue => opCodeValue;

    /// <summary>Gets the recognized operation shape.</summary>
    public LoadedOperationKind Kind => kind;

    /// <summary>Gets the unresolved method, constructor, or field token.</summary>
    public int MetadataToken => metadataToken;

    /// <summary>Gets the canonical exact constructed operand signature.</summary>
    public string CanonicalSignature => canonicalSignature;

    /// <summary>Gets the accepted prefix sequence in original order.</summary>
    public ImmutableArray<LoadedOperationPrefixDescriptor> Prefixes =>
        prefixes;
}
