namespace AlvorKit;

/// <summary>Loads and caches resource fonts and owns fonts opened from embedded data.</summary>
[Root]
public class RootFonts
{
    private readonly FontCollection fonts;
    private readonly RootFiles files;
    private readonly Dictionary<string, Font> loaded = [];

    public Font this[string path]
    {
        get
        {
            if (loaded.TryGetValue(path, out var font))
                return font;

            font = Open(new() { File = path });
            loaded.Add(path, font);
            return font;
        }
    }

    public RootFonts(RootFontPacking packing, Ft ft, RootFiles files, RootGl gl) : this(packing, ft, files, (GlLayer)gl) { }

    protected RootFonts(RootFontPacking packing, Ft ft, RootFiles files, GlLayer gl)
    {
        fonts = new(packing, ft, gl);
        this.files = files;
    }

    /// <summary>Opens resource paths through this owner's files, or uses supplied font bytes directly.</summary>
    public Font Open(FontOptions options)
    {
        if (options.File is { } path)
            options = new() { Data = files.ReadAllBytes(path), Index = options.Index };

        return fonts.Open(options);
    }

    public void Unload()
    {
        fonts.Unload();
        loaded.Clear();
    }
}
