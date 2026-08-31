namespace AlvorKit;

/// <summary>Identifies how CellularDistance combines its two selected distance ranks.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are resolved by exact metadata name.</remarks>
public enum FnCellularReturnType
{
    /// <summary>Returns the distance rank selected by <see cref="FnIntegerVariable.DistanceIndex0"/>.</summary>
    Index0,

    /// <summary>Returns distance index 0 plus distance index 1.</summary>
    Index0Add1,

    /// <summary>Returns the absolute difference between distance index 0 and distance index 1.</summary>
    Index0AbsoluteDifference1,

    /// <summary>Returns distance index 0 multiplied by distance index 1.</summary>
    Index0Multiply1,

    /// <summary>Returns distance index 0 divided by distance index 1.</summary>
    Index0Divide1,
}
