namespace AlvorKit.UI.Blend;

/// <summary>Root-owned embedded Inter font faces used by the Blend design system.</summary>
[Root]
[ExcludeFromCodeCoverage]
public class RootInter(RootFonts fonts)
{
    private const string RegularResourceName = "AlvorKit.UI.Blend.res.fonts.Inter.ttf";
    private const string SemiBoldResourceName = "AlvorKit.UI.Blend.res.fonts.Inter-SemiBold.ttf";

    /// <summary>Gets the regular Inter face.</summary>
    public Font Regular { get; } = Open(fonts, RegularResourceName);

    /// <summary>Gets the semi-bold Inter face used for emphasis text.</summary>
    public Font SemiBold { get; } = Open(fonts, SemiBoldResourceName);

    private static Font Open(RootFonts fonts, string resourceName)
    {
        var assembly = typeof(RootInter).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded Inter font resource '{resourceName}' is missing.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return fonts.Open(new() { Data = memory.ToArray() });
    }
}
