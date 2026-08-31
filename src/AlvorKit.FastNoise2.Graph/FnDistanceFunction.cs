namespace AlvorKit;

/// <summary>Identifies a distance function supported by cellular and point-distance nodes.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are resolved by exact metadata name.</remarks>
public enum FnDistanceFunction
{
    /// <summary>Uses the square root of the sum of squared axis distances.</summary>
    Euclidean,

    /// <summary>Uses the sum of squared axis distances, avoiding a square root and changing the distance scale.</summary>
    EuclideanSquared,

    /// <summary>Uses the sum of absolute axis distances.</summary>
    Manhattan,

    /// <summary>Uses Euclidean-squared distance plus Manhattan distance.</summary>
    Hybrid,

    /// <summary>Uses the greatest absolute distance on any axis.</summary>
    MaximumAxis,

    /// <summary>Uses the configurable p-norm whose exponent is <see cref="FnHybrid.MinkowskiP"/>.</summary>
    Minkowski,
}
