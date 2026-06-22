namespace AlvorKit.Engine;

/// <summary>One allocation slot inside an <see cref="Allocator"/>.</summary>
[ExcludeFromCodeCoverage(Justification = "Data shape owned by the allocator coverage exception.")]
public struct Allocation
{
    /// <summary>Gets or sets the unaligned backing index.</summary>
    public long Index;

    /// <summary>Gets or sets the requested payload size.</summary>
    public long Size;

    /// <summary>Gets or sets the requested byte alignment.</summary>
    public int Alignment;

    /// <summary>Gets or sets the dense rank inside the live allocation list.</summary>
    public int Rank;
}
