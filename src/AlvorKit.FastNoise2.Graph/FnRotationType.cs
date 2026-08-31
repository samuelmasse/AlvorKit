namespace AlvorKit;

/// <summary>Identifies a plane preset supported by <see cref="FnNodeType.DomainRotatePlane"/>.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are resolved by exact metadata name.</remarks>
public enum FnRotationType
{
    /// <summary>Optimizes a three-dimensional domain for sources whose important features lie in XY planes.</summary>
    ImproveXyPlanes,

    /// <summary>Optimizes a three-dimensional domain for sources whose important features lie in XZ planes.</summary>
    ImproveXzPlanes,
}
