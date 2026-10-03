namespace AlvorKit;

/// <summary>Implements Blend root, panel, list, and row layout recipes.</summary>
internal class BlendStyleLayout
{
    /// <summary>Style façade supplying current palette, metrics, and shared recipes.</summary>
    private readonly BlendStyle s;

    /// <summary>Creates layout recipes over the owning style.</summary>
    internal BlendStyleLayout(BlendStyle s) =>
        this.s = s;

    /// <summary>Applies the full-window vertical root layout with editor-shell click-away semantics.</summary>
    internal void Root(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0)
        .InnerAlignmentSnapV(1f)
        .ColorV(s.Palette.AppBackground)
        .IsSelectableV(true)
        .IsSilentFocusableV(true);

    /// <summary>Applies an explicit-position board layout.</summary>
    internal void Board(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.Board)
        .InnerAlignmentSnapV(1f);

    /// <summary>Applies the top application menu bar surface.</summary>
    internal void MenuBar(EntMut ent) =>
        Strip(ent, s.Metrics.MenuBarHeight, s.Palette.Panel);

    /// <summary>Applies the main tool strip surface.</summary>
    internal void Toolbar(EntMut ent) =>
        Strip(ent, s.Metrics.ToolbarHeight, s.Palette.Raised);

    /// <summary>Applies the bottom status strip surface.</summary>
    internal void StatusBar(EntMut ent) => ent.Mutate()
        .Mutate(s.Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, s.Metrics.StatusBarHeight))
        .ColorV(s.Palette.Panel)
        .Mutate(s.TopRule);

    /// <summary>Applies a plain panel fill.</summary>
    internal void Panel(EntMut ent) => ent.Mutate()
        .ColorV(s.Palette.Panel);

    /// <summary>Applies a raised panel title strip.</summary>
    internal void PanelTitle(EntMut ent) => ent.Mutate()
        .Mutate(s.Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, s.Metrics.PanelTitleHeight))
        .ColorV(s.Palette.Raised)
        .Mutate(s.BottomRule);

    /// <summary>Applies a vertical panel body that fills the available space.</summary>
    internal void PanelFillList(EntMut ent) => ent.Mutate()
        .ColorV(s.Palette.Panel)
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0);

    /// <summary>Applies a vertical panel section sized to its children.</summary>
    internal void PanelFitList(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(0);

    /// <summary>Applies a raised horizontal header strip.</summary>
    internal void HeaderStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .ColorV(s.Palette.Raised)
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .PaddingV(s.Metrics.PanelTitlePadding)
        .Mutate(s.BottomRule);

    /// <summary>Applies an inset vertical list panel with a bottom separator.</summary>
    internal void InsetPanelList(EntMut ent) => ent.Mutate()
        .ColorV(s.Palette.Panel)
        .SizeRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .PaddingV(s.Metrics.InsetPanelPadding)
        .Mutate(s.BottomRule);

    /// <summary>Applies a padded vertical list body.</summary>
    internal void ListBody(EntMut ent) => ent.Mutate()
        .ColorV(s.Palette.Panel)
        .PaddingV((
            s.Metrics.ButtonTextPadding,
            s.Metrics.ButtonTextPadding,
            s.Metrics.ButtonTextPadding,
            s.Metrics.ButtonTextPadding))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(s.Metrics.CompactSpacing);

    /// <summary>Applies a vertical list sized from its children.</summary>
    internal void VerticalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((0, 1))
        .SizeInnerMaxRelativeV((1, 0));

    /// <summary>Applies a horizontal list sized from its children.</summary>
    internal void HorizontalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .SizeInnerMaxRelativeV((0, 1));

    /// <summary>Applies a full-size weighted horizontal list.</summary>
    internal void HorizontalFill(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a fixed-height horizontal row.</summary>
    internal void HorizontalRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight);

    /// <summary>Applies a selectable horizontal list row.</summary>
    internal void SelectableListRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .SizeV((0, s.Metrics.ButtonHeight))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerSpacingV(s.Metrics.LooseSpacing)
        .PaddingV((0, 0, s.Metrics.ButtonTextPadding, 0))
        .ColorF(() => ent.IsHoveredR ? s.Palette.Hover : default)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    /// <summary>Applies a fixed-height label/value metric row.</summary>
    internal void MetricRow(EntMut ent) => ent.Mutate()
        .Mutate(s.Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, s.Metrics.MetricRowHeight));

    /// <summary>Applies a fixed-height strip with a bottom separator.</summary>
    private void Strip(EntMut ent, float height, Vec4 color) => ent.Mutate()
        .Mutate(s.Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, height))
        .ColorV(color)
        .Mutate(s.BottomRule);
}
