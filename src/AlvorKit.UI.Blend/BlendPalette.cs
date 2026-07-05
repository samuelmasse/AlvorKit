namespace AlvorKit.UI.Blend;

/// <summary>Color tokens for compact Blender-inspired editor UI.</summary>
public readonly record struct BlendPalette(
    Vec4 AppBackground,
    Vec4 Panel,
    Vec4 Raised,
    Vec4 Border,
    Vec4 StrongBorder,
    Vec4 Text,
    Vec4 MutedText,
    Vec4 Accent,
    Vec4 ActiveSurface,
    Vec4 Hover,
    Vec4 Selection,
    Vec4 Scrim)
{
    /// <summary>Gets the default neutral dark palette used by the editor-shell reference.</summary>
    public static BlendPalette Default { get; } = new(
        Rgb(0x18, 0x18, 0x18),
        Rgb(0x20, 0x21, 0x22),
        Rgb(0x28, 0x2A, 0x2B),
        Rgb(0x36, 0x39, 0x3A),
        Rgb(0x4A, 0x4E, 0x50),
        Rgb(0xD7, 0xD7, 0xD2),
        Rgb(0x8D, 0x91, 0x8E),
        Rgb(0xB9, 0x82, 0x45),
        Rgb(0x3A, 0x33, 0x2B),
        Rgb(0x2D, 0x30, 0x31),
        Rgb(0x34, 0x38, 0x3A),
        (0.03f, 0.035f, 0.04f, 0.62f));

    /// <summary>Returns <paramref name="color" /> with its alpha channel replaced.</summary>
    public Vec4 WithAlpha(Vec4 color, float alpha) => (color.X, color.Y, color.Z, alpha);

    private static Vec4 Rgb(byte r, byte g, byte b) => (r / 255f, g / 255f, b / 255f, 1f);
}
