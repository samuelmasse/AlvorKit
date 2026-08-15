using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Returns a complete selected symbolic generation or only pristine diagnostics.
/// </summary>
public sealed class LoadedInterceptionPreparationResult
{
    /// <summary>The deterministic recognition and selection preview.</summary>
    private readonly LoadedInterceptionPreparationPreview preview;

    /// <summary>The complete selected symbolic generation, or null.</summary>
    private readonly LoadedSymbolicMethodGeneration? generation;

    /// <summary>The deterministic symbolic-composition rejections.</summary>
    private readonly ImmutableArray<LoadedSymbolicCompositionRejection>
        compositionRejections;

    /// <summary>Creates one immutable preparation result.</summary>
    internal LoadedInterceptionPreparationResult(
        LoadedInterceptionPreparationPreview preview,
        LoadedSymbolicMethodGeneration? generation,
        ImmutableArray<LoadedSymbolicCompositionRejection>
            compositionRejections)
    {
        this.preview = preview;
        this.generation = generation;
        this.compositionRejections = compositionRejections;
    }

    /// <summary>Gets whether one complete selected generation was composed.</summary>
    public bool IsSuccessful =>
        preview.IsSuccessful &&
        generation is not null &&
        compositionRejections.IsEmpty;

    /// <summary>Gets the deterministic recognition and selection preview.</summary>
    public LoadedInterceptionPreparationPreview Preview => preview;

    /// <summary>Gets the complete selected symbolic generation, or null.</summary>
    public LoadedSymbolicMethodGeneration? Generation => generation;

    /// <summary>Gets deterministic symbolic-composition rejections.</summary>
    public ImmutableArray<LoadedSymbolicCompositionRejection>
        CompositionRejections => compositionRejections;
}
