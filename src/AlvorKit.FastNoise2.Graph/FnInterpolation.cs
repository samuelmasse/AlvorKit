namespace AlvorKit;

/// <summary>Identifies the interpolation curve used by <see cref="FnNodeType.Fade"/>.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are resolved by exact metadata name.</remarks>
public enum FnInterpolation
{
    /// <summary>Uses an uncurved interpolation parameter.</summary>
    Linear,

    /// <summary>Uses the cubic smoothstep curve 3t² - 2t³.</summary>
    Hermite,

    /// <summary>Uses the quintic smootherstep curve 6t⁵ - 15t⁴ + 10t³.</summary>
    Quintic,
}
