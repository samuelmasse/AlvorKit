namespace AlvorKit.Graphics2D.Fonts;

public static partial class FontSpriteBatchExtensions
{
    /// <summary>Draws a string in white at unscaled size.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, string text, Vector2 position) =>
        Write(sprites, fontSize, (ReadOnlySpan<char>)text, position, Vector4.One, 1f, 0f);

    /// <summary>Draws a tinted string at unscaled size.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, string text, Vector2 position, Vector4 color) =>
        Write(sprites, fontSize, (ReadOnlySpan<char>)text, position, color, 1f, 0f);

    /// <summary>Draws a tinted string with a supplied scale.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, string text, Vector2 position, Vector4 color, float scale) =>
        Write(sprites, fontSize, (ReadOnlySpan<char>)text, position, color, scale, 0f);

    /// <summary>Draws a tinted string with supplied scale and snapping.</summary>
    public static void Write(
        this SpriteBatchWriter sprites,
        FontSize fontSize,
        string text,
        Vector2 position,
        Vector4 color,
        float scale,
        float snap) =>
        Write(sprites, fontSize, (ReadOnlySpan<char>)text, position, color, scale, snap);

    /// <summary>Draws a character span in white at unscaled size.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, ReadOnlySpan<char> text, Vector2 position) =>
        Write(sprites, fontSize, text, position, Vector4.One, 1f, 0f);

    /// <summary>Draws a tinted character span at unscaled size.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, ReadOnlySpan<char> text, Vector2 position, Vector4 color) =>
        Write(sprites, fontSize, text, position, color, 1f, 0f);

    /// <summary>Draws a tinted character span with a supplied scale.</summary>
    public static void Write(this SpriteBatchWriter sprites, FontSize fontSize, ReadOnlySpan<char> text, Vector2 position, Vector4 color, float scale) =>
        Write(sprites, fontSize, text, position, color, scale, 0f);

    /// <summary>Draws glyphs into a sprite batch using cached atlas slots.</summary>
    public static void Write(
        this SpriteBatchWriter sprites,
        FontSize fontSize,
        ReadOnlySpan<char> text,
        Vector2 position,
        Vector4 color,
        float scale,
        float snap)
    {
        var baselineOffset = (fontSize.Metrics.Ascender + fontSize.Metrics.Descender) * 0.5f;
        var pen = 0f;
        var first = true;

        foreach (var character in text.EnumerateRunes())
        {
            var slot = fontSize.GlyphSlot(character);
            var relativeX = pen + (first ? 0f : slot.Glyph.Bearing.X);
            var relativeY = baselineOffset - slot.Glyph.Bearing.Y;
            var x = position.X + Snap(relativeX, snap);
            var y = position.Y + Snap(relativeY, snap);

            sprites.Draw(slot.Texture, new Vector2(x, y) / scale, slot.Glyph.Box / scale, slot.Position, slot.Glyph.Box, color);
            pen += slot.Glyph.Advance;
            first = false;
        }
    }
}
