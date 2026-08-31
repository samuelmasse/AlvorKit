namespace AlvorKit;

/// <summary>Identifies a displacement-vector algorithm supported by simplex domain-warp nodes.</summary>
/// <remarks>
/// This is a noise algorithm choice, not a CPU SIMD feature set. Managed numeric values are wrapper implementation
/// details and are resolved by exact metadata name.
/// </remarks>
public enum FnVectorizationScheme
{
    /// <summary>Builds domain-warp vectors using FastNoise2's orthogonal gradient-matrix scheme.</summary>
    OrthogonalGradientMatrix,

    /// <summary>Builds domain-warp vectors using FastNoise2's gradient outer-product scheme.</summary>
    GradientOuterProduct,
}
