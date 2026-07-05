namespace AlvorKit.UI.Blend;

/// <summary>Applies Blender-inspired recipes to AlvorKit UI nodes.</summary>
public class BlendStyle(BlendStyleOptions options)
{
    private readonly Font font = options.Font;
    private readonly Font emphasisFont = options.EmphasisFont ?? options.Font;
    private readonly BlendControlChrome? chrome = options.Chrome;

    /// <summary>Gets the active color palette.</summary>
    public virtual BlendPalette Palette => BlendPalette.Default;

    /// <summary>Gets the active layout metrics.</summary>
    public virtual BlendMetrics Metrics { get; } = new(
        25f,
        35f,
        22f,
        28f,
        29f,
        29f,
        31f,
        24f,
        26f,
        24f,
        23f,
        26f,
        2f,
        1f,
        1f,
        12,
        11,
        12,
        11,
        10,
        9f,
        8f,
        6f,
        4f,
        6f,
        10f,
        8f,
        2f,
        5f,
        4f,
        8f,
        16f,
        4f,
        (0, 0, 10f, 0),
        (6f, 4f, 6f, 4f),
        (10f, 0, 0, 0),
        (0, 0, 8f, 0),
        (7f, 2.5f, 7f, 3.5f),
        (7f, 2f, 7f, 3f),
        (10f, 0, 10f, 0),
        (8f, 0, 8f, 0),
        2f);

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

    private void Strip(EntMut ent, float height, Vec4 color) => ent.Mutate()
        .Mutate(Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, height))
        .ColorV(color)
        .Mutate(BottomRule);

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
        .Mutate(TopRule);

    /// <summary>Applies a plain panel fill.</summary>
    public void Panel(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel);

    /// <summary>Applies a raised panel title strip.</summary>
    public void PanelTitle(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.PanelTitleHeight))
        .ColorV(Palette.Raised)
        .Mutate(BottomRule);

    /// <summary>Applies body text matching the HTML editor-shell reference.</summary>
    public void Text(EntMut ent) => ent.Mutate()
        .FontV(font)
        .FontSizeV(Metrics.TextFontSize)
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
        .FontSizeV(Metrics.MutedFontSize)
        .TextColorV(Palette.MutedText);

    /// <summary>Applies centered text for compact controls.</summary>
    public void CenterText(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .TextAlignmentV(Alignment.Center)
        .TextPaddingV((Metrics.CenterTextPadding, 0, Metrics.CenterTextPadding, 0));

    /// <summary>Applies a menu item hit target with transparent idle fill.</summary>
    public void MenuItem(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .TextPaddingV((Metrics.MenuItemTextPadding, 0, Metrics.MenuItemTextPadding, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .ColorF(() => ent.IsHoveredR || ent.IsFocusedR ? Palette.Hover : default);

    /// <summary>Builds a compact rounded button using the standard Blend button font size.</summary>
    public void Button(EntMut ent) =>
        Button(ent, Metrics.ButtonHeight, Metrics.ButtonFontSize, Metrics.ButtonTextPadding, false);

    /// <summary>Builds an active compact rounded button using the standard Blend button font size.</summary>
    public void ActiveButton(EntMut ent) =>
        Button(ent, Metrics.ButtonHeight, Metrics.ButtonFontSize, Metrics.ButtonTextPadding, true);

    /// <summary>Builds a compact rounded button sized for title rows and toolbar strips.</summary>
    public void ToolbarButton(EntMut ent)
    {
        Button(ent, Metrics.ToolbarButtonHeight, Metrics.ButtonFontSize, Metrics.ButtonTextPadding, false);

        ent.Mutate()
            .OffsetV((0, -Metrics.Hairline));
    }

    /// <summary>Builds an active compact rounded button sized for title rows and toolbar strips.</summary>
    public void ActiveToolbarButton(EntMut ent)
    {
        Button(ent, Metrics.ToolbarButtonHeight, Metrics.ButtonFontSize, Metrics.ButtonTextPadding, true);

        ent.Mutate()
            .OffsetV((0, -Metrics.Hairline));
    }

    /// <summary>Builds a compact square button using the standard Blend square-button font size.</summary>
    public void SquareButton(EntMut ent)
    {
        FixedButton(
            ent,
            (Metrics.SquareButtonSize, Metrics.SquareButtonSize),
            Metrics.SquareButtonFontSize,
            false);
    }

    /// <summary>Builds an active compact square button using the standard Blend square-button font size.</summary>
    public void ActiveSquareButton(EntMut ent)
    {
        FixedButton(
            ent,
            (Metrics.SquareButtonSize, Metrics.SquareButtonSize),
            Metrics.SquareButtonFontSize,
            true);
    }

    /// <summary>Applies a smaller toolbar chip.</summary>
    public void Chip(EntMut ent)
        => Button(ent, Metrics.ChipHeight, Metrics.ChipFontSize, Metrics.ChipTextPadding, false);

    /// <summary>Applies a static field-like surface.</summary>
    public void Field(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.FieldHeight))
        .TextPaddingV((Metrics.FieldTextPadding, 0, Metrics.FieldTextPadding, 0))
        .TextColorV(Palette.MutedText)
        .ColorV(Palette.AppBackground)
        .Mutate(Border);

    /// <summary>Applies a bottom-dock tab surface sized from its text.</summary>
    public void Tab(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, Metrics.TabStripHeight))
        .TextPaddingV((Metrics.TabTextPaddingLeft, 0, Metrics.TabTextPaddingRight, 0))
        .ColorV(Palette.Raised)
        .TextColorV(Palette.MutedText)
        .Mutate(RightRule);

    /// <summary>Applies an active bottom-dock tab surface sized from its text.</summary>
    public void ActiveTab(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, Metrics.TabStripHeight))
        .TextPaddingV((Metrics.TabTextPaddingLeft, 0, Metrics.TabTextPaddingRight, 0))
        .ColorV(Palette.Panel)
        .TextColorV(Palette.Text)
        .Mutate(RightRule);

    /// <summary>Adds a one-pixel border around a node.</summary>
    public void Border(EntMut ent)
    {
        TopRule(ent);
        BottomRule(ent);
        LeftRule(ent);
        RightRule(ent);
    }

    /// <summary>Adds a top hairline rule.</summary>
    public void TopRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, Metrics.Hairline));

    /// <summary>Adds a bottom hairline rule.</summary>
    public void BottomRule(EntMut ent) =>
        Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, Metrics.Hairline));

    /// <summary>Adds a left hairline rule.</summary>
    public void LeftRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (Metrics.Hairline, 0));

    /// <summary>Adds a right hairline rule.</summary>
    public void RightRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (Metrics.Hairline, 0));

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

    private void FixedButton(EntMut ent, Vec2 size, int fontSize, bool active)
    {
        ButtonFrame(ent, size);

        if (chrome == null)
        {
            ent.Mutate(CenterText)
                .FontSizeV(fontSize)
                .ColorF(() => ButtonFill(ent, active))
                .TextColorF(() => active ? Palette.Text : Palette.MutedText)
                .Mutate(node => ControlBorder(node, active));
            return;
        }

        RoundedControlSurface(ent, size, active);
        ControlLabel(ent, fontSize, active);
    }

    private void Button(EntMut ent, float height, int fontSize, float horizontalPadding, bool active)
    {
        MeasuredButtonFrame(ent, height, fontSize, horizontalPadding);

        if (chrome == null)
        {
            ent.Mutate(CenterText)
                .FontSizeV(fontSize)
                .TextPaddingV((horizontalPadding, 0, horizontalPadding, 0))
                .TextColorF(() => active ? Palette.Text : Palette.MutedText)
                .ColorF(() => ButtonFill(ent, active))
                .Mutate(node => ControlBorder(node, active));
            return;
        }

        RoundedControlSurface(ent, (0, height), active);
        ControlLabel(ent, fontSize, active);
    }

    private void ButtonFrame(EntMut ent, Vec2 size) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    private void MeasuredButtonFrame(EntMut ent, float height, int fontSize, float horizontalPadding) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, height))
        .FontV(font)
        .FontSizeV(fontSize)
        .TextPaddingV((horizontalPadding, 0, horizontalPadding, 0))
        .TextColorV(default)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    private void RoundedControlSurface(EntMut ent, Vec2 size, bool active)
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
            .TextureF(() => ControlCap(ent, size.Y, active));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 0))
            .SizeV((capWidth, size.Y))
            .TextureF(() => ControlCap(ent, size.Y, active))
            .TextureFlipV(SpriteBatchFlip.Horizontal);

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active));

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, borderWidth))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, size.Y - (borderWidth * 2f)))
            .ColorF(() => ButtonFill(ent, active));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active));
    }

    private Texture2D ControlCap(EntMut ent, float height, bool active)
    {
        Debug.Assert(chrome != null);
        return chrome.Cap(
            height,
            Metrics.ControlRadius,
            Metrics.ControlBorderWidth,
            ButtonFill(ent, active),
            ButtonBorder(ent, active));
    }

    private void ControlLabel(EntMut ent, int fontSize, bool active) =>
        Node(ent)
            .IsFloatingV(true)
            .SizeRelativeV((1, 1))
            .Mutate(CenterText)
            .FontSizeV(fontSize)
            .TextColorF(() => active ? Palette.Text : Palette.MutedText)
            .TextF(() => ent.TextFV.Resolve());

    private void ControlBorder(EntMut ent, bool active)
    {
        ControlRule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, Metrics.Hairline), active);
        ControlRule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, Metrics.Hairline), active);
        ControlRule(ent, Alignment.Top | Alignment.Left, (0, 1), (Metrics.Hairline, 0), active);
        ControlRule(ent, Alignment.Top | Alignment.Right, (0, 1), (Metrics.Hairline, 0), active);
    }

    private void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorV(Palette.Border);

    private void ControlRule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, bool active) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorF(() => ButtonBorder(ent, active));
}
