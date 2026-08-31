namespace AlvorKit;

/// <summary>Identifies every node type exposed by the FastNoise2 1.1.1 runtime metadata catalog.</summary>
/// <remarks>
/// A node type describes an operation in a FastNoise2 graph, not a fixed output dimensionality. The same graph can
/// normally be sampled in two, three, or four dimensions. Configure a created node only with the variables, hybrids,
/// enum options, and required sources documented for that node.
/// Managed numeric values are wrapper implementation details, not native metadata IDs or stable serialized values.
/// </remarks>
public enum FnNodeType
{
    /// <summary>Returns one constant value at every position. Configure <see cref="FnFloatVariable.Value"/>.</summary>
    Constant,

    /// <summary>Returns an uncorrelated random value in the configured output range at every position.</summary>
    White,

    /// <summary>Alternates between -1 and 1 in axis-aligned cells whose size is the feature scale.</summary>
    Checkerboard,

    /// <summary>Multiplies <c>sin(coordinate / FeatureScale)</c> across every active axis.</summary>
    SineWave,

    /// <summary>Returns the sum of each input coordinate, after applying its offset and multiplier.</summary>
    Gradient,

    /// <summary>Returns the selected distance from each sampled position to a configurable point.</summary>
    DistanceToPoint,

    /// <summary>Generates smooth simplex noise on a simplex grid.</summary>
    Simplex,

    /// <summary>Generates smoother simplex noise at a higher computational cost than <see cref="Simplex"/>.</summary>
    SuperSimplex,

    /// <summary>Generates gradient noise on an integer lattice.</summary>
    Perlin,

    /// <summary>Generates interpolated random values on an integer lattice.</summary>
    Value,

    /// <summary>Returns the random value assigned to the selected nearest cellular feature point.</summary>
    CellularValue,

    /// <summary>Returns one cellular distance or a remapped combination of two selected distance ranks.</summary>
    CellularDistance,

    /// <summary>Evaluates a required lookup graph at the nearest cellular feature-point position.</summary>
    CellularLookup,

    /// <summary>Sums unnormalized octaves of a required source using fractional Brownian motion.</summary>
    FractalFbm,

    /// <summary>Folds a scaled required source into a repeating triangular waveform in the range [0, 1].</summary>
    PingPong,

    /// <summary>Sums unnormalized octaves of a required source into sharp ridges and canyons.</summary>
    FractalRidged,

    /// <summary>Warps a required source's domain with simplex vectors.</summary>
    DomainWarpSimplex,

    /// <summary>Warps a required source's domain with smoother, slower SuperSimplex vectors.</summary>
    DomainWarpSuperSimplex,

    /// <summary>Warps a required source's domain with fast grid-gradient vectors.</summary>
    DomainWarpGradient,

    /// <summary>Applies fractal domain-warp octaves progressively, feeding each octave the preceding warped position.</summary>
    DomainWarpFractalProgressive,

    /// <summary>Evaluates all fractal domain-warp octaves from the original position and accumulates their offsets.</summary>
    DomainWarpFractalIndependent,

    /// <summary>Adds a required left source to a constant or node-valued right input.</summary>
    Add,

    /// <summary>Subtracts a constant or node-valued right input from a constant or node-valued left input.</summary>
    Subtract,

    /// <summary>Multiplies a required left source by a constant or node-valued right input.</summary>
    Multiply,

    /// <summary>Divides a constant or node-valued left input by a constant or node-valued right input.</summary>
    Divide,

    /// <summary>Returns the absolute value of a required source.</summary>
    Abs,

    /// <summary>Returns the smaller of a required left source and a constant or node-valued right input.</summary>
    Min,

    /// <summary>Returns the larger of a required left source and a constant or node-valued right input.</summary>
    Max,

    /// <summary>Returns a quadratic smooth minimum of a required left source and a hybrid right input.</summary>
    MinSmooth,

    /// <summary>Returns a quadratic smooth maximum of a required left source and a hybrid right input.</summary>
    MaxSmooth,

    /// <summary>Returns the square root of a source's magnitude while preserving its sign.</summary>
    SignedSquareRoot,

    /// <summary>Raises a clamped absolute hybrid input to a hybrid floating-point power, discarding the input sign.</summary>
    PowFloat,

    /// <summary>Raises a required source to an integer power, avoiding the general floating-point power operation.</summary>
    PowInt,

    /// <summary>Uniformly scales the input coordinates before evaluating a required source.</summary>
    DomainScale,

    /// <summary>Adds an independent constant or node-valued offset to each coordinate before evaluating a required source.</summary>
    DomainOffset,

    /// <summary>Rotates the input domain with yaw, pitch, and roll before evaluating a required source.</summary>
    DomainRotate,

    /// <summary>Independently scales each input coordinate before evaluating a required source.</summary>
    DomainAxisScale,

    /// <summary>Adds an integer offset to the seed used by a required source.</summary>
    SeedOffset,

    /// <summary>Clamps and converts a source to packed grayscale RGBA8 bits stored in a float-sized output slot.</summary>
    ConvertRgba8,

    /// <summary>Caches the last SIMD batch per thread for an exactly repeated position and seed.</summary>
    GeneratorCache,

    /// <summary>Interpolates between required sources A and B using a hybrid fade input.</summary>
    Fade,

    /// <summary>Linearly maps a required source from one numeric interval to another, with optional output clamping.</summary>
    Remap,

    /// <summary>Quantizes a required source into terraces and optionally smooths the transitions.</summary>
    Terrace,

    /// <summary>Appends a configurable coordinate to 2D or 3D input; 4D input passes through unchanged.</summary>
    AddDimension,

    /// <summary>Removes one selected coordinate from 3D or 4D input; 2D input passes through unchanged.</summary>
    RemoveDimension,

    /// <summary>Returns the floating-point remainder of a hybrid left input divided by a hybrid right input.</summary>
    Modulus,

    /// <summary>Applies a preset, plane-oriented domain rotation before evaluating a required source.</summary>
    DomainRotatePlane,
}
