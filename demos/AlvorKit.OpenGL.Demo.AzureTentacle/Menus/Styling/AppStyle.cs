namespace AlvorKit;

/// <summary>App-specific overlay recipes for the azure tentacle demo.</summary>
[App]
public class AppStyle(BlendUi bl)
{
    /// <summary>Gets the OpenGL clear color used behind the animated model.</summary>
    public Vec4 SceneClearColor => bl.S.Palette.AppBackground;

    /// <summary>Applies the base full-screen board treatment used behind app overlays.</summary>
    public void OverlayBoard(EntMut ent) => ent.Mutate()
        .Mutate(bl.S.Board)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a vertical rail surface with a left separator.</summary>
    public void RailSurface(EntMut ent) => ent.Mutate()
        .ColorV(bl.S.Palette.Panel)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0)
        .IsSelectableV(true)
        .Mutate(bl.S.LeftRule);

    /// <summary>Applies a compact floating status strip.</summary>
    public void FloatingStatusStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .ColorV(bl.S.Palette.WithAlpha(bl.S.Palette.Panel, 0.92f))
        .PaddingV((bl.S.Metrics.ButtonTextPadding, 0, bl.S.Metrics.ButtonTextPadding, 0))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .Mutate(bl.S.Border);
}
