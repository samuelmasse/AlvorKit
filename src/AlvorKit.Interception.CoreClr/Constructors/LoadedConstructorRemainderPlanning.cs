using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Returns either one pristine constructor split or deterministic rejections.</summary>
public sealed class LoadedConstructorRemainderPlanning
{
    /// <summary>The safe constructor split, or null on rejection.</summary>
    private readonly LoadedConstructorRemainderPlan? plan;

    /// <summary>The structured rejections in deterministic baseline order.</summary>
    private readonly ImmutableArray<LoadedConstructorRemainderRejection> rejections;

    /// <summary>Creates one immutable planning result.</summary>
    internal LoadedConstructorRemainderPlanning(
        LoadedConstructorRemainderPlan? plan,
        ImmutableArray<LoadedConstructorRemainderRejection> rejections)
    {
        this.plan = plan;
        this.rejections = rejections;
    }

    /// <summary>Gets whether a safe split was produced without any rejection.</summary>
    public bool IsSuccessful => plan is not null && rejections.IsEmpty;

    /// <summary>Gets the safe split, or null when planning rejected the body.</summary>
    public LoadedConstructorRemainderPlan? Plan => plan;

    /// <summary>Gets structured rejections in deterministic baseline order.</summary>
    public ImmutableArray<LoadedConstructorRemainderRejection> Rejections =>
        rejections;
}
