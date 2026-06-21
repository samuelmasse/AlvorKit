namespace AlvorKit.Graphics2D.Fonts;

internal sealed partial class FontAtlas
{
    /// <summary>Finds the next skyline slot for a glyph without mutating the atlas.</summary>
    private (int X, int Y) NextSlot(FontGlyph glyph)
    {
        var width = checked((int)glyph.Box.X);
        var x = cursor;

        if (cursor + width > tablet.Size)
            x = 0;

        var y = 0;
        for (var dx = 0; dx < width; dx++)
            y = Math.Max(y, used[x + dx]);

        return (x, y);
    }

    /// <summary>Marks the atlas skyline region consumed by a glyph.</summary>
    private void Advance(FontGlyph glyph, int x, int y)
    {
        var width = checked((int)glyph.Box.X);
        for (var dx = 0; dx < width; dx++)
            used[x + dx] = y + checked((int)glyph.Box.Y) + 1;

        cursor = x + width + 1;
    }

    /// <summary>Uploads a glyph bitmap into its final atlas position.</summary>
    private void Upload(FontGlyph glyph, ReadOnlySpan<(byte Red, byte Green, byte Blue, byte Alpha)> pixels, int x, int y)
    {
        var width = checked((int)glyph.Box.X);
        var height = checked((int)glyph.Box.Y);
        if (width == 0 || height == 0)
            return;

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, tablet.Texture.Id);
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 1);
        gl.TexSubImage2D(
            GlTextureTarget.Texture2D,
            0,
            x,
            tablet.Size - y - height,
            width,
            height,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixels);
        gl.ResetPixelStore(GlPixelStoreParameter.UnpackAlignment);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
    }
}
