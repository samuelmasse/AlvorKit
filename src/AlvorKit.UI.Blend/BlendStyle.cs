namespace AlvorKit.UI.Blend;

/// <summary>Applies Blender-inspired recipes to AlvorKit UI nodes, using the embedded Inter faces from <see cref="RootInter"/>.</summary>
public class BlendStyle(RootInter inter, RootGl gl, RootUiScale scale, RootKeyboard keyboard)
{
    private readonly Font font = inter.Regular;
    private readonly Font emphasisFont = inter.SemiBold;
    private readonly BlendControlChrome chrome = new(gl, scale);

    /// <summary>Gets the active color palette.</summary>
    public virtual BlendPalette Palette => BlendPalette.Default;

    /// <summary>Gets the active layout metrics.</summary>
    public virtual BlendMetrics Metrics { get; } = new();

    /// <summary>Gets the regular text font face, for collaborators that measure text.</summary>
    public Font TextFont => font;

    /// <summary>
    /// Applies the full-window vertical root layout. The root silently takes focus when empty chrome is
    /// pressed, giving Blend apps editor-shell click-away semantics (field edits commit, popups close)
    /// without imposing that policy on other UI styles at the engine level.
    /// </summary>
    public void Root(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(0)
        .InnerAlignmentSnapV(1f)
        .ColorV(Palette.AppBackground)
        .IsSelectableV(true)
        .IsSilentFocusableV(true);

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

    /// <summary>Applies a vertical panel body that fills the available space.</summary>
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
        .PaddingV(Metrics.InsetPanelPadding)
        .Mutate(BottomRule);

    /// <summary>Applies a padded vertical list body.</summary>
    public void ListBody(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .PaddingV((Metrics.ButtonTextPadding, Metrics.ButtonTextPadding, Metrics.ButtonTextPadding, Metrics.ButtonTextPadding))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(Metrics.CompactSpacing);

    /// <summary>Applies a vertical list sized from its children.</summary>
    public void VerticalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((0, 1))
        .SizeInnerMaxRelativeV((1, 0));

    /// <summary>Applies a horizontal list sized from its children.</summary>
    public void HorizontalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .SizeInnerMaxRelativeV((0, 1));

    /// <summary>Applies a full-size weighted horizontal list.</summary>
    public void HorizontalFill(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .SizeRelativeV((1, 1));

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
        .ColorF(() => ent.IsHoveredR ? Palette.Hover : default)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

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

    /// <summary>Applies a menu item hit target with transparent idle fill.</summary>
    public void MenuItem(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .TextPaddingV((Metrics.MenuItemTextPadding, 0, Metrics.MenuItemTextPadding, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .ColorF(() => ent.IsHoveredR ? Palette.Hover : default)
        .Mutate(ActivateOnEnter);

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

    /// <summary>Applies a non-interactive readout chip; it stays hoverable so it can carry a tooltip, but never reacts.</summary>
    public void ReadoutChip(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, Metrics.ChipHeight))
        .FontV(font)
        .FontSizeV(Metrics.ChipFontSize)
        .TextPaddingV((Metrics.ChipTextPadding, 0, Metrics.ChipTextPadding, 0))
        .TextAlignmentV(Alignment.Center)
        .TextColorV(Palette.MutedText)
        .ColorV(Palette.Panel)
        .IsSelectableV(true)
        .Mutate(Border);

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
        .Mutate(RightRule)
        .Mutate(BottomRule);

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

    /// <summary>Adds the accent bar that marks an active tab, sparing the tab's right separator.</summary>
    public void ActiveTabAccent(EntMut ent) =>
        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .OffsetV((0, Metrics.ActiveTabAccentOffset))
            .SizeRelativeV((1, 0))
            .SizeV((-Metrics.Hairline, Metrics.ActiveTabAccentHeight))
            .ColorV(Palette.Accent);

    /// <summary>Applies a raised tab strip surface; tabs carry the bottom rule so active tabs stay open to the content below.</summary>
    public void TabStrip(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.TabStripHeight))
        .ColorV(Palette.Raised)
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerSpacingV(0);

    /// <summary>Fills the tab strip after the last tab and carries its bottom rule.</summary>
    public void TabFiller(EntMut ent) => ent.Mutate()
        .ColorV(default)
        .Mutate(BottomRule);

    /// <summary>Applies a vertical dock panel surface with a bottom separator.</summary>
    public void Dock(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 1))
        .ColorV(Palette.Panel)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .Mutate(BottomRule);

    /// <summary>Applies a thin vertical splitter between docks.</summary>
    public void Splitter(EntMut ent) => ent.Mutate()
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 1))
        .ColorV(Palette.AppBackground)
        .Mutate(LeftRule)
        .Mutate(RightRule);

    /// <summary>Adds a one-pixel border around a node.</summary>
    public void Border(EntMut ent)
    {
        TopRule(ent);
        BottomRule(ent);
        LeftRule(ent);
        RightRule(ent);
    }

    /// <summary>Adds a one-pixel strong border around a node.</summary>
    public void StrongBorder(EntMut ent)
    {
        Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, Metrics.Hairline), Palette.StrongBorder);
        Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, Metrics.Hairline), Palette.StrongBorder);
        Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (Metrics.Hairline, 0), Palette.StrongBorder);
        Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (Metrics.Hairline, 0), Palette.StrongBorder);
    }

    /// <summary>Adds a top hairline rule.</summary>
    public void TopRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, Metrics.Hairline), Palette.Border);

    /// <summary>Adds a bottom hairline rule.</summary>
    public void BottomRule(EntMut ent) =>
        Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, Metrics.Hairline), Palette.Border);

    /// <summary>Adds a left hairline rule.</summary>
    public void LeftRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (Metrics.Hairline, 0), Palette.Border);

    /// <summary>Adds a right hairline rule.</summary>
    public void RightRule(EntMut ent) =>
        Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (Metrics.Hairline, 0), Palette.Border);

    /// <summary>Applies the full-screen tinted layer behind a modal dialog.</summary>
    public void ModalLayer(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerAlignmentSnapV(1f)
        .ColorV(Palette.Scrim)
        .IsSelectableV(true)
        .IsSilentFocusableV(true);

    /// <summary>Applies a centered modal dialog panel.</summary>
    public void ModalPanel(EntMut ent) => ent.Mutate()
        .ColorV(Palette.Panel)
        .SizeRelativeV((0, 0))
        .AlignmentV(Alignment.Horizontal | Alignment.Vertical)
        .AlignmentSnapV(1f)
        .InnerAlignmentSnapV(1f)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .IsSelectableV(true)
        .IsSilentFocusableV(true)
        .Mutate(StrongBorder);

    /// <summary>Applies a modal dialog's padded content area.</summary>
    public void ModalContent(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .PaddingV(Metrics.ModalContentPadding)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerAlignmentSnapV(1f);

    /// <summary>Applies a floating tooltip surface that sizes to its line children.</summary>
    public void Tooltip(EntMut ent) => ent.Mutate()
        .Mutate(VerticalList)
        .InnerSpacingV(Metrics.TooltipLineSpacing)
        .PaddingV(Metrics.TooltipPadding)
        .ColorV(Palette.Raised)
        .Mutate(StrongBorder);

    /// <summary>Applies the emphasized first line of a tooltip.</summary>
    public void TooltipTitle(EntMut ent) => ent.Mutate()
        .Mutate(EmphasisText)
        .FontSizeV(Metrics.MutedFontSize)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a muted tooltip detail line.</summary>
    public void TooltipLine(EntMut ent) => ent.Mutate()
        .Mutate(MutedText)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Applies a small legend swatch; set the color at the call site.</summary>
    public void Swatch(EntMut ent) => ent.Mutate()
        .SizeRelativeV((0, 0))
        .SizeV((Metrics.SwatchWidth, Metrics.SwatchHeight))
        .AlignmentV(Alignment.Vertical);

    /// <summary>Applies a fixed-height label/value metric row.</summary>
    public void MetricRow(EntMut ent) => ent.Mutate()
        .Mutate(Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, Metrics.MetricRowHeight));

    private Vec4 ButtonFill(EntMut ent, bool active)
    {
        if (active)
            return Palette.ActiveSurface;
        if (ent.IsPressedR)
            return Palette.Selection;
        if (ent.IsHoveredR)
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
        RoundedControlSurface(ent, size, active);
        ControlLabel(ent, fontSize, active);
    }

    private void Button(EntMut ent, float height, int fontSize, float horizontalPadding, bool active)
    {
        MeasuredButtonFrame(ent, height, fontSize, horizontalPadding);
        RoundedControlSurface(ent, (0, height), active);
        ControlLabel(ent, fontSize, active);
    }

    private void ButtonFrame(EntMut ent, Vec2 size) => ent.Mutate()
        .Mutate(Board)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(ActivateOnEnter);

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
        .CursorF(() => CursorShape.Hand)
        .Mutate(ActivateOnEnter);

    /// <summary>Runs the node's click (or press) callback when it is focused and Enter is pressed.</summary>
    public void ActivateOnEnter(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate()
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                {
                    var click = ent.OnClickFV.Resolve();
                    if (click != null)
                        click();
                    else
                        ent.OnPressFV.Resolve()?.Invoke();
                }

                enterWasDown = enterDown;
            });
    }

    private void RoundedControlSurface(EntMut ent, Vec2 size, bool active)
    {
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

    /// <summary>Adds a floating hairline rule node with a fixed color.</summary>
    public static void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, Vec4 color) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorV(color);

    /// <summary>Adds a floating hairline rule node with a reactive color.</summary>
    public static void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, Func<Vec4> color) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorF(color);
}
