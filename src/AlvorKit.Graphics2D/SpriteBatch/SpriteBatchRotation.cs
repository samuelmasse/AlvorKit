namespace AlvorKit.Graphics2D;

/// <summary>Right-angle texture-coordinate rotations that can be applied to a drawn sprite.</summary>
public enum SpriteBatchRotation : byte
{
    /// <summary>Draws texture coordinates without rotation.</summary>
    None,

    /// <summary>Rotates texture coordinates by 90 degrees clockwise.</summary>
    Clockwise90,

    /// <summary>Rotates texture coordinates by 180 degrees clockwise.</summary>
    Clockwise180,

    /// <summary>Rotates texture coordinates by 270 degrees clockwise.</summary>
    Clockwise270
}
