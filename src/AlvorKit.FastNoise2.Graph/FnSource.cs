namespace AlvorKit;

/// <summary>Identifies a FastNoise2 input that requires another node.</summary>
/// <remarks>Managed numeric values are wrapper implementation details and are not native source indexes.</remarks>
public enum FnSource
{
    /// <summary>Supplies the first endpoint of <see cref="FnNodeType.Fade"/>.</summary>
    A,

    /// <summary>Supplies the second endpoint of <see cref="FnNodeType.Fade"/>.</summary>
    B,

    /// <summary>Supplies the domain-warp generator used by a fractal domain-warp node.</summary>
    DomainWarpSource,

    /// <summary>Supplies the required left operand of an applicable arithmetic node.</summary>
    Lhs,

    /// <summary>Supplies the graph evaluated at cellular feature-point positions by <see cref="FnNodeType.CellularLookup"/>.</summary>
    Lookup,

    /// <summary>Supplies the primary input graph of an applicable node.</summary>
    Source,

    /// <summary>Supplies the base value raised by <see cref="FnNodeType.PowInt"/>.</summary>
    Value,
}
