namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Maps one immutable baseline coordinate to a symbolic generation instruction.</summary>
public sealed class LoadedSymbolicIlMapEntry
{
    /// <summary>The original loaded IL coordinate.</summary>
    private readonly int baselineOffset;

    /// <summary>The corresponding symbolic instruction ordinal.</summary>
    private readonly int instructionIndex;

    /// <summary>The symbolic label retained for branches, EH, and diagnostics.</summary>
    private readonly string label;

    /// <summary>Creates one original-to-symbolic map entry.</summary>
    internal LoadedSymbolicIlMapEntry(
        int baselineOffset,
        int instructionIndex,
        string label)
    {
        this.baselineOffset = baselineOffset;
        this.instructionIndex = instructionIndex;
        this.label = label;
    }

    /// <summary>Gets the original loaded IL coordinate.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the corresponding symbolic instruction ordinal.</summary>
    public int InstructionIndex => instructionIndex;

    /// <summary>Gets the retained symbolic baseline label.</summary>
    public string Label => label;
}
