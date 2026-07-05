namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Blend-backed UI style for the Noise Lab; ramps and probe colors stay in <see cref="AppRamps"/>.</summary>
[App]
public class AppStyle(
    RootFonts fonts,
    RootGl gl,
    RootUiScale scale,
    RootKeyboard keyboard) : BlendStyle(new()
    {
        Font = fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(AppStyle)), "fonts", "Inter.ttf") }),
        EmphasisFont = fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(AppStyle)), "fonts", "Inter-SemiBold.ttf") }),
        Chrome = new BlendControlChrome(gl, scale),
        Keyboard = keyboard,
    })
{
    /// <summary>Chips carry decimal readouts; the 11px period glyph rasterizes to nothing, so chips use 12px text.</summary>
    public override BlendMetrics Metrics { get; } = new() { ChipFontSize = 12 };
}
