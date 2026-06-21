namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Identifies where a glyph bitmap lives inside an atlas texture.</summary>
/// <param name="glyph">The glyph metrics for this slot.</param>
/// <param name="texture">The atlas texture that stores the bitmap.</param>
/// <param name="position">The top-left atlas pixel coordinate of the glyph bitmap.</param>
public class FontGlyphSlot(FontGlyph glyph, Texture texture, Vec2u position)
{
    /// <summary>Gets the glyph metrics for this slot.</summary>
    public FontGlyph Glyph => glyph;

    /// <summary>Gets the atlas texture that stores the bitmap.</summary>
    public Texture Texture { get; internal set; } = texture;

    /// <summary>Gets the top-left atlas pixel coordinate of the glyph bitmap.</summary>
    public Vec2u Position { get; internal set; } = position;
}
