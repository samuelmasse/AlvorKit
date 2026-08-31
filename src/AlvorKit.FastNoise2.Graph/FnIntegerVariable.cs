namespace AlvorKit;

/// <summary>Identifies an integer-valued FastNoise2 node variable.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are not native variable indexes.</remarks>
public enum FnIntegerVariable
{
    /// <summary>Adds to the incoming seed before the node evaluates. Most generators default to 0; SeedOffset defaults to 1.</summary>
    SeedOffset,

    /// <summary>Selects the zero-based nearest cellular feature point whose random value is returned. The default is 0.</summary>
    ValueIndex,

    /// <summary>Selects the first zero-based cellular distance rank. The default is 0.</summary>
    DistanceIndex0,

    /// <summary>Selects the second zero-based cellular distance rank. The default is 1.</summary>
    DistanceIndex1,

    /// <summary>Sets the number of fractal layers. The runtime default is 3.</summary>
    Octaves,

    /// <summary>Sets the integer exponent used by <see cref="FnNodeType.PowInt"/>. The default is 2.</summary>
    Power,
}
