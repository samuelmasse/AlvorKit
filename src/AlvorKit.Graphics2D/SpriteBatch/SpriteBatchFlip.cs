namespace AlvorKit.Graphics2D;

/// <summary>Texture-coordinate flips that can be applied to a drawn sprite.</summary>
[Flags]
public enum SpriteBatchFlip : byte
{
    /// <summary>Draws texture coordinates without flipping them.</summary>
    None = 0,

    /// <summary>Flips texture coordinates vertically.</summary>
    Vertical = 1,

    /// <summary>Flips texture coordinates horizontally.</summary>
    Horizontal = 2
}
