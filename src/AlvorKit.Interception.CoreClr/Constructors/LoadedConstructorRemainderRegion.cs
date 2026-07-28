using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Describes one contiguous instruction region in immutable baseline coordinates.</summary>
public sealed class LoadedConstructorRemainderRegion
{
    /// <summary>The region's first baseline offset.</summary>
    private readonly int startOffset;

    /// <summary>The first baseline offset outside the region.</summary>
    private readonly int endOffset;

    /// <summary>The exact baseline instructions retained by the region.</summary>
    private readonly ImmutableArray<LoadedIlInstruction> instructions;

    /// <summary>Creates one nonempty region from a validated contiguous instruction sequence.</summary>
    internal LoadedConstructorRemainderRegion(
        int startOffset,
        int endOffset,
        ImmutableArray<LoadedIlInstruction> instructions)
    {
        this.startOffset = startOffset;
        this.endOffset = endOffset;
        this.instructions = instructions;
    }

    /// <summary>Gets the region's first baseline offset.</summary>
    public int StartOffset => startOffset;

    /// <summary>Gets the first baseline offset outside the region.</summary>
    public int EndOffset => endOffset;

    /// <summary>Gets the region length in loaded IL bytes.</summary>
    public int Length => checked(endOffset - startOffset);

    /// <summary>Gets the exact immutable baseline instructions in this region.</summary>
    public ImmutableArray<LoadedIlInstruction> Instructions => instructions;

    /// <summary>Returns the stable half-open loaded-IL range.</summary>
    public override string ToString() =>
        $"[IL_{startOffset:X4}, IL_{endOffset:X4})";
}
