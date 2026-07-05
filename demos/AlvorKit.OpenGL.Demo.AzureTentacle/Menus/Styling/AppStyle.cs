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

    /// <summary>Applies a vertical panel body that fills the available rail space.</summary>
    public void PanelFillList(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0);

    /// <summary>Applies a vertical panel section sized to its children.</summary>
    public void PanelFitList(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(0);

    /// <summary>Applies a raised horizontal header strip.</summary>
    public void HeaderStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .ColorV(Palette.Raised)
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .PaddingV(Metrics.PanelTitlePadding)
        .Mutate(BottomRule);

    /// <summary>Applies an inset vertical list panel with a bottom separator.</summary>
    public void InsetPanelList(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .SizeRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .PaddingV((10f, 10f, 10f, 10f))
        .Mutate(BottomRule);

    /// <summary>Applies a padded vertical list body.</summary>
    public void ListBody(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .PaddingV((Metrics.ButtonTextPadding, Metrics.ButtonTextPadding, Metrics.ButtonTextPadding, Metrics.ButtonTextPadding))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(Metrics.CompactSpacing);

    /// <summary>Applies a fixed-height horizontal row.</summary>
    public void HorizontalRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight);

    /// <summary>Applies a selectable horizontal list row.</summary>
    public void SelectableListRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.ButtonHeight))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerSpacingV(Metrics.LooseSpacing)
        .PaddingV((0, 0, Metrics.ButtonTextPadding, 0))
        .ColorF(() => ent.IsFocusedR || ent.IsHoveredR ? Palette.Hover : default)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    /// <summary>Applies a compact floating status strip.</summary>
    public void FloatingStatusStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .ColorV(Palette.WithAlpha(Palette.Panel, 0.92f))
        .PaddingV((Metrics.ButtonTextPadding, 0, Metrics.ButtonTextPadding, 0))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .Mutate(Border);

    /// <summary>Applies a text label sized from its text.</summary>
    public void Label(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a muted text label sized from its text.</summary>
    public void MutedLabel(EntMut ent) => ent.Mutate()
        .Mutate(MutedText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies an emphasized text label sized from its text.</summary>
    public void EmphasisLabel(EntMut ent) => ent.Mutate()
        .Mutate(EmphasisText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a label that fills its assigned row cell.</summary>
    public void CellLabel(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a muted label that fills its assigned row cell.</summary>
    public void MutedCellLabel(EntMut ent) => ent.Mutate()
        .Mutate(MutedText)
        .SizeRelativeV((1, 1));

    /// <summary>Applies an emphasized label that fills its assigned row cell.</summary>
    public void EmphasisCellLabel(EntMut ent) => ent.Mutate()
        .Mutate(EmphasisText)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a toolbar button that also invokes its click callback when focused and Enter is pressed.</summary>
    public void ToolbarActionButton(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate(ToolbarButton)
            .AlignmentV(Alignment.Vertical)
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                    ent.OnClickFV.Resolve()?.Invoke();

                enterWasDown = enterDown;
            });
    }

    /// <summary>Applies a primary toolbar button that also invokes its click callback when focused and Enter is pressed.</summary>
    public void PrimaryToolbarActionButton(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate(ActiveToolbarButton)
            .AlignmentV(Alignment.Vertical)
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                    ent.OnClickFV.Resolve()?.Invoke();

                enterWasDown = enterDown;
            });
    }

}
