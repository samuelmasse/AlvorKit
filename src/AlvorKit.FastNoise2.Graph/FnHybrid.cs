namespace AlvorKit;

/// <summary>Identifies an input that accepts either a float constant or another node.</summary>
/// <remarks>
/// A hybrid configured with a node evaluates that node at every position and the node takes priority over its stored
/// constant. FastNoise2 1.1.1 cannot detach that connection, so a float is active only before any node is connected.
/// Each node type accepts only its documented hybrid inputs.
/// Managed numeric values are wrapper implementation details and are not native hybrid indexes.
/// </remarks>
public enum FnHybrid
{
    /// <summary>Controls the blend between A and B on <see cref="FnNodeType.Fade"/>. The default is 0.</summary>
    Fade,

    /// <summary>Sets the fade value that selects 100 percent B. The default is 1.</summary>
    FadeMaximum,

    /// <summary>Sets the fade value that selects 100 percent A. The default is -1.</summary>
    FadeMinimum,

    /// <summary>Sets the upper input bound of <see cref="FnNodeType.Remap"/>. The default is 1.</summary>
    FromMaximum,

    /// <summary>Sets the lower input bound of <see cref="FnNodeType.Remap"/>. The default is -1.</summary>
    FromMinimum,

    /// <summary>Multiplies amplitude from one fractal octave to the next. The default is 0.5.</summary>
    Gain,

    /// <summary>Controls how far cellular feature points may move within their grid cells. The default is 1.</summary>
    GridJitter,

    /// <summary>Supplies the left operand of an applicable arithmetic node. The default constant is 0.</summary>
    Lhs,

    /// <summary>Sets the exponent p used by the Minkowski distance. The default is 1.5.</summary>
    MinkowskiP,

    /// <summary>Sets the appended coordinate used by <see cref="FnNodeType.AddDimension"/>. The default is 0.</summary>
    NewDimensionPosition,

    /// <summary>Offsets the X coordinate before evaluating an applicable node. The default is 0.</summary>
    OffsetX,

    /// <summary>Offsets the Y coordinate before evaluating an applicable node. The default is 0.</summary>
    OffsetY,

    /// <summary>Offsets the Z coordinate before evaluating an applicable node. The default is 0.</summary>
    OffsetZ,

    /// <summary>Offsets the W coordinate before evaluating an applicable node. The default is 0.</summary>
    OffsetW,

    /// <summary>Controls the reflection frequency of <see cref="FnNodeType.PingPong"/>. The default is 2.</summary>
    PingPongStrength,

    /// <summary>Sets the X coordinate of the target used by <see cref="FnNodeType.DistanceToPoint"/>. The default is 0.</summary>
    PointX,

    /// <summary>Sets the Y coordinate of the target used by <see cref="FnNodeType.DistanceToPoint"/>. The default is 0.</summary>
    PointY,

    /// <summary>Sets the Z coordinate of the target used by <see cref="FnNodeType.DistanceToPoint"/>. The default is 0.</summary>
    PointZ,

    /// <summary>Sets the W coordinate of the target used by <see cref="FnNodeType.DistanceToPoint"/>. The default is 0.</summary>
    PointW,

    /// <summary>Supplies the exponent of <see cref="FnNodeType.PowFloat"/>. The default constant is 2.</summary>
    Power,

    /// <summary>Supplies the right operand of an applicable arithmetic node. The default constant is 0.</summary>
    Rhs,

    /// <summary>Varies the size of cellular feature points. The default is 0.</summary>
    SizeJitter,

    /// <summary>Controls transition smoothing; the default is 0.1 on smooth min/max and 0 on Terrace.</summary>
    Smoothness,

    /// <summary>Sets the upper output bound of <see cref="FnNodeType.Remap"/>. The default is 1.</summary>
    ToMaximum,

    /// <summary>Sets the lower output bound of <see cref="FnNodeType.Remap"/>. The default is 0.</summary>
    ToMinimum,

    /// <summary>Supplies the base of <see cref="FnNodeType.PowFloat"/>. The default constant is 2.</summary>
    Value,

    /// <summary>Controls maximum displacement of an applicable domain warp. The default is 50.</summary>
    WarpAmplitude,

    /// <summary>Feeds octave output back into later fractal amplitude. The default is 0, which disables weighting.</summary>
    WeightedStrength,
}
