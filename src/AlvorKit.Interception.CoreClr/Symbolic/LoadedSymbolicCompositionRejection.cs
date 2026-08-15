namespace AlvorKit;

/// <summary>Identifies a stable caller-composition rejection category.</summary>
public enum LoadedSymbolicCompositionRejectionReason
{
    /// <summary>A site belongs to another authoritative body snapshot.</summary>
    StaleBodyIdentity,

    /// <summary>A site location or versioned identity no longer matches the request.</summary>
    StaleSiteIdentity,

    /// <summary>The baseline instruction no longer matches the site descriptor.</summary>
    StaleOperation,

    /// <summary>An accepted prefix no longer matches the loaded baseline.</summary>
    StalePrefix,

    /// <summary>Two requested site edit regions intersect.</summary>
    OverlappingEdit
}

/// <summary>Describes one deterministic symbolic-composition rejection.</summary>
public sealed class LoadedSymbolicCompositionRejection
{
    /// <summary>The stable rejection category.</summary>
    private readonly LoadedSymbolicCompositionRejectionReason reason;

    /// <summary>The affected baseline coordinate.</summary>
    private readonly int baselineOffset;

    /// <summary>The related conflicting coordinate, or the same coordinate.</summary>
    private readonly int relatedOffset;

    /// <summary>The affected exact site identity, or an empty string.</summary>
    private readonly string siteId;

    /// <summary>The deterministic actionable diagnostic.</summary>
    private readonly string detail;

    /// <summary>Creates one structured composition rejection.</summary>
    internal LoadedSymbolicCompositionRejection(
        LoadedSymbolicCompositionRejectionReason reason,
        int baselineOffset,
        int relatedOffset,
        string siteId,
        string detail)
    {
        this.reason = reason;
        this.baselineOffset = baselineOffset;
        this.relatedOffset = relatedOffset;
        this.siteId = siteId;
        this.detail = detail;
    }

    /// <summary>Gets the stable rejection category.</summary>
    public LoadedSymbolicCompositionRejectionReason Reason => reason;

    /// <summary>Gets the affected baseline coordinate.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the related conflicting coordinate, or the same coordinate.</summary>
    public int RelatedOffset => relatedOffset;

    /// <summary>Gets the affected exact site identity, or an empty string.</summary>
    public string SiteId => siteId;

    /// <summary>Gets the deterministic actionable diagnostic.</summary>
    public string Detail => detail;
}
