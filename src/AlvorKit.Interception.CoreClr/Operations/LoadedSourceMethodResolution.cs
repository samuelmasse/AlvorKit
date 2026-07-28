using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Returns either one exact source/body target or structured deterministic rejections.</summary>
public sealed class LoadedSourceMethodResolution
{
    /// <summary>The exact source/body target, or null on rejection.</summary>
    private readonly LoadedSourceMethodTarget? target;

    /// <summary>The structured deterministic rejections.</summary>
    private readonly ImmutableArray<LoadedSourceMethodRejection> rejections;

    /// <summary>Creates one immutable source-targeting result.</summary>
    internal LoadedSourceMethodResolution(
        LoadedSourceMethodTarget? target,
        ImmutableArray<LoadedSourceMethodRejection> rejections)
    {
        this.target = target;
        this.rejections = rejections;
    }

    /// <summary>Gets whether exactly one executable loaded body was resolved.</summary>
    public bool IsSuccessful => target is not null && rejections.IsEmpty;

    /// <summary>Gets the exact source/body target, or null when resolution rejected.</summary>
    public LoadedSourceMethodTarget? Target => target;

    /// <summary>Gets structured deterministic rejections.</summary>
    public ImmutableArray<LoadedSourceMethodRejection> Rejections =>
        rejections;
}
