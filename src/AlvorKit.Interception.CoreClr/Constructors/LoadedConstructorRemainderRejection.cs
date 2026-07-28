namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies one stable constructor-split rejection category.</summary>
public enum LoadedConstructorRemainderRejectionReason
{
    /// <summary>The body does not contain exactly one direct-base or delegating-this call.</summary>
    InitializerCount,

    /// <summary>A classified initializer token did not resolve to exact instance-constructor metadata.</summary>
    InvalidInitializerMetadata,

    /// <summary>The initializer call has no post-call instruction to move.</summary>
    MissingRemainder,

    /// <summary>An explicit branch enters or leaves the moved suffix across the split.</summary>
    CrossBoundaryBranch,

    /// <summary>An exception clause cannot be retained or moved as one complete unit.</summary>
    CrossBoundaryExceptionRegion,

    /// <summary>The post-initializer boundary does not have a proven empty evaluation stack.</summary>
    NonEmptyEvaluationStack,

    /// <summary>A local is used on both sides of the split and cannot be moved independently.</summary>
    CrossBoundaryLocal,

    /// <summary>A retained control-flow cycle prevents proof that the initializer executes once.</summary>
    PrefixControlFlowCycle
}

/// <summary>Describes one structured deterministic constructor-split rejection.</summary>
public sealed class LoadedConstructorRemainderRejection
{
    /// <summary>The stable rejection category.</summary>
    private readonly LoadedConstructorRemainderRejectionReason reason;

    /// <summary>The primary baseline coordinate.</summary>
    private readonly int baselineOffset;

    /// <summary>The related branch, range, or split coordinate.</summary>
    private readonly int relatedOffset;

    /// <summary>The deterministic actionable diagnostic.</summary>
    private readonly string detail;

    /// <summary>Creates one immutable structured rejection.</summary>
    internal LoadedConstructorRemainderRejection(
        LoadedConstructorRemainderRejectionReason reason,
        int baselineOffset,
        int relatedOffset,
        string detail)
    {
        this.reason = reason;
        this.baselineOffset = baselineOffset;
        this.relatedOffset = relatedOffset;
        this.detail = detail;
    }

    /// <summary>Gets the stable rejection category.</summary>
    public LoadedConstructorRemainderRejectionReason Reason => reason;

    /// <summary>Gets the primary baseline coordinate.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the related branch, range, or split coordinate.</summary>
    public int RelatedOffset => relatedOffset;

    /// <summary>Gets the deterministic actionable diagnostic.</summary>
    public string Detail => detail;
}
