namespace AlvorKit;

/// <summary>Embedded Inter font faces used by the Blend design system.</summary>
public class BlendFonts(Func<FontOptions, Font> open)
{
    private const string RegularResourceName = "AlvorKit.UI.Blend.res.fonts.Inter.ttf";
    private const string SemiBoldResourceName = "AlvorKit.UI.Blend.res.fonts.Inter-SemiBold.ttf";

    private readonly Font regular = Open(open, RegularResourceName);
    private readonly Font semiBold = Open(open, SemiBoldResourceName);

    /// <summary>Gets the regular Inter face.</summary>
    public Font Regular => regular;

    /// <summary>Gets the semi-bold Inter face used for emphasis text.</summary>
    public Font SemiBold => semiBold;

    private static Font Open(Func<FontOptions, Font> open, string resourceName)
    {
        var assembly = typeof(BlendFonts).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded Inter font resource '{resourceName}' is missing.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return open(new() { Data = memory.ToArray() });
    }
}
