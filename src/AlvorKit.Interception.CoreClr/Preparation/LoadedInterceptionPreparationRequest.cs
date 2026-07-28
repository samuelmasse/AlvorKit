namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Selects one exact operation from one exact source and executable loaded body.
/// </summary>
public sealed class LoadedInterceptionPreparationRequest
{
    /// <summary>The exact source/body target resolved from code-first metadata.</summary>
    private readonly LoadedSourceMethodTarget caller;

    /// <summary>The body identity expected by the code-first plan.</summary>
    private readonly LoadedMethodBodyIdentity expectedBodyIdentity;

    /// <summary>The exact canonical operation member signature.</summary>
    private readonly string memberSignature;

    /// <summary>The exact constructed declaring-type and method context.</summary>
    private readonly string constructedContext;

    /// <summary>The optional exact stable site identity.</summary>
    private readonly string? stableSiteId;

    /// <summary>The optional zero-based occurrence among signature matches.</summary>
    private readonly int? occurrence;

    /// <summary>Creates one immutable code-first operation-selection request.</summary>
    public LoadedInterceptionPreparationRequest(
        LoadedSourceMethodTarget caller,
        LoadedMethodBodyIdentity expectedBodyIdentity,
        string memberSignature,
        string constructedContext = "",
        string? stableSiteId = null,
        int? occurrence = null)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(expectedBodyIdentity);
        ArgumentNullException.ThrowIfNull(memberSignature);
        ArgumentNullException.ThrowIfNull(constructedContext);

        this.caller = caller;
        this.expectedBodyIdentity = expectedBodyIdentity;
        this.memberSignature = memberSignature;
        this.constructedContext = constructedContext;
        this.stableSiteId = stableSiteId;
        this.occurrence = occurrence;
    }

    /// <summary>Gets the exact selected source and executable loaded body.</summary>
    public LoadedSourceMethodTarget Caller => caller;

    /// <summary>Gets the body identity expected by the code-first plan.</summary>
    public LoadedMethodBodyIdentity ExpectedBodyIdentity =>
        expectedBodyIdentity;

    /// <summary>Gets the exact canonical operation member signature.</summary>
    public string MemberSignature => memberSignature;

    /// <summary>Gets the exact constructed declaring-type and method context.</summary>
    public string ConstructedContext => constructedContext;

    /// <summary>Gets the optional exact stable site identity.</summary>
    public string? StableSiteId => stableSiteId;

    /// <summary>Gets the optional zero-based occurrence among signature matches.</summary>
    public int? Occurrence => occurrence;
}
