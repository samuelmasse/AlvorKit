using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies one validated disjoint baseline region replaced by a symbolic route.</summary>
internal sealed class LoadedSymbolicEdit
{
    /// <summary>The exact selected site.</summary>
    private readonly LoadedOperationSiteDescriptor site;

    /// <summary>The selected operation instruction.</summary>
    private readonly LoadedIlInstruction operation;

    /// <summary>The accepted owned prefix instructions.</summary>
    private readonly ImmutableArray<LoadedIlInstruction> prefixes;

    /// <summary>The inclusive baseline edit start.</summary>
    private readonly int startOffset;

    /// <summary>The exclusive baseline edit end.</summary>
    private readonly int endOffset;

    /// <summary>Creates one fully validated disjoint edit.</summary>
    internal LoadedSymbolicEdit(
        LoadedOperationSiteDescriptor site,
        LoadedIlInstruction operation,
        ImmutableArray<LoadedIlInstruction> prefixes,
        int startOffset,
        int endOffset)
    {
        this.site = site;
        this.operation = operation;
        this.prefixes = prefixes;
        this.startOffset = startOffset;
        this.endOffset = endOffset;
    }

    /// <summary>Gets the exact selected site.</summary>
    internal LoadedOperationSiteDescriptor Site => site;

    /// <summary>Gets the selected operation instruction.</summary>
    internal LoadedIlInstruction Operation => operation;

    /// <summary>Gets accepted owned prefix instructions.</summary>
    internal ImmutableArray<LoadedIlInstruction> Prefixes => prefixes;

    /// <summary>Gets the inclusive baseline edit start.</summary>
    internal int StartOffset => startOffset;

    /// <summary>Gets the exclusive baseline edit end.</summary>
    internal int EndOffset => endOffset;
}
