namespace AlvorKit;

/// <summary>The coordinate removed by a RemoveDimension node.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are resolved by exact metadata name.</remarks>
public enum FnRemovedDimension
{
    /// <summary>Removes the X coordinate.</summary>
    X,

    /// <summary>Removes the Y coordinate. This is the runtime default.</summary>
    Y,

    /// <summary>Removes the Z coordinate.</summary>
    Z,

    /// <summary>Removes the W coordinate.</summary>
    W,
}
