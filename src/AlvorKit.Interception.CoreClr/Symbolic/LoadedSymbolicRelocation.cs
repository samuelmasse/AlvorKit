namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies metadata that native lowering must create or reuse safely.</summary>
public enum LoadedSymbolicRelocationKind
{
    /// <summary>An augmented local signature containing exact operand spill locals.</summary>
    ExactOperandLocals,

    /// <summary>The neutral route-resolution helper MemberRef or MethodSpec.</summary>
    RouteResolverMethod,

    /// <summary>The exact StandAloneSig consumed by <c>calli</c>.</summary>
    CallSiteSignature,

    /// <summary>A constructed caller method-handle token.</summary>
    ConstructedMethodHandle,

    /// <summary>A constructed caller declaring-type handle token.</summary>
    ConstructedTypeHandle
}

/// <summary>Describes one symbolic metadata operand without assigning a token.</summary>
public sealed class LoadedSymbolicRelocation
{
    /// <summary>The stable relocation kind.</summary>
    private readonly LoadedSymbolicRelocationKind kind;

    /// <summary>The symbolic instruction ordinal containing the future operand.</summary>
    private readonly int instructionIndex;

    /// <summary>The deterministic symbolic relocation name.</summary>
    private readonly string symbol;

    /// <summary>The owning exact site identity.</summary>
    private readonly string siteId;

    /// <summary>The exact constructed signature or context needed for token emission.</summary>
    private readonly string signature;

    /// <summary>Creates one token-free symbolic metadata relocation.</summary>
    internal LoadedSymbolicRelocation(
        LoadedSymbolicRelocationKind kind,
        int instructionIndex,
        string symbol,
        string siteId,
        string signature)
    {
        this.kind = kind;
        this.instructionIndex = instructionIndex;
        this.symbol = symbol;
        this.siteId = siteId;
        this.signature = signature;
    }

    /// <summary>Gets the stable relocation kind.</summary>
    public LoadedSymbolicRelocationKind Kind => kind;

    /// <summary>Gets the symbolic instruction ordinal containing the future operand.</summary>
    public int InstructionIndex => instructionIndex;

    /// <summary>Gets the deterministic symbolic relocation name.</summary>
    public string Symbol => symbol;

    /// <summary>Gets the owning exact site identity.</summary>
    public string SiteId => siteId;

    /// <summary>Gets the exact constructed signature or context needed for token emission.</summary>
    public string Signature => signature;
}
