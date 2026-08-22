namespace AlvorKit;

/// <summary>Identifies a signed coordinate-axis direction in two dimensions.</summary>
public enum AxisDirection2 : byte
{
    /// <summary>The negative x-axis direction.</summary>
    NegativeX = 0,

    /// <summary>The positive x-axis direction.</summary>
    PositiveX = 1,

    /// <summary>The negative y-axis direction.</summary>
    NegativeY = 2,

    /// <summary>The positive y-axis direction.</summary>
    PositiveY = 3,

    /// <summary>The number of signed axis directions; this value is not a direction.</summary>
    Count = 4,
}
