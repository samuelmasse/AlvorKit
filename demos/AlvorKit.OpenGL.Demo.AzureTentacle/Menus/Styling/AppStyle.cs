namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Blend-backed UI style for the azure tentacle demo.</summary>
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
    /// <summary>Gets the OpenGL clear color used behind the animated model.</summary>
    public Vec4 SceneClearColor => Palette.AppBackground;

    /// <summary>Applies the base full-screen board treatment used behind app overlays.</summary>
    public void OverlayBoard(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a vertical rail surface with a left separator.</summary>
    public void RailSurface(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0)
        .IsSelectableV(true)
        .Mutate(LeftRule);

    /// <summary>Applies a compact floating status strip.</summary>
    public void FloatingStatusStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .ColorV(Palette.WithAlpha(Palette.Panel, 0.92f))
        .PaddingV((Metrics.ButtonTextPadding, 0, Metrics.ButtonTextPadding, 0))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .Mutate(Border);

}
