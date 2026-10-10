namespace AlvorKit;

/// <summary>Packs glyph atlases across active font collections; each collection retains ownership of its fonts.</summary>
[Root]
public class RootFontPacking
{
    private readonly HashSet<Font> fonts = [];

    internal void Register(Font font) => fonts.Add(font);

    internal void Unregister(Font font) => fonts.Remove(font);

    /// <summary>Repacks atlases that filled during glyph insertion, before the next drawing pass.</summary>
    public void Pack()
    {
        foreach (var font in fonts)
            font.Pack();
    }
}
