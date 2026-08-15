using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Returns either one complete symbolic generation or pristine rejections.</summary>
public sealed class LoadedSymbolicComposition
{
    /// <summary>The complete generation, or null when validation rejected.</summary>
    private readonly LoadedSymbolicMethodGeneration? generation;

    /// <summary>The structured deterministic validation rejections.</summary>
    private readonly ImmutableArray<LoadedSymbolicCompositionRejection> rejections;

    /// <summary>Creates one immutable symbolic-composition result.</summary>
    internal LoadedSymbolicComposition(
        LoadedSymbolicMethodGeneration? generation,
        ImmutableArray<LoadedSymbolicCompositionRejection> rejections)
    {
        this.generation = generation;
        this.rejections = rejections;
    }

    /// <summary>Gets whether a complete generation was produced.</summary>
    public bool IsSuccessful => generation is not null;

    /// <summary>Gets the complete generation, or null when validation rejected.</summary>
    public LoadedSymbolicMethodGeneration? Generation => generation;

    /// <summary>Gets structured deterministic validation rejections.</summary>
    public ImmutableArray<LoadedSymbolicCompositionRejection> Rejections =>
        rejections;
}
