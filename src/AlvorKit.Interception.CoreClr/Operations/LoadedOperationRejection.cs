namespace AlvorKit;

/// <summary>Identifies a stable category of loaded-operation recognition failure.</summary>
public enum LoadedOperationRejectionReason
{
    /// <summary>The operation metadata token was not resolved.</summary>
    UnresolvedMetadata,

    /// <summary>The opcode and resolved static or constructor shape disagree.</summary>
    InvalidOperationSignature,

    /// <summary>The executable signature uses variable arguments.</summary>
    VariableArguments,

    /// <summary>The executable signature retains open generic parameters.</summary>
    OpenGenericSignature,

    /// <summary>The live value receiver is by-ref-like.</summary>
    RefLikeReceiver,

    /// <summary>The receiver stack shape cannot use an exact supported route.</summary>
    UnsupportedReceiver,

    /// <summary>The operation owns a prefix outside the accepted matrix.</summary>
    UnsupportedPrefix,

    /// <summary>The operation repeats a prefix with ambiguous replay semantics.</summary>
    DuplicatePrefix
}

/// <summary>Describes one deterministic recognition rejection without mutating planner state.</summary>
public sealed class LoadedOperationRejection
{
    /// <summary>The stable rejection category.</summary>
    private readonly LoadedOperationRejectionReason reason;

    /// <summary>The baseline operation offset.</summary>
    private readonly int baselineOffset;

    /// <summary>The related prefix offset, or the operation offset.</summary>
    private readonly int relatedOffset;

    /// <summary>The rejected operation or related prefix opcode value.</summary>
    private readonly ushort opCodeValue;

    /// <summary>The unresolved operation or prefix token, or zero.</summary>
    private readonly int metadataToken;

    /// <summary>The deterministic actionable diagnostic.</summary>
    private readonly string detail;

    /// <summary>Creates one structured recognition rejection.</summary>
    internal LoadedOperationRejection(
        LoadedOperationRejectionReason reason,
        int baselineOffset,
        int relatedOffset,
        ushort opCodeValue,
        int metadataToken,
        string detail)
    {
        this.reason = reason;
        this.baselineOffset = baselineOffset;
        this.relatedOffset = relatedOffset;
        this.opCodeValue = opCodeValue;
        this.metadataToken = metadataToken;
        this.detail = detail;
    }

    /// <summary>Gets the stable rejection category.</summary>
    public LoadedOperationRejectionReason Reason => reason;

    /// <summary>Gets the baseline operation offset.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the related prefix offset, or the operation offset.</summary>
    public int RelatedOffset => relatedOffset;

    /// <summary>Gets the rejected operation or related prefix opcode value.</summary>
    public ushort OpCodeValue => opCodeValue;

    /// <summary>Gets the unresolved operation or prefix token, or zero.</summary>
    public int MetadataToken => metadataToken;

    /// <summary>Gets the deterministic actionable diagnostic.</summary>
    public string Detail => detail;
}
