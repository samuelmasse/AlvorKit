namespace AlvorKit.UI.Blend;

/// <summary>Implements Blend tab, dock, rule, modal, tooltip, and swatch surfaces.</summary>
internal sealed class BlendStyleSurfaces
{
    /// <summary>Style façade supplying current palette, metrics, and shared recipes.</summary>
    private readonly BlendStyle style;

    /// <summary>Creates surface recipes over the owning style.</summary>
    internal BlendStyleSurfaces(BlendStyle style) =>
        this.style = style;

    /// <summary>Adds the accent bar that marks an active tab, sparing the tab's right separator.</summary>
    internal void ActiveTabAccent(EntMut ent) =>
        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .OffsetV((0, style.Metrics.ActiveTabAccentOffset))
            .SizeRelativeV((1, 0))
            .SizeV((-style.Metrics.Hairline, style.Metrics.ActiveTabAccentHeight))
            .ColorV(style.Palette.Accent);

    /// <summary>Applies a raised tab strip surface.</summary>
    internal void TabStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, style.Metrics.TabStripHeight))
        .ColorV(style.Palette.Raised)
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerSpacingV(0);

    /// <summary>Fills the tab strip after the last tab and carries its bottom rule.</summary>
    internal void TabFiller(EntMut ent) => ent.Mutate()
        .ColorV(default)
        .Mutate(style.BottomRule);

    /// <summary>Applies a vertical dock panel surface with a bottom separator.</summary>
    internal void Dock(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 1))
        .ColorV(style.Palette.Panel)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .Mutate(style.BottomRule);

    /// <summary>Applies a thin vertical splitter between docks.</summary>
    internal void Splitter(EntMut ent)
    {
        ent.Mutate()
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((0, 1))
            .ColorV(style.Palette.AppBackground);
        LeftRule(ent);
        RightRule(ent);
    }

    /// <summary>Adds a one-pixel border around a node.</summary>
    internal void Border(EntMut ent)
    {
        TopRule(ent);
        BottomRule(ent);
        LeftRule(ent);
        RightRule(ent);
    }

    /// <summary>Adds a one-pixel strong border around a node.</summary>
    internal void StrongBorder(EntMut ent)
    {
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, style.Metrics.Hairline), style.Palette.StrongBorder);
        BlendStyle.Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, style.Metrics.Hairline), style.Palette.StrongBorder);
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (style.Metrics.Hairline, 0), style.Palette.StrongBorder);
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (style.Metrics.Hairline, 0), style.Palette.StrongBorder);
    }

    /// <summary>Adds a top hairline rule.</summary>
    internal void TopRule(EntMut ent) =>
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, style.Metrics.Hairline), style.Palette.Border);

    /// <summary>Adds a bottom hairline rule.</summary>
    internal void BottomRule(EntMut ent) =>
        BlendStyle.Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, style.Metrics.Hairline), style.Palette.Border);

    /// <summary>Adds a left hairline rule.</summary>
    internal void LeftRule(EntMut ent) =>
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (style.Metrics.Hairline, 0), style.Palette.Border);

    /// <summary>Adds a right hairline rule.</summary>
    internal void RightRule(EntMut ent) =>
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (style.Metrics.Hairline, 0), style.Palette.Border);

    /// <summary>Applies the full-screen tinted layer behind a modal dialog.</summary>
    internal void ModalLayer(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerAlignmentSnapV(1f)
        .ColorV(style.Palette.Scrim)
        .IsSelectableV(true)
        .IsSilentFocusableV(true);

    /// <summary>Applies a centered modal dialog panel.</summary>
    internal void ModalPanel(EntMut ent) => ent.Mutate()
        .ColorV(style.Palette.Panel)
        .SizeRelativeV((0, 0))
        .AlignmentV(Alignment.Horizontal | Alignment.Vertical)
        .AlignmentSnapV(1f)
        .InnerAlignmentSnapV(1f)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .IsSelectableV(true)
        .IsSilentFocusableV(true)
        .Mutate(style.StrongBorder);

    /// <summary>Applies a modal dialog's padded content area.</summary>
    internal void ModalContent(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .PaddingV(style.Metrics.ModalContentPadding)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerAlignmentSnapV(1f);

    /// <summary>Applies a floating tooltip surface that sizes to its line children.</summary>
    internal void Tooltip(EntMut ent) => ent.Mutate()
        .Mutate(style.VerticalList)
        .InnerSpacingV(style.Metrics.TooltipLineSpacing)
        .PaddingV(style.Metrics.TooltipPadding)
        .ColorV(style.Palette.Raised)
        .Mutate(style.StrongBorder);

    /// <summary>Applies a small legend swatch; set the color at the call site.</summary>
    internal void Swatch(EntMut ent) => ent.Mutate()
        .SizeRelativeV((0, 0))
        .SizeV((style.Metrics.SwatchWidth, style.Metrics.SwatchHeight))
        .AlignmentV(Alignment.Vertical);
}
