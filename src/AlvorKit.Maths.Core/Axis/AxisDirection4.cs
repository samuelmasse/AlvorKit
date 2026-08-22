namespace AlvorKit;

/// <summary>Identifies a signed coordinate-axis direction in four dimensions.</summary>
public enum AxisDirection4 : byte
{
    /// <summary>The negative x-axis direction.</summary>
    NegativeX = 0,

    /// <summary>The positive x-axis direction.</summary>
    PositiveX = 1,

    /// <summary>The negative y-axis direction.</summary>
    NegativeY = 2,

    /// <summary>The positive y-axis direction.</summary>
    PositiveY = 3,

    /// <summary>The negative z-axis direction.</summary>
    NegativeZ = 4,

    /// <summary>The positive z-axis direction.</summary>
    PositiveZ = 5,

    /// <summary>The negative w-axis direction.</summary>
    NegativeW = 6,

    /// <summary>The positive w-axis direction.</summary>
    PositiveW = 7,

    /// <summary>The number of signed axis directions; this value is not a direction.</summary>
    Count = 8,
}
