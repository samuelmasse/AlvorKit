using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies a baseline instruction or one symbolic caller-route operation.</summary>
public enum LoadedSymbolicInstructionKind
{
    /// <summary>An unchanged instruction copied from the authoritative baseline.</summary>
    Baseline,

    /// <summary>Declares and stores the operation's exact stack operands in fresh locals.</summary>
    SpillOperands,

    /// <summary>Resolves the inert physical site and construction-specific route.</summary>
    ResolveRoute,

    /// <summary>Branches to the active route when its resolved pointer is nonzero.</summary>
    BranchIfRoute,

    /// <summary>Reloads exact operation operands from symbolic fresh locals.</summary>
    ReloadOperands,

    /// <summary>Replays one accepted original-operation prefix.</summary>
    ReplayPrefix,

    /// <summary>Replays the exact original operation on a route miss.</summary>
    ReplayOriginal,

    /// <summary>Branches from original replay to the common result merge.</summary>
    Branch,

    /// <summary>Invokes the active exact route through a symbolic call-site signature.</summary>
    CallIndirect,

    /// <summary>Marks the common stack-equivalent result continuation.</summary>
    Merge,

    /// <summary>Marks the symbolic end of the rewritten IL stream.</summary>
    End
}

/// <summary>Describes one immutable instruction in a symbolic rewritten generation.</summary>
public sealed class LoadedSymbolicInstruction
{
    /// <summary>The symbolic operation kind.</summary>
    private readonly LoadedSymbolicInstructionKind kind;

    /// <summary>All symbolic labels attached to this instruction.</summary>
    private readonly ImmutableArray<string> labels;

    /// <summary>The related immutable baseline coordinate, or minus one.</summary>
    private readonly int baselineOffset;

    /// <summary>The original opcode for baseline or replay instructions, or zero.</summary>
    private readonly ushort opCodeValue;

    /// <summary>The immutable original operand for baseline or replay instructions.</summary>
    private readonly LoadedIlOperand operand;

    /// <summary>Symbolic branch and switch targets in original operand order.</summary>
    private readonly ImmutableArray<string> targetLabels;

    /// <summary>The stable site identity for a synthetic route operation, or empty.</summary>
    private readonly string siteId;

    /// <summary>The exact site signature for symbolic local and calli lowering, or empty.</summary>
    private readonly string canonicalSignature;

    /// <summary>Creates one internally validated symbolic instruction.</summary>
    internal LoadedSymbolicInstruction(
        LoadedSymbolicInstructionKind kind,
        ImmutableArray<string> labels,
        int baselineOffset,
        ushort opCodeValue,
        LoadedIlOperand operand,
        ImmutableArray<string> targetLabels,
        string siteId,
        string canonicalSignature)
    {
        this.kind = kind;
        this.labels = labels;
        this.baselineOffset = baselineOffset;
        this.opCodeValue = opCodeValue;
        this.operand = operand;
        this.targetLabels = targetLabels;
        this.siteId = siteId;
        this.canonicalSignature = canonicalSignature;
    }

    /// <summary>Gets the symbolic operation kind.</summary>
    public LoadedSymbolicInstructionKind Kind => kind;

    /// <summary>Gets all symbolic labels attached to this instruction.</summary>
    public ImmutableArray<string> Labels => labels;

    /// <summary>Gets the related immutable baseline coordinate, or minus one.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the original opcode for baseline or replay instructions, or zero.</summary>
    public ushort OpCodeValue => opCodeValue;

    /// <summary>Gets the immutable original operand for baseline or replay instructions.</summary>
    public LoadedIlOperand Operand => operand;

    /// <summary>Gets symbolic branch and switch targets in original operand order.</summary>
    public ImmutableArray<string> TargetLabels => targetLabels;

    /// <summary>Gets the stable site identity for a synthetic route operation, or empty.</summary>
    public string SiteId => siteId;

    /// <summary>Gets the exact signature for symbolic local and calli lowering, or empty.</summary>
    public string CanonicalSignature => canonicalSignature;
}
