namespace AlvorKit;

/// <summary>Identifies one stable preparation-selection rejection category.</summary>
public enum LoadedInterceptionPreparationRejectionReason
{
    /// <summary>The requested body identity is no longer authoritative.</summary>
    StaleBodyIdentity,

    /// <summary>No recognized operation has the exact member signature.</summary>
    MemberSignatureNotFound,

    /// <summary>The exact member signature resolves to more than one site.</summary>
    AmbiguousMemberSignature,

    /// <summary>A stable site and occurrence were both supplied.</summary>
    ConflictingSiteSelector,

    /// <summary>The exact stable site does not match the selected signature.</summary>
    StableSiteNotFound,

    /// <summary>The occurrence is negative or outside the signature matches.</summary>
    OccurrenceOutOfRange
}

/// <summary>Describes one deterministic code-first selection rejection.</summary>
public sealed class LoadedInterceptionPreparationRejection
{
    /// <summary>The stable rejection category.</summary>
    private readonly LoadedInterceptionPreparationRejectionReason reason;

    /// <summary>The requested exact member signature.</summary>
    private readonly string memberSignature;

    /// <summary>The requested stable site identity, or an empty string.</summary>
    private readonly string stableSiteId;

    /// <summary>The requested occurrence, or negative one when absent.</summary>
    private readonly int occurrence;

    /// <summary>The deterministic actionable diagnostic.</summary>
    private readonly string detail;

    /// <summary>Creates one immutable structured preparation rejection.</summary>
    internal LoadedInterceptionPreparationRejection(
        LoadedInterceptionPreparationRejectionReason reason,
        string memberSignature,
        string? stableSiteId,
        int? occurrence,
        string detail)
    {
        this.reason = reason;
        this.memberSignature = memberSignature;
        this.stableSiteId = stableSiteId ?? string.Empty;
        this.occurrence = occurrence ?? -1;
        this.detail = detail;
    }

    /// <summary>Gets the stable rejection category.</summary>
    public LoadedInterceptionPreparationRejectionReason Reason => reason;

    /// <summary>Gets the requested exact member signature.</summary>
    public string MemberSignature => memberSignature;

    /// <summary>Gets the requested stable site identity, or an empty string.</summary>
    public string StableSiteId => stableSiteId;

    /// <summary>Gets the requested occurrence, or negative one when absent.</summary>
    public int Occurrence => occurrence;

    /// <summary>Gets the deterministic actionable diagnostic.</summary>
    public string Detail => detail;
}
