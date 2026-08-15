namespace AlvorKit;

/// <summary>Implements Blend text, label, tab, and tooltip text recipes.</summary>
internal sealed class BlendStyleTypography
{
    /// <summary>Style façade supplying current palette, metrics, and shared recipes.</summary>
    private readonly BlendStyle style;

    /// <summary>Regular Inter face used for body text.</summary>
    private readonly Font font;

    /// <summary>Semibold Inter face used for emphasized text.</summary>
    private readonly Font emphasisFont;

    /// <summary>Creates typography recipes over the owning style and its loaded faces.</summary>
    internal BlendStyleTypography(BlendStyle style, Font font, Font emphasisFont)
    {
        this.style = style;
        this.font = font;
        this.emphasisFont = emphasisFont;
    }

    /// <summary>Applies body text matching the editor-shell reference.</summary>
    internal void Text(EntMut ent) => ent.Mutate()
        .FontV(font)
        .FontSizeV(style.Metrics.TextFontSize)
        .TextColorV(style.Palette.Text)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextAlignmentSnapV(1f)
        .TextGlyphAlignmentSnapV(0f);

    /// <summary>Applies a semibold text treatment using the loaded font face.</summary>
    internal void EmphasisText(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .FontV(emphasisFont);

    /// <summary>Applies smaller muted metadata text.</summary>
    internal void MutedText(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .FontSizeV(style.Metrics.MutedFontSize)
        .TextColorV(style.Palette.MutedText);

    /// <summary>Applies centered text for compact controls.</summary>
    internal void CenterText(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .TextAlignmentV(Alignment.Center)
        .TextPaddingV((style.Metrics.CenterTextPadding, 0, style.Metrics.CenterTextPadding, 0));

    /// <summary>Applies a text label sized from its text.</summary>
    internal void Label(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a muted text label sized from its text.</summary>
    internal void MutedLabel(EntMut ent) => ent.Mutate()
        .Mutate(style.MutedText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies an emphasized text label sized from its text.</summary>
    internal void EmphasisLabel(EntMut ent) => ent.Mutate()
        .Mutate(style.EmphasisText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a label that fills its assigned row cell.</summary>
    internal void CellLabel(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a muted label that fills its assigned row cell.</summary>
    internal void MutedCellLabel(EntMut ent) => ent.Mutate()
        .Mutate(style.MutedText)
        .SizeRelativeV((1, 1));

    /// <summary>Applies an emphasized label that fills its assigned row cell.</summary>
    internal void EmphasisCellLabel(EntMut ent) => ent.Mutate()
        .Mutate(style.EmphasisText)
        .SizeRelativeV((1, 1));

    /// <summary>Applies a menu item hit target with transparent idle fill.</summary>
    internal void MenuItem(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .TextPaddingV((style.Metrics.MenuItemTextPadding, 0, style.Metrics.MenuItemTextPadding, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .ColorF(() => ent.IsHoveredR ? style.Palette.Hover : default)
        .Mutate(style.ActivateOnEnter);

    /// <summary>Applies a bottom-dock tab surface sized from its text.</summary>
    internal void Tab(EntMut ent)
    {
        Text(ent);
        ent.Mutate()
            .SizeRelativeV((0, 0))
            .SizeTextRelativeV((1, 0))
            .SizeV((0, style.Metrics.TabStripHeight))
            .TextPaddingV((style.Metrics.TabTextPaddingLeft, 0, style.Metrics.TabTextPaddingRight, 0))
            .ColorV(style.Palette.Raised)
            .TextColorV(style.Palette.MutedText);
        style.RightRule(ent);
        style.BottomRule(ent);
    }

    /// <summary>Applies an active bottom-dock tab surface sized from its text.</summary>
    internal void ActiveTab(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, style.Metrics.TabStripHeight))
        .TextPaddingV((style.Metrics.TabTextPaddingLeft, 0, style.Metrics.TabTextPaddingRight, 0))
        .ColorV(style.Palette.Panel)
        .TextColorV(style.Palette.Text)
        .Mutate(style.RightRule);

    /// <summary>Applies the emphasized first line of a tooltip.</summary>
    internal void TooltipTitle(EntMut ent) => ent.Mutate()
        .Mutate(style.EmphasisText)
        .FontSizeV(style.Metrics.MutedFontSize)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a muted tooltip detail line.</summary>
    internal void TooltipLine(EntMut ent) => ent.Mutate()
        .Mutate(style.MutedText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));
}
