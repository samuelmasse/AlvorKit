namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies one stable source-to-loaded-body resolution failure.</summary>
public enum LoadedSourceMethodRejectionReason
{
    /// <summary>The selected source cannot identify one exact supported MethodDef.</summary>
    UnsupportedSourceMethod,

    /// <summary>More than one supported state-machine marker owns the source method.</summary>
    AmbiguousStateMachineMetadata,

    /// <summary>A state-machine marker has an unsupported type or argument shape.</summary>
    UnsupportedStateMachineMetadata,

    /// <summary>The generated state-machine type has no exact executable <c>MoveNext</c>.</summary>
    MissingMoveNextBody,

    /// <summary>The generated state-machine type has multiple exact <c>MoveNext</c> candidates.</summary>
    AmbiguousMoveNextBody,

    /// <summary>The backend did not supply an authoritative loaded body for the target MethodDef.</summary>
    MissingLoadedBody
}

/// <summary>Describes one structured deterministic source-targeting rejection.</summary>
public sealed class LoadedSourceMethodRejection
{
    /// <summary>The stable rejection category.</summary>
    private readonly LoadedSourceMethodRejectionReason reason;

    /// <summary>The selected source MethodDef token, or zero when unavailable.</summary>
    private readonly int sourceMethodToken;

    /// <summary>The related state-machine type or method name.</summary>
    private readonly string relatedMetadata;

    /// <summary>The deterministic actionable diagnostic.</summary>
    private readonly string detail;

    /// <summary>Creates one immutable structured rejection.</summary>
    internal LoadedSourceMethodRejection(
        LoadedSourceMethodRejectionReason reason,
        int sourceMethodToken,
        string relatedMetadata,
        string detail)
    {
        this.reason = reason;
        this.sourceMethodToken = sourceMethodToken;
        this.relatedMetadata = relatedMetadata;
        this.detail = detail;
    }

    /// <summary>Gets the stable rejection category.</summary>
    public LoadedSourceMethodRejectionReason Reason => reason;

    /// <summary>Gets the selected source MethodDef token, or zero.</summary>
    public int SourceMethodToken => sourceMethodToken;

    /// <summary>Gets the related state-machine type or method name.</summary>
    public string RelatedMetadata => relatedMetadata;

    /// <summary>Gets the deterministic actionable diagnostic.</summary>
    public string Detail => detail;
}
