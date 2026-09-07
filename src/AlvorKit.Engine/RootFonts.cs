namespace AlvorKit;

[Root]
public class RootFonts(RootGl gl, Ft ft)
{
    private readonly FontContext ctx = new(gl, ft, new(gl));
    private readonly HashSet<Font> fonts = [];
    private readonly HashSet<Font> owned = [];

    public Font Open(FontOptions options)
    {
        var font = new Font(ctx, options);
        owned.Add(font);
        fonts.Add(font);
        return font;
    }

    /// <summary>Registers a caller-owned font for packing.</summary>
    public void Add(Font font) => fonts.Add(font);

    /// <summary>Stops packing a font without disposing it.</summary>
    public void Remove(Font font) => fonts.Remove(font);

    internal void Pack()
    {
        foreach (var font in fonts)
            font.Pack();
    }

    internal void Unload()
    {
        foreach (var font in owned)
            font.Dispose();

        owned.Clear();
        fonts.Clear();
        ctx.Dispose();
    }
}
