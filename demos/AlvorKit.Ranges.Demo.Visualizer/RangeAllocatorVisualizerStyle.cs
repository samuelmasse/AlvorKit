namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Shared visual language for the range allocator visualizer UI.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerStyle(RootRoboto roboto, RootKeyboard keyboard)
{
    internal Vec4 BackgroundColor => (0.025f, 0.03f, 0.038f, 1f);
    internal Vec4 PanelColor => (0.07f, 0.08f, 0.095f, 0.94f);
    internal Vec4 PanelRaisedColor => (0.105f, 0.115f, 0.13f, 0.94f);
    internal Vec4 PanelInsetColor => (0.045f, 0.05f, 0.058f, 1f);
    internal Vec4 ModalTintColor => (0.01f, 0.012f, 0.016f, 0.68f);
    internal Vec4 TextColor => (0.9f, 0.93f, 0.95f, 1f);
    internal Vec4 MutedTextColor => (0.58f, 0.64f, 0.68f, 1f);
    internal Vec4 AccentColor => (0.25f, 0.8f, 0.95f, 1f);
    internal Vec4 WarmAccentColor => (0.98f, 0.72f, 0.3f, 1f);
    internal Vec4 ModalBorderColor => (0.22f, 0.38f, 0.44f, 1f);
    internal Vec4 TooltipColor => (0.04f, 0.055f, 0.066f, 0.98f);

    internal float SpacingXS => 4f;
    internal float SpacingS => 6f;
    internal float Spacing => 10f;
    internal float SpacingL => 12f;
    internal float RootPadding => SpacingL;
    internal float PanelPadding => 14f;
    internal float FloatingTextInset => PanelPadding + SpacingS;
    internal float ButtonHeight => 24f;
    internal float PickerScreenInset => 36f;
    internal float PickerOptionHeight => 48f;
    internal float ModalVerticalPadding => (PanelPadding + Spacing) + PanelPadding;
    internal float ModalHeaderHeight => 36f;
    internal float TooltipOffset => 14f;
    internal float TooltipLift => 30f;

    internal int FontSizeSmall => 10;
    internal int FontSizeBody => 12;
    internal int FontSizeHeader => 15;
    internal int FontSizeTitle => 22;

    /// <summary>Configures the full-screen root node.</summary>
    internal void Root(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSizingV(InnerSizing.VerticalWeight)
        .InnerSpacingV(Spacing)
        .InnerAlignmentSnapV(1f)
        .PaddingV((RootPadding, RootPadding, RootPadding, RootPadding))
        .ColorV(BackgroundColor);

    /// <summary>Configures a full-width panel.</summary>
    internal void Panel(EntMut ent) => ent.Mutate()
        .ColorV(PanelColor)
        .PaddingV((PanelPadding, PanelPadding, PanelPadding, PanelPadding));

    /// <summary>Configures a filled vertical list panel.</summary>
    internal void PanelList(EntMut ent) => ent.Mutate()
        .Mutate(Panel)
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(Spacing)
        .SizeRelativeV((1, 1));

    /// <summary>Configures a compact vertical list that sizes itself from its children.</summary>
    internal void VerticalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(SpacingS)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((0, 1))
        .SizeInnerMaxRelativeV((1, 0));

    /// <summary>Configures a compact horizontal list that sizes itself from its children.</summary>
    internal void HorizontalList(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSpacingV(SpacingS)
        .SizeRelativeV((0, 0))
        .SizeInnerSumRelativeV((1, 0))
        .SizeInnerMaxRelativeV((0, 1));

    /// <summary>Configures a full-width horizontal list.</summary>
    internal void HorizontalFill(EntMut ent) => ent.Mutate()
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerSpacingV(Spacing)
        .SizeRelativeV((1, 1));

    /// <summary>Configures common text rendering.</summary>
    internal void Text(EntMut ent) => ent.Mutate()
        .FontV(roboto.Font)
        .FontSizeV(FontSizeBody)
        .TextColorV(TextColor)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextAlignmentSnapV(1f);

    /// <summary>Configures a compact label that sizes to its text.</summary>
    internal void Label(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    /// <summary>Configures a muted compact label.</summary>
    internal void MutedLabel(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeSmall)
        .TextColorV(MutedTextColor);

    /// <summary>Configures a title label.</summary>
    internal void Title(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeTitle)
        .TextColorV(TextColor);

    /// <summary>Configures a section heading label.</summary>
    internal void Heading(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeHeader)
        .TextColorV(TextColor);

    /// <summary>Configures an interactive text button.</summary>
    internal void Button(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate()
            .Mutate(Text)
            .SizeRelativeV((0, 0))
            .SizeV((92f, ButtonHeight))
            .TextAlignmentV(Alignment.Center)
            .TextPaddingV((SpacingS, 0, SpacingS, 0))
            .IsSelectableV(true)
            .IsFocusableV(true)
            .CursorF(() => ent.IsInputDisabledFV.Resolve() ? CursorShape.Default : CursorShape.Hand)
            .ColorF(() => ButtonFill(ent))
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                    ent.OnPressFV.Resolve()?.Invoke();

                enterWasDown = enterDown;
            })
            .Mutate(ButtonBorder);
    }

    /// <summary>Configures the clickable scenario name in the header.</summary>
    internal void ScenarioLink(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeHeader)
        .TextColorF(() => ent.IsHoveredR || ent.IsFocusedR ? AccentColor : WarmAccentColor)
        .TextPaddingV((0, 0, SpacingS, 1f))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(ScenarioLinkUnderline);

    /// <summary>Configures the full-screen layer behind the scenario-picker dialog.</summary>
    internal void ModalLayer(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerAlignmentSnapV(1f)
        .ColorV(ModalTintColor);

    /// <summary>Configures the centered scenario-picker dialog panel.</summary>
    internal void ModalPanel(EntMut ent) => ent.Mutate()
        .ColorV(PanelColor)
        .PaddingV(default)
        .SizeRelativeV((0, 0))
        .SizeAlignmentSnapV(2f)
        .AlignmentV(Alignment.Horizontal | Alignment.Vertical)
        .AlignmentSnapV(1f)
        .InnerAlignmentSnapV(1f)
        .IsSelectableV(true)
        .IsSilentFocusableV(true)
        .Mutate(node => Border(node, () => ModalBorderColor));

    /// <summary>Configures the modal's padded content area.</summary>
    internal void ModalContent(EntMut ent) => ent.Mutate()
        .PaddingV((PanelPadding + Spacing, PanelPadding, PanelPadding + Spacing, PanelPadding))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(SpacingS)
        .InnerAlignmentSnapV(1f)
        .SizeRelativeV((1, 1));

    /// <summary>Configures one two-line scenario option row.</summary>
    internal void PickerOption(EntMut ent, Func<bool> selected) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .SizeV((0, PickerOptionHeight))
        .ColorF(() => PickerOptionFill(ent, selected()))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(node => Border(node, () => PickerOptionBorder(node, selected())));

    /// <summary>Configures the floating tooltip readout.</summary>
    internal void Tooltip(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .FontSizeV(FontSizeSmall)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextColorV(TextColor)
        .TextPaddingV((Spacing, SpacingS, Spacing, SpacingS))
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1))
        .ColorF(() => ent.TextFV.Resolve().Length == 0 ? default : TooltipColor)
        .Mutate(node => Border(node, () => ent.TextFV.Resolve().Length == 0 ? default : ModalBorderColor));

    /// <summary>Configures a small timeline or memory legend swatch.</summary>
    internal void Swatch(EntMut ent, Vec4 color) => ent.Mutate()
        .SizeRelativeV((0, 0))
        .SizeV((18f, 10f))
        .AlignmentV(Alignment.Vertical)
        .ColorV(color);

    /// <summary>Configures a full-width fixed-height metric row.</summary>
    internal void MetricRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .SizeV((0, 16f));

    private void ScenarioLinkUnderline(EntMut ent)
    {
        Node(ent)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .SizeRelativeV((1, 0))
            .SizeV((0, 1f))
            .ColorF(() => ent.IsHoveredR || ent.IsFocusedR ? AccentColor : WarmAccentColor);
    }

    private void ButtonBorder(EntMut ent) => Border(ent, () => ButtonBorderColor(ent));

    private void Border(EntMut ent, Func<Vec4> color)
    {
        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .SizeRelativeV((1, 0))
            .SizeV((0, 1f))
            .ColorF(color);

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .SizeRelativeV((1, 0))
            .SizeV((0, 1f))
            .ColorF(color);

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .SizeRelativeV((0, 1))
            .SizeV((1f, 0))
            .ColorF(color);

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 1))
            .SizeV((1f, 0))
            .ColorF(color);
    }

    private Vec4 ButtonFill(EntMut ent)
    {
        if (ent.IsInputDisabledFV.Resolve())
            return (0.1f, 0.11f, 0.13f, 0.75f);

        if (ent.IsPressedR)
            return (0.18f, 0.34f, 0.38f, 1f);

        if (ent.IsFocusedR || ent.IsHoveredR)
            return (0.14f, 0.24f, 0.29f, 1f);

        return PanelRaisedColor;
    }

    private Vec4 ButtonBorderColor(EntMut ent)
    {
        if (ent.IsInputDisabledFV.Resolve())
            return (0.18f, 0.19f, 0.21f, 1f);

        if (ent.IsFocusedR || ent.IsHoveredR)
            return AccentColor;

        return (0.17f, 0.19f, 0.22f, 1f);
    }

    private Vec4 PickerOptionFill(EntMut ent, bool selected)
    {
        if (selected)
            return (0.16f, 0.18f, 0.15f, 1f);

        if (ent.IsPressedR)
            return (0.16f, 0.24f, 0.27f, 1f);

        if (ent.IsFocusedR || ent.IsHoveredR)
            return (0.12f, 0.16f, 0.18f, 1f);

        return PanelInsetColor;
    }

    private Vec4 PickerOptionBorder(EntMut ent, bool selected)
    {
        if (selected)
            return WarmAccentColor;

        if (ent.IsFocusedR || ent.IsHoveredR)
            return AccentColor;

        return (0.16f, 0.19f, 0.22f, 1f);
    }
}
