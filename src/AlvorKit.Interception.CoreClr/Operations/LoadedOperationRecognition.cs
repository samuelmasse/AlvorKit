using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Returns either pristine supported sites or structured deterministic rejections.</summary>
public sealed class LoadedOperationRecognition
{
    /// <summary>The supported exact sites, empty whenever any rejection exists.</summary>
    private readonly ImmutableArray<LoadedOperationSiteDescriptor> sites;

    /// <summary>The structured rejections in baseline order.</summary>
    private readonly ImmutableArray<LoadedOperationRejection> rejections;

    /// <summary>Creates one immutable pristine recognition result.</summary>
    internal LoadedOperationRecognition(
        ImmutableArray<LoadedOperationSiteDescriptor> sites,
        ImmutableArray<LoadedOperationRejection> rejections)
    {
        this.sites = sites;
        this.rejections = rejections;
    }

    /// <summary>Gets whether every candidate operation was recognized safely.</summary>
    public bool IsSuccessful => rejections.IsEmpty;

    /// <summary>Gets supported exact sites, empty whenever any rejection exists.</summary>
    public ImmutableArray<LoadedOperationSiteDescriptor> Sites => sites;

    /// <summary>Gets structured rejections in baseline order.</summary>
    public ImmutableArray<LoadedOperationRejection> Rejections =>
        rejections;
}
