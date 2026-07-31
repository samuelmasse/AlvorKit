namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>One selected-assembly frame resolved to a readable method and optional source line.</summary>
/// <param name="Method">Readable managed method identity.</param>
/// <param name="Document">Portable PDB document path, when available.</param>
/// <param name="Line">One-based source line, when available.</param>
public readonly record struct InterceptionAllocationSourceFrame(
    string Method,
    string? Document,
    int? Line);

/// <summary>One sampled stack and the number of exact capture ordinals it represents.</summary>
/// <param name="Weight">Number of capture ordinals represented by the stack.</param>
/// <param name="Frames">Source-resolved frames in root-to-leaf order.</param>
public record InterceptionAllocationSourceSample(
    ulong Weight,
    IReadOnlyList<InterceptionAllocationSourceFrame> Frames);

/// <summary>Aggregated allocation attribution for one source line.</summary>
/// <param name="Method">Readable managed method identity.</param>
/// <param name="Document">Portable PDB document path.</param>
/// <param name="Line">One-based source line.</param>
/// <param name="AttributedObjectAllocations">Weighted object count attributed to the line.</param>
/// <param name="RetainedSamples">Number of retained stacks attributed to the line.</param>
public readonly record struct InterceptionAllocationLine(
    string Method,
    string Document,
    int Line,
    ulong AttributedObjectAllocations,
    uint RetainedSamples);
