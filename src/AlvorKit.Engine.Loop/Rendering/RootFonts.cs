namespace AlvorKit.Engine.Loop;

/// <summary>Root-owned font context and opened font collection.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootFonts : IDisposable
{
    private readonly SpriteBatch batch;
    private readonly FontContext context;
    private readonly List<Font> fonts = [];

    /// <summary>Creates a font context backed by the root OpenGL layer.</summary>
    public RootFonts(RootGl gl)
    {
        batch = new(gl);
        context = new(gl, batch);
    }

    /// <summary>Opens a font and keeps it owned by the root font context.</summary>
    public Font Open(FontOptions options)
    {
        var font = new Font(context, options);
        fonts.Add(font);
        return font;
    }

    /// <summary>Releases all opened fonts and shared font resources.</summary>
    public void Dispose()
    {
        foreach (var font in CollectionsMarshal.AsSpan(fonts))
            font.Dispose();
        context.Dispose();
        batch.Dispose();
    }

    /// <summary>Re-packs pending font atlases after glyph insertion attempts.</summary>
    internal void Pack()
    {
        foreach (var font in CollectionsMarshal.AsSpan(fonts))
            font.Pack();
    }
}
