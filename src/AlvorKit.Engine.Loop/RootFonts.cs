namespace AlvorKit.Engine.Loop;

[Root]
public class RootFonts(RootGl gl)
{
    private readonly FontContext ctx = new(gl, new(gl));
    private readonly HashSet<Font> fonts = [];

    public Font Open(FontOptions options)
    {
        var font = new Font(ctx, options);
        fonts.Add(font);
        return font;
    }

    internal void Pack()
    {
        foreach (var font in fonts)
            font.Pack();
    }

    internal void Unload()
    {
        foreach (var font in fonts)
            font.Dispose();
        ctx.Dispose();
    }
}
