namespace AlvorKit;

/// <summary>Identifies a coordinate axis in four dimensions.</summary>
public enum Axis4 : byte
{
    /// <summary>The x-axis, at component index zero.</summary>
    X = 0,

    /// <summary>The y-axis, at component index one.</summary>
    Y = 1,

    /// <summary>The z-axis, at component index two.</summary>
    Z = 2,

    /// <summary>The w-axis, at component index three.</summary>
    W = 3,

    /// <summary>The number of coordinate axes; this value is not an axis.</summary>
    Count = 4,
}
