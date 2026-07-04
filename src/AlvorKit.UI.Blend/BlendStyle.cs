namespace AlvorKit.UI.Blend;

/// <summary>Applies Blender-inspired recipes to AlvorKit UI nodes.</summary>
public class BlendStyle(Font font, Font emphasisFont, BlendControlChrome? chrome, BlendPalette palette, BlendMetrics metrics)
{
    /// <summary>Creates a style with the default editor-shell palette and metrics.</summary>
    public BlendStyle(Font font) : this(font, font, null, BlendPalette.Default, BlendMetrics.EditorShell)
    {
    }

    /// <summary>Creates a style with a separate emphasis font and the default editor-shell palette and metrics.</summary>
    public BlendStyle(Font font, Font emphasisFont) : this(font, emphasisFont, null, BlendPalette.Default, BlendMetrics.EditorShell)
    {
    }

    /// <summary>Creates a texture-backed style with the default editor-shell palette and metrics.</summary>
    public BlendStyle(Font font, BlendControlChrome chrome) : this(font, font, chrome, BlendPalette.Default, BlendMetrics.EditorShell)
    {
    }

    /// <summary>Creates a texture-backed style with a separate emphasis font and the default editor-shell palette and metrics.</summary>
    public BlendStyle(Font font, Font emphasisFont, BlendControlChrome chrome) :
        this(font, emphasisFont, chrome, BlendPalette.Default, BlendMetrics.EditorShell)
    {
    }

    /// <summary>Creates a style with explicit palette and metrics using one font face.</summary>
    public BlendStyle(Font font, BlendPalette palette, BlendMetrics metrics) : this(font, font, null, palette, metrics)
    {
    }

    /// <summary>Creates a texture-backed style with explicit palette and metrics using one font face.</summary>
    public BlendStyle(Font font, BlendControlChrome chrome, BlendPalette palette, BlendMetrics metrics) : this(font, font, chrome, palette, metrics)
    {
    }

    /// <summary>Gets the active color palette.</summary>
    public BlendPalette Palette { get; } = palette;

    /// <summary>Gets the active layout metrics.</summary>
    public BlendMetrics Metrics { get; } = metrics;

    /// <summary>Gets the font at a concrete size.</summary>
    public FontSize FontSize(int size) => font.Size(size);

    /// <summary>Converts a physical pixel count to logical UI units for this style.</summary>
    public float PhysicalPixels(int pixels) => chrome?.UiPixels(pixels) ?? pixels;

    /// <summary>Applies the full-window vertical root layout.</summary>
    public void Root(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0)
        .InnerAlignmentSnapV(1f)
        .ColorV(Palette.AppBackground);

    /// <summary>Applies an explicit-position board layout.</summary>
    public void Board(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.Board)
        .InnerAlignmentSnapV(1f);

    /// <summary>Applies a self-height strip with the given fill and bottom rule.</summary>
    public void Strip(EntMut ent, float height, Vec4 color) => ent.Mutate()
        .Mutate(Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, height))
        .ColorV(color)
        .Mutate(node => BottomRule(node, () => Palette.Border));

    /// <summary>Applies the top application menu bar surface.</summary>
    public void MenuBar(EntMut ent) => Strip(ent, Metrics.MenuBarHeight, Palette.Panel);

    /// <summary>Applies the main tool strip surface.</summary>
    public void Toolbar(EntMut ent) => Strip(ent, Metrics.ToolbarHeight, Palette.Raised);

    /// <summary>Applies the bottom status strip surface.</summary>
    public void StatusBar(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.StatusBarHeight))
        .ColorV(Palette.Panel)
        .Mutate(node => TopRule(node, () => Palette.Border));

    /// <summary>Applies a plain panel fill.</summary>
    public void Panel(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel);

    /// <summary>Applies a raised panel title strip.</summary>
    public void PanelTitle(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.PanelTitleHeight))
        .ColorV(Palette.Raised)
        .Mutate(node => BottomRule(node, () => Palette.Border));

    /// <summary>Applies body text matching the HTML editor-shell reference.</summary>
    public void Text(EntMut ent) => ent.Mutate()
        .FontV(font)
        .FontSizeV(12)
        .TextColorV(Palette.Text)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextAlignmentSnapV(1f)
        .TextGlyphAlignmentSnapV(0f);

    /// <summary>Applies a semibold text treatment using the loaded font face.</summary>
    public void EmphasisText(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .FontV(emphasisFont);

    /// <summary>Applies smaller muted metadata text.</summary>
    public void MutedText(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .FontSizeV(11)
        .TextColorV(Palette.MutedText);

    /// <summary>Applies centered text for compact controls.</summary>
    public void CenterText(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .TextAlignmentV(Alignment.Center)
        .TextPaddingV((4f, 0, 4f, 0));

    /// <summary>Applies a menu item hit target with transparent idle fill.</summary>
    public void MenuItem(EntMut ent, Vec2 size) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .TextPaddingV((9f, 0, 9f, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .ColorF(() => ent.IsHoveredR || ent.IsFocusedR ? Palette.Hover : default);

    /// <summary>Applies a compact button matching the editor-shell chrome.</summary>
    public void Button(EntMut ent, Vec2 size, Func<bool> active) => ent.Mutate()
        .Mutate(CenterText)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .TextPaddingV((8f, 0, 8f, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .ColorF(() => ButtonFill(ent, active()))
        .TextColorF(() => active() ? Palette.Text : Palette.MutedText)
        .Mutate(node => Border(node, () => ButtonBorder(ent, active())));

    /// <summary>Builds a compact rounded button with its text rendered above the generated surface.</summary>
    public void Button(EntMut ent, Vec2 size, string text, int fontSize, Func<bool> active, Func<Vec4> outside)
    {
        ButtonFrame(ent, size);

        if (chrome == null)
        {
            ent.Mutate(CenterText)
                .FontSizeV(fontSize)
                .TextV(text)
                .ColorF(() => ButtonFill(ent, active()))
                .TextColorF(() => active() ? Palette.Text : Palette.MutedText)
                .Mutate(node => Border(node, () => ButtonBorder(ent, active())));
            return;
        }

        RoundedControlSurface(ent, size, active, outside);
        ControlLabel(ent, text, fontSize, active);
    }

    /// <summary>Applies a smaller toolbar chip.</summary>
    public void Chip(EntMut ent, Vec2 size)
    {
        Button(ent, size, () => false);

        ent.Mutate()
            .FontSizeV(11)
            .TextPaddingV((6f, 0, 6f, 0));
    }

    /// <summary>Builds a smaller rounded toolbar chip with its text rendered above the generated surface.</summary>
    public void Chip(EntMut ent, Vec2 size, string text, Func<Vec4> outside)
    {
        Button(ent, size, text, 11, () => false, outside);
    }

    /// <summary>Applies a static field-like surface.</summary>
    public void Field(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((1, 0))
        .SizeV((0, 24f))
        .TextPaddingV((6f, 0, 6f, 0))
        .TextColorV(Palette.MutedText)
        .ColorV(Palette.AppBackground)
        .Mutate(node => Border(node, () => Palette.Border));

    /// <summary>Applies a bottom-dock tab surface.</summary>
    public void Tab(EntMut ent, Vec2 size, bool active) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .TextPaddingV((10f, 0, 8f, 0))
        .ColorV(active ? Palette.Panel : Palette.Raised)
        .TextColorV(active ? Palette.Text : Palette.MutedText)
        .Mutate(node => RightRule(node, () => Palette.Border));

    /// <summary>Adds a one-pixel border around a node.</summary>
    public void Border(EntMut ent, Func<Vec4> color)
    {
        TopRule(ent, color);
        BottomRule(ent, color);
        LeftRule(ent, color);
        RightRule(ent, color);
    }

    /// <summary>Adds a top hairline rule.</summary>
    public void TopRule(EntMut ent, Func<Vec4> color) =>
        Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, Metrics.Hairline), color);

    /// <summary>Adds a bottom hairline rule.</summary>
    public void BottomRule(EntMut ent, Func<Vec4> color) =>
        Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, Metrics.Hairline), color);

    /// <summary>Adds a left hairline rule.</summary>
    public void LeftRule(EntMut ent, Func<Vec4> color) =>
        Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (Metrics.Hairline, 0), color);

    /// <summary>Adds a right hairline rule.</summary>
    public void RightRule(EntMut ent, Func<Vec4> color) =>
        Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (Metrics.Hairline, 0), color);

    private Vec4 ButtonFill(EntMut ent, bool active)
    {
        if (active)
            return Palette.ActiveSurface;
        if (ent.IsPressedR)
            return Palette.Selection;
        if (ent.IsHoveredR || ent.IsFocusedR)
            return Palette.Hover;
        return Palette.Panel;
    }

    private Vec4 ButtonBorder(EntMut ent, bool active)
    {
        if (active)
            return Palette.Accent;
        if (ent.IsHoveredR || ent.IsFocusedR)
            return Palette.StrongBorder;
        return Palette.Border;
    }

    private void ButtonFrame(EntMut ent, Vec2 size) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    private void RoundedControlSurface(EntMut ent, Vec2 size, Func<bool> active, Func<Vec4> outside)
    {
        if (chrome == null)
            return;

        var capPixels = chrome.PhysicalPixels(Metrics.ControlRadius);
        var capWidth = chrome.UiPixels(capPixels);
        var borderWidth = Metrics.ControlBorderWidth;
        var middleWidth = -(capWidth * 2f);

        Node(ent)
            .IsFloatingV(true)
            .SizeRelativeV((0, 0))
            .SizeV((capWidth, size.Y))
            .TextureF(() => ControlCap(ent, size.Y, active, outside));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 0))
            .SizeV((capWidth, size.Y))
            .TextureF(() => ControlCap(ent, size.Y, active, outside))
            .TextureFlipV(SpriteBatchFlip.Horizontal);

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active()));

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, borderWidth))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, size.Y - (borderWidth * 2f)))
            .ColorF(() => ButtonFill(ent, active()));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active()));
    }

    private Texture2D ControlCap(EntMut ent, float height, Func<bool> active, Func<Vec4> outside)
    {
        Debug.Assert(chrome != null);
        return chrome.Cap(
            height,
            Metrics.ControlRadius,
            Metrics.ControlBorderWidth,
            ButtonFill(ent, active()),
            ButtonBorder(ent, active()),
            outside());
    }

    private void ControlLabel(EntMut ent, string text, int fontSize, Func<bool> active) =>
        Node(ent)
            .IsFloatingV(true)
            .SizeRelativeV((1, 1))
            .Mutate(CenterText)
            .FontSizeV(fontSize)
            .TextColorF(() => active() ? Palette.Text : Palette.MutedText)
            .TextV(text);

    private void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, Func<Vec4> color) =>
        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorF(color);
}
