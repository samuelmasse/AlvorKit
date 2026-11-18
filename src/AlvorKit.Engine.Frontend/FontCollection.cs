namespace AlvorKit;

/// <summary>Owns a collection's native fonts and registers them for rendering-time atlas packing.</summary>
/// <remarks>The supplied GL layer owns GPU resources. Unload the fonts before disposing that layer.</remarks>
public class FontCollection(RootFontPacking packing, Ft ft, GlLayer gl)
{
    private readonly List<Font> fonts = [];
    private FontContext? context;

    /// <summary>Opens a font and creates shared native and GPU staging resources on first use.</summary>
    public Font Open(FontOptions options)
    {
        context ??= new(gl, ft, new(gl));
        var font = new Font(context, options);
        fonts.Add(font);
        packing.Register(font);
        return font;
    }

    /// <summary>Unregisters and releases this collection's fonts and native context; leaves GPU cleanup to its GL layer.</summary>
    public void Unload()
    {
        foreach (var font in fonts)
        {
            packing.Unregister(font);
            font.Dispose();
        }

        fonts.Clear();
        context?.Dispose();
        context = null;
    }
}
