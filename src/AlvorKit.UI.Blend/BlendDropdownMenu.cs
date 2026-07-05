namespace AlvorKit.UI.Blend;

/// <summary>
/// Root-layer dropdown popup shared by every <see cref="BlendFields"/> dropdown: one field opens it at a
/// time with its items and pick callback. Mount it near the end of the root tree (before the tooltip layer)
/// so the popup floats over the app; while open, the full-root layer catches press-away and keyboard input.
/// </summary>
public class BlendDropdownMenu(Keyboard keyboard, BlendStyle style)
{
    private EntMut anchor;
    private IReadOnlyList<BlendDropdownItem>? items;
    private Action<int>? onPick;
    private int selectedIndex;
    private int highlightIndex;
    private long revision;
    private bool openedThisUpdate;

    /// <summary>Gets whether a dropdown popup is open.</summary>
    public bool IsOpen => items != null;

    /// <summary>Returns whether the popup is currently anchored to the supplied field node.</summary>
    public bool IsOpenFor(EntMut field) => IsOpen && anchor == field;

    /// <summary>Opens the popup under a field with the field's options; a pick invokes <paramref name="onPick"/> with the option index.</summary>
    public void Open(EntMut anchor, IReadOnlyList<BlendDropdownItem> items, int selectedIndex, Action<int> onPick)
    {
        this.anchor = anchor;
        this.items = items;
        this.onPick = onPick;
        this.selectedIndex = selectedIndex;
        highlightIndex = selectedIndex;
        revision++;
        openedThisUpdate = true;
    }

    /// <summary>Closes the popup without picking.</summary>
    public void Close()
    {
        anchor = default;
        items = null;
        onPick = null;
    }

    public void Create(EntMut root)
    {
        var builtRevision = 0L;

        Node(root, out var layer)
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => !IsOpen)
            .IsSelectableV(true)
            .IsSilentFocusableV(true)
            .OnPressF(Close)
            .OnUpdateF(() =>
            {
                if (!IsOpen)
                    return;

                if (openedThisUpdate)
                {
                    openedThisUpdate = false;
                    return;
                }

                if (keyboard.IsKeyPressed(Keys.Escape))
                {
                    Close();
                    return;
                }

                var count = items!.Count;
                if (keyboard.IsKeyPressedRepeated(Keys.Up))
                    highlightIndex = (highlightIndex - 1 + count) % count;
                if (keyboard.IsKeyPressedRepeated(Keys.Down))
                    highlightIndex = (highlightIndex + 1) % count;
                if (keyboard.IsKeyPressed(Keys.Enter))
                    Pick(highlightIndex);
            });
        {
            Node(layer, out var panel)
                .IsFloatingV(true)
                .SizeRelativeV((0, 0))
                .SizeF(() => (anchor.SizeR.X, PanelHeight()))
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSpacingV(0)
                .ColorV(style.Palette.Raised)
                .PaddingV((
                    style.Metrics.DropdownPopupPadding,
                    style.Metrics.DropdownPopupPadding,
                    style.Metrics.DropdownPopupPadding,
                    style.Metrics.DropdownPopupPadding))
                .OffsetF(() => PanelOffset(root))
                .Mutate(style.StrongBorder)
                .OnUpdateF(() =>
                {
                    if (builtRevision == revision || !IsOpen)
                        return;

                    builtRevision = revision;
                    RebuildRows(panel);
                });
        }

        void RebuildRows(EntMut panel)
        {
            NodesClear(panel);

            var count = items!.Count;
            for (var i = 0; i < count; i++)
            {
                var index = i;
                var item = items[i];

                Node(panel, out var row)
                    .Mutate(style.Board)
                    .SizeRelativeV((1, 0))
                    .SizeV((0, style.Metrics.DropdownOptionHeight))
                    .IsSelectableV(true)
                    .CursorF(() => CursorShape.Hand)
                    .ColorF(() => RowFill(index))
                    .OnPressF(() => Pick(index))
                    .OnUpdateF(() =>
                    {
                        if (row.IsHoveredR)
                            highlightIndex = index;
                    });
                {
                    if (index == selectedIndex)
                    {
                        Node(row)
                            .IsFloatingV(true)
                            .SizeRelativeV((0, 1))
                            .SizeV((3, 0))
                            .ColorV(style.Palette.Accent);
                    }

                    var textLeft = style.Metrics.TabTextPaddingLeft;
                    if (item.Swatch.W > 0)
                    {
                        Node(row)
                            .Mutate(style.Swatch)
                            .IsFloatingV(true)
                            .AlignmentV(Alignment.Left | Alignment.Vertical)
                            .OffsetV((textLeft, 0))
                            .ColorV(item.Swatch);
                        textLeft += style.Metrics.SwatchWidth + style.Metrics.CompactSpacing;
                    }

                    Node(row)
                        .Mutate(style.CellLabel)
                        .IsFloatingV(true)
                        .TextPaddingV((textLeft, 0, style.Metrics.TabTextPaddingRight, 0))
                        .TextV(item.Text);

                    if (index == selectedIndex)
                    {
                        Node(row)
                            .Mutate(style.MutedCellLabel)
                            .IsFloatingV(true)
                            .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                            .TextPaddingV((0, 0, style.Metrics.TabTextPaddingRight, 0))
                            .TextV("current");
                    }
                }
            }
        }

        float PanelHeight() =>
            ((items?.Count ?? 0) * style.Metrics.DropdownOptionHeight) + (style.Metrics.DropdownPopupPadding * 2);

        Vec2 PanelOffset(EntMut root)
        {
            var height = PanelHeight();
            var inset = style.Metrics.LooseSpacing;

            var x = Math.Min(anchor.PositionR.X, root.SizeR.X - anchor.SizeR.X - inset);
            var below = anchor.PositionR.Y + anchor.SizeR.Y + style.Metrics.DropdownPopupGap;
            if (below + height > root.SizeR.Y - inset)
                below = Math.Max(inset, anchor.PositionR.Y - height - style.Metrics.DropdownPopupGap);

            return (x, below);
        }

        Vec4 RowFill(int index)
        {
            if (index == highlightIndex)
                return style.Palette.Hover;
            if (index == selectedIndex)
                return style.Palette.ActiveSurface;
            return default;
        }
    }

    private void Pick(int index)
    {
        var pick = onPick;
        Close();
        pick?.Invoke(index);
    }
}
