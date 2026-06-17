namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Text measuring and drawing helpers for <see cref="SpriteBatchWriter"/>.</summary>
public static partial class FontSpriteBatchExtensions
{
    /// <summary>Measures the width of a string without position snapping.</summary>
    public static float Measure(this SpriteBatchWriter sprites, FontSize fontSize, string text) =>
        Measure(sprites, fontSize, (ReadOnlySpan<char>)text, 0f);

    /// <summary>Measures the width of a string with optional position snapping.</summary>
    public static float Measure(this SpriteBatchWriter sprites, FontSize fontSize, string text, float snap) =>
        Measure(sprites, fontSize, (ReadOnlySpan<char>)text, snap);

    /// <summary>Measures the width of a character span without position snapping.</summary>
    public static float Measure(this SpriteBatchWriter sprites, FontSize fontSize, ReadOnlySpan<char> text) =>
        Measure(sprites, fontSize, text, 0f);

    /// <summary>Measures the width of a character span with optional position snapping.</summary>
    public static float Measure(this SpriteBatchWriter sprites, FontSize fontSize, ReadOnlySpan<char> text, float snap)
    {
        _ = sprites;
        var pen = 0f;
        var lastX = 0f;
        var lastSize = 0f;
        var first = true;

        foreach (var character in text.EnumerateRunes())
        {
            var slot = fontSize.GlyphSlot(character);
            var relativeX = pen + (first ? 0f : slot.Glyph.Bearing.X);
            lastX = Snap(relativeX, snap);
            lastSize = slot.Glyph.Box.X;
            pen += slot.Glyph.Advance;
            first = false;
        }

        return lastX + lastSize;
    }

    /// <summary>Snaps a value to the requested pixel interval.</summary>
    private static float Snap(float value, float snap) => snap > 0f ? MathF.Round(value / snap) * snap : value;
}
