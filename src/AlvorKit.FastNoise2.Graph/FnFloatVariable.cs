namespace AlvorKit;

/// <summary>Identifies a scalar float variable, including an explicitly qualified vector component.</summary>
/// <remarks>
/// Variable names are shared across node types, but each node accepts only its documented subset. Supplying an
/// unsupported variable causes <see cref="FnGraphNode.Float"/> to throw instead of silently configuring another field.
/// Managed numeric values are wrapper implementation details and must not be passed to the raw binding or persisted.
/// </remarks>
public enum FnFloatVariable
{
    /// <summary>Scales the domain-warp displacement on the X axis. The runtime default is 1.</summary>
    AmplitudeScalingX,

    /// <summary>Scales the domain-warp displacement on the Y axis. The runtime default is 1.</summary>
    AmplitudeScalingY,

    /// <summary>Scales the domain-warp displacement on the Z axis. The runtime default is 1.</summary>
    AmplitudeScalingZ,

    /// <summary>Scales the domain-warp displacement on the W axis. The runtime default is 1.</summary>
    AmplitudeScalingW,

    /// <summary>Sets the characteristic feature size in domain units; it is the inverse of frequency. The default is 100.</summary>
    FeatureScale,

    /// <summary>Multiplies frequency from one fractal octave to the next. The runtime default is 2.</summary>
    Lacunarity,

    /// <summary>Sets the upper input bound used by <see cref="FnNodeType.ConvertRgba8"/>. The default is 1.</summary>
    Maximum,

    /// <summary>Sets the lower input bound used by <see cref="FnNodeType.ConvertRgba8"/>. The default is -1.</summary>
    Minimum,

    /// <summary>Sets the X coefficient used by <see cref="FnNodeType.Gradient"/>. The default is 0.</summary>
    MultiplierX,

    /// <summary>Sets the Y coefficient used by <see cref="FnNodeType.Gradient"/>. The default is 0.</summary>
    MultiplierY,

    /// <summary>Sets the Z coefficient used by <see cref="FnNodeType.Gradient"/>. The default is 0.</summary>
    MultiplierZ,

    /// <summary>Sets the W coefficient used by <see cref="FnNodeType.Gradient"/>. The default is 0.</summary>
    MultiplierW,

    /// <summary>Sets the upper remapped output bound of applicable coherent-noise nodes. The default is 1.</summary>
    OutputMaximum,

    /// <summary>Sets the lower remapped output bound of applicable coherent-noise nodes. The default is -1.</summary>
    OutputMinimum,

    /// <summary>Sets the Y-axis domain-rotation angle in radians. The default is 0.</summary>
    Pitch,

    /// <summary>Sets the X-axis domain-rotation angle in radians. The default is 0.</summary>
    Roll,

    /// <summary>Sets the uniform coordinate multiplier used by <see cref="FnNodeType.DomainScale"/>. The default is 1.</summary>
    Scaling,

    /// <summary>Sets the X-coordinate multiplier used by <see cref="FnNodeType.DomainAxisScale"/>. The default is 1.</summary>
    ScalingX,

    /// <summary>Sets the Y-coordinate multiplier used by <see cref="FnNodeType.DomainAxisScale"/>. The default is 1.</summary>
    ScalingY,

    /// <summary>Sets the Z-coordinate multiplier used by <see cref="FnNodeType.DomainAxisScale"/>. The default is 1.</summary>
    ScalingZ,

    /// <summary>Sets the W-coordinate multiplier used by <see cref="FnNodeType.DomainAxisScale"/>. The default is 1.</summary>
    ScalingW,

    /// <summary>Controls the number of quantization steps used by <see cref="FnNodeType.Terrace"/>. The default is 1.</summary>
    StepCount,

    /// <summary>Sets the output returned by <see cref="FnNodeType.Constant"/>. The default is 1.</summary>
    Value,

    /// <summary>Sets the Z-axis domain-rotation angle in radians and controls 2D rotation. The default is 0.</summary>
    Yaw,
}
