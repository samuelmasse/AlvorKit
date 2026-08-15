using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Exposes deterministic recognition and exact selection before composition.
/// </summary>
public sealed class LoadedInterceptionPreparationPreview
{
    /// <summary>The immutable code-first selection request.</summary>
    private readonly LoadedInterceptionPreparationRequest request;

    /// <summary>All pristine sites recognized in baseline order.</summary>
    private readonly ImmutableArray<LoadedOperationSiteDescriptor> resolvedSites;

    /// <summary>The one selected site, or an empty array on rejection.</summary>
    private readonly ImmutableArray<LoadedOperationSiteDescriptor> selectedSites;

    /// <summary>The semantic recognition rejections.</summary>
    private readonly ImmutableArray<LoadedOperationRejection>
        recognitionRejections;

    /// <summary>The exact selection rejections.</summary>
    private readonly ImmutableArray<LoadedInterceptionPreparationRejection>
        selectionRejections;

    /// <summary>Creates one immutable preview result.</summary>
    internal LoadedInterceptionPreparationPreview(
        LoadedInterceptionPreparationRequest request,
        ImmutableArray<LoadedOperationSiteDescriptor> resolvedSites,
        ImmutableArray<LoadedOperationSiteDescriptor> selectedSites,
        ImmutableArray<LoadedOperationRejection> recognitionRejections,
        ImmutableArray<LoadedInterceptionPreparationRejection>
            selectionRejections)
    {
        this.request = request;
        this.resolvedSites = resolvedSites;
        this.selectedSites = selectedSites;
        this.recognitionRejections = recognitionRejections;
        this.selectionRejections = selectionRejections;
    }

    /// <summary>Gets whether recognition and exact selection both succeeded.</summary>
    public bool IsSuccessful =>
        recognitionRejections.IsEmpty &&
        selectionRejections.IsEmpty &&
        selectedSites.Length == 1;

    /// <summary>Gets the immutable code-first selection request.</summary>
    public LoadedInterceptionPreparationRequest Request => request;

    /// <summary>Gets all pristine sites recognized in baseline order.</summary>
    public ImmutableArray<LoadedOperationSiteDescriptor> ResolvedSites =>
        resolvedSites;

    /// <summary>Gets the one selected site, or an empty array on rejection.</summary>
    public ImmutableArray<LoadedOperationSiteDescriptor> SelectedSites =>
        selectedSites;

    /// <summary>Gets semantic recognition rejections.</summary>
    public ImmutableArray<LoadedOperationRejection> RecognitionRejections =>
        recognitionRejections;

    /// <summary>Gets exact selection rejections.</summary>
    public ImmutableArray<LoadedInterceptionPreparationRejection>
        SelectionRejections => selectionRejections;
}
