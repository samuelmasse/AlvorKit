namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Placement and advance metrics for one rendered glyph.</summary>
/// <param name="Char">The Unicode scalar represented by the glyph.</param>
/// <param name="Size">The requested font pixel size.</param>
/// <param name="Box">The bitmap size in pixels.</param>
/// <param name="Bearing">The glyph bearing relative to the pen baseline.</param>
/// <param name="Advance">The horizontal pen advance in pixels.</param>
public sealed record FontGlyph(Rune Char, int Size, Vec2u Box, Vec2i Bearing, float Advance);
