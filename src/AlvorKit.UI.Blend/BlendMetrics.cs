namespace AlvorKit.UI.Blend;

/// <summary>Reusable pixel metrics for Blender-inspired UI surfaces and controls.</summary>
public sealed record BlendMetrics
{
    /// <summary>Gets the top application menu bar height.</summary>
    public float MenuBarHeight { get; init; } = 25f;

    /// <summary>Gets the main tool strip height.</summary>
    public float ToolbarHeight { get; init; } = 35f;

    /// <summary>Gets the bottom status strip height.</summary>
    public float StatusBarHeight { get; init; } = 22f;

    /// <summary>Gets the raised panel title strip height.</summary>
    public float PanelTitleHeight { get; init; } = 28f;

    /// <summary>Gets the viewport header strip height.</summary>
    public float ViewportHeaderHeight { get; init; } = 29f;

    /// <summary>Gets the bottom-dock tab strip height.</summary>
    public float TabStripHeight { get; init; } = 29f;

    /// <summary>Gets the asset browser toolbar height.</summary>
    public float AssetToolbarHeight { get; init; } = 31f;

    /// <summary>Gets the static field surface height.</summary>
    public float FieldHeight { get; init; } = 24f;

    /// <summary>Gets the compact rounded button height.</summary>
    public float ButtonHeight { get; init; } = 26f;

    /// <summary>Gets the toolbar-sized rounded button height.</summary>
    public float ToolbarButtonHeight { get; init; } = 24f;

    /// <summary>Gets the toolbar chip height.</summary>
    public float ChipHeight { get; init; } = 23f;

    /// <summary>Gets the square button edge length.</summary>
    public float SquareButtonSize { get; init; } = 26f;

    /// <summary>Gets the rounded control corner radius.</summary>
    public float ControlRadius { get; init; } = 2f;

    /// <summary>Gets the rounded control border thickness.</summary>
    public float ControlBorderWidth { get; init; } = 1f;

    /// <summary>Gets the hairline rule thickness.</summary>
    public float Hairline { get; init; } = 1f;

    /// <summary>Gets the body text font size.</summary>
    public int TextFontSize { get; init; } = 12;

    /// <summary>Gets the muted metadata font size.</summary>
    public int MutedFontSize { get; init; } = 11;

    /// <summary>Gets the button label font size.</summary>
    public int ButtonFontSize { get; init; } = 12;

    /// <summary>Gets the chip label font size.</summary>
    public int ChipFontSize { get; init; } = 11;

    /// <summary>Gets the square button label font size.</summary>
    public int SquareButtonFontSize { get; init; } = 10;

    /// <summary>Gets the horizontal menu item text padding.</summary>
    public float MenuItemTextPadding { get; init; } = 9f;

    /// <summary>Gets the horizontal button text padding.</summary>
    public float ButtonTextPadding { get; init; } = 8f;

    /// <summary>Gets the horizontal chip text padding.</summary>
    public float ChipTextPadding { get; init; } = 6f;

    /// <summary>Gets the horizontal centered-text padding.</summary>
    public float CenterTextPadding { get; init; } = 4f;

    /// <summary>Gets the horizontal field text padding.</summary>
    public float FieldTextPadding { get; init; } = 6f;

    /// <summary>Gets the tab label left padding.</summary>
    public float TabTextPaddingLeft { get; init; } = 10f;

    /// <summary>Gets the tab label right padding.</summary>
    public float TabTextPaddingRight { get; init; } = 8f;

    /// <summary>Gets the right-aligned glyph guard padding.</summary>
    public float RightGlyphPadding { get; init; } = 2f;

    /// <summary>Gets the spacing between toolbar controls.</summary>
    public float ToolbarSpacing { get; init; } = 5f;

    /// <summary>Gets the compact list spacing.</summary>
    public float CompactSpacing { get; init; } = 4f;

    /// <summary>Gets the loose list spacing.</summary>
    public float LooseSpacing { get; init; } = 8f;

    /// <summary>Gets the spacing between status bar items.</summary>
    public float StatusSpacing { get; init; } = 16f;

    /// <summary>Gets the right margin after a transform tool group.</summary>
    public float TransformGroupMarginRight { get; init; } = 4f;

    /// <summary>Gets the menu bar content padding.</summary>
    public Vec4 MenuBarPadding { get; init; } = (0, 0, 10f, 0);

    /// <summary>Gets the toolbar content padding.</summary>
    public Vec4 ToolbarPadding { get; init; } = (6f, 4f, 6f, 4f);

    /// <summary>Gets the brand area content padding.</summary>
    public Vec4 BrandPadding { get; init; } = (10f, 0, 0, 0);

    /// <summary>Gets the transform tool group padding.</summary>
    public Vec4 TransformGroupPadding { get; init; } = (0, 0, 8f, 0);

    /// <summary>Gets the viewport header content padding.</summary>
    public Vec4 ViewportHeaderPadding { get; init; } = (7f, 2.5f, 7f, 3.5f);

    /// <summary>Gets the asset toolbar content padding.</summary>
    public Vec4 AssetToolbarPadding { get; init; } = (7f, 2f, 7f, 3f);

    /// <summary>Gets the status bar content padding.</summary>
    public Vec4 StatusBarPadding { get; init; } = (10f, 0, 10f, 0);

    /// <summary>Gets the panel title content padding.</summary>
    public Vec4 PanelTitlePadding { get; init; } = (8f, 0, 8f, 0);

    /// <summary>Gets the active tab accent bar thickness.</summary>
    public float ActiveTabAccentHeight { get; init; } = 2f;

    /// <summary>Gets the active tab accent bar top offset.</summary>
    public float ActiveTabAccentOffset { get; init; } = 1f;

    /// <summary>Gets the inset panel list content padding.</summary>
    public Vec4 InsetPanelPadding { get; init; } = (10f, 10f, 10f, 10f);

    /// <summary>Gets the modal content padding.</summary>
    public Vec4 ModalContentPadding { get; init; } = (16f, 14f, 16f, 12f);

    /// <summary>Gets the tooltip text padding.</summary>
    public Vec4 TooltipPadding { get; init; } = (9f, 6f, 9f, 6f);

    /// <summary>Gets the legend swatch width.</summary>
    public float SwatchWidth { get; init; } = 12f;

    /// <summary>Gets the legend swatch height.</summary>
    public float SwatchHeight { get; init; } = 8f;

    /// <summary>Gets the label/value metric row height.</summary>
    public float MetricRowHeight { get; init; } = 18f;
}
