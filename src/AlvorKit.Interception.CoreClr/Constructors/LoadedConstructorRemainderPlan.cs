using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Describes the exact initializer prefix retained in place and constructor remainder moved out.
/// </summary>
public sealed class LoadedConstructorRemainderPlan
{
    /// <summary>The authoritative loaded-body identity.</summary>
    private readonly LoadedMethodBodyIdentity bodyIdentity;

    /// <summary>The direct-base or delegating-this initializer relation.</summary>
    private readonly LoadedConstructorInitializerKind initializerKind;

    /// <summary>The initializer call's immutable baseline offset.</summary>
    private readonly int initializerCallOffset;

    /// <summary>The initializer call's unresolved loaded metadata token.</summary>
    private readonly int initializerMetadataToken;

    /// <summary>The initializer call's exact constructed signature.</summary>
    private readonly string initializerSignature;

    /// <summary>The body prefix, argument evaluation, and initializer call retained in place.</summary>
    private readonly LoadedConstructorRemainderRegion preservedPrefix;

    /// <summary>The exact post-initializer suffix moved into the original remainder.</summary>
    private readonly LoadedConstructorRemainderRegion movedRemainder;

    /// <summary>Exception clauses retained wholly in the constructor prefix.</summary>
    private readonly ImmutableArray<LoadedExceptionRegion> preservedExceptionRegions;

    /// <summary>Exception clauses moved wholly with the constructor remainder.</summary>
    private readonly ImmutableArray<LoadedExceptionRegion> movedExceptionRegions;

    /// <summary>Creates one fully validated constructor split plan.</summary>
    internal LoadedConstructorRemainderPlan(
        LoadedMethodBodyIdentity bodyIdentity,
        LoadedConstructorInitializerKind initializerKind,
        int initializerCallOffset,
        int initializerMetadataToken,
        string initializerSignature,
        LoadedConstructorRemainderRegion preservedPrefix,
        LoadedConstructorRemainderRegion movedRemainder,
        ImmutableArray<LoadedExceptionRegion> preservedExceptionRegions,
        ImmutableArray<LoadedExceptionRegion> movedExceptionRegions)
    {
        this.bodyIdentity = bodyIdentity;
        this.initializerKind = initializerKind;
        this.initializerCallOffset = initializerCallOffset;
        this.initializerMetadataToken = initializerMetadataToken;
        this.initializerSignature = initializerSignature;
        this.preservedPrefix = preservedPrefix;
        this.movedRemainder = movedRemainder;
        this.preservedExceptionRegions = preservedExceptionRegions;
        this.movedExceptionRegions = movedExceptionRegions;
    }

    /// <summary>Gets the authoritative loaded-body identity.</summary>
    public LoadedMethodBodyIdentity BodyIdentity => bodyIdentity;

    /// <summary>Gets whether the exact initializer targets the direct base or current type.</summary>
    public LoadedConstructorInitializerKind InitializerKind => initializerKind;

    /// <summary>Gets the initializer call's immutable baseline offset.</summary>
    public int InitializerCallOffset => initializerCallOffset;

    /// <summary>Gets the initializer call's unresolved loaded metadata token.</summary>
    public int InitializerMetadataToken => initializerMetadataToken;

    /// <summary>Gets the initializer call's exact constructed signature.</summary>
    public string InitializerSignature => initializerSignature;

    /// <summary>
    /// Gets the unchanged prefix containing argument evaluation and the initializer call.
    /// </summary>
    public LoadedConstructorRemainderRegion PreservedPrefix =>
        preservedPrefix;

    /// <summary>Gets the exact suffix moved into the callable original remainder.</summary>
    public LoadedConstructorRemainderRegion MovedRemainder =>
        movedRemainder;

    /// <summary>Gets clauses retained wholly in the unchanged constructor prefix.</summary>
    public ImmutableArray<LoadedExceptionRegion> PreservedExceptionRegions =>
        preservedExceptionRegions;

    /// <summary>Gets clauses moved wholly with the original remainder.</summary>
    public ImmutableArray<LoadedExceptionRegion> MovedExceptionRegions =>
        movedExceptionRegions;
}
