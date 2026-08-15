using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Describes one complete immutable symbolic caller generation.</summary>
public sealed class LoadedSymbolicMethodGeneration
{
    /// <summary>The deterministic identity of this complete composition.</summary>
    private readonly string identity;

    /// <summary>The authoritative baseline body identity.</summary>
    private readonly LoadedMethodBodyIdentity bodyIdentity;

    /// <summary>The loaded module version containing the caller.</summary>
    private readonly Guid moduleVersionId;

    /// <summary>The caller MethodDef token.</summary>
    private readonly int containingMethodToken;

    /// <summary>The exact constructed caller context.</summary>
    private readonly string constructedContext;

    /// <summary>The baseline declared maximum stack.</summary>
    private readonly ushort baselineMaxStack;

    /// <summary>Whether baseline local storage is initialized.</summary>
    private readonly bool initLocals;

    /// <summary>The existing baseline local-signature token.</summary>
    private readonly int baselineLocalSignatureToken;

    /// <summary>The complete symbolic instruction stream.</summary>
    private readonly ImmutableArray<LoadedSymbolicInstruction> instructions;

    /// <summary>The symbolically remapped exception clauses.</summary>
    private readonly ImmutableArray<LoadedSymbolicExceptionRegion> exceptionRegions;

    /// <summary>The token-free metadata relocation requests.</summary>
    private readonly ImmutableArray<LoadedSymbolicRelocation> relocations;

    /// <summary>The original-to-symbolic IL map.</summary>
    private readonly ImmutableArray<LoadedSymbolicIlMapEntry> ilMap;

    /// <summary>The exact sites composed into this generation.</summary>
    private readonly ImmutableArray<LoadedOperationSiteDescriptor> sites;

    /// <summary>Creates one fully validated immutable symbolic generation.</summary>
    internal LoadedSymbolicMethodGeneration(
        string identity,
        LoadedMethodBodyIdentity bodyIdentity,
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        ushort baselineMaxStack,
        bool initLocals,
        int baselineLocalSignatureToken,
        ImmutableArray<LoadedSymbolicInstruction> instructions,
        ImmutableArray<LoadedSymbolicExceptionRegion> exceptionRegions,
        ImmutableArray<LoadedSymbolicRelocation> relocations,
        ImmutableArray<LoadedSymbolicIlMapEntry> ilMap,
        ImmutableArray<LoadedOperationSiteDescriptor> sites)
    {
        this.identity = identity;
        this.bodyIdentity = bodyIdentity;
        this.moduleVersionId = moduleVersionId;
        this.containingMethodToken = containingMethodToken;
        this.constructedContext = constructedContext;
        this.baselineMaxStack = baselineMaxStack;
        this.initLocals = initLocals;
        this.baselineLocalSignatureToken = baselineLocalSignatureToken;
        this.instructions = instructions;
        this.exceptionRegions = exceptionRegions;
        this.relocations = relocations;
        this.ilMap = ilMap;
        this.sites = sites;
    }

    /// <summary>Gets the deterministic identity of this complete composition.</summary>
    public string Identity => identity;

    /// <summary>Gets the authoritative baseline body identity.</summary>
    public LoadedMethodBodyIdentity BodyIdentity => bodyIdentity;

    /// <summary>Gets the loaded module version containing the caller.</summary>
    public Guid ModuleVersionId => moduleVersionId;

    /// <summary>Gets the caller MethodDef token.</summary>
    public int ContainingMethodToken => containingMethodToken;

    /// <summary>Gets the exact constructed caller context.</summary>
    public string ConstructedContext => constructedContext;

    /// <summary>Gets the baseline declared maximum stack.</summary>
    public ushort BaselineMaxStack => baselineMaxStack;

    /// <summary>Gets whether baseline local storage is initialized.</summary>
    public bool InitLocals => initLocals;

    /// <summary>Gets the existing baseline local-signature token.</summary>
    public int BaselineLocalSignatureToken => baselineLocalSignatureToken;

    /// <summary>Gets whether lowering must recompute max stack after expansion.</summary>
    public bool RequiresMaxStackRecompute => !sites.IsEmpty;

    /// <summary>Gets the complete symbolic instruction stream.</summary>
    public ImmutableArray<LoadedSymbolicInstruction> Instructions =>
        instructions;

    /// <summary>Gets the symbolically remapped exception clauses.</summary>
    public ImmutableArray<LoadedSymbolicExceptionRegion> ExceptionRegions =>
        exceptionRegions;

    /// <summary>Gets the token-free metadata relocation requests.</summary>
    public ImmutableArray<LoadedSymbolicRelocation> Relocations =>
        relocations;

    /// <summary>Gets the original-to-symbolic IL map.</summary>
    public ImmutableArray<LoadedSymbolicIlMapEntry> IlMap => ilMap;

    /// <summary>Gets exact sites in baseline edit order.</summary>
    public ImmutableArray<LoadedOperationSiteDescriptor> Sites => sites;
}
