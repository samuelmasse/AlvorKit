namespace AlvorKit;

/// <summary>Builds independent popup layers; mount each above its fields and below any tooltip layer.</summary>
public class BlendDropdownMenu(Keyboard keyboard, BlendStyle s)
{
    /// <summary>Mounts a popup and returns the handle that its dropdown fields share.</summary>
    public BlendDropdownHandle Create(EntMut root)
    {
        Node(root, out var layer);
        Node(layer, out var panel);
        var popup = new BlendDropdownHandle(panel);
        var builtRevision = 0L;

        layer.Mutate()
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => !popup.IsOpen)
            .IsSelectableV(true)
            .IsSilentFocusableV(true)
            .OnPressF(popup.Close)
            .OnUpdateF(() =>
            {
                if (!popup.IsOpen || popup.SkipOpeningUpdate())
                    return;

                if (keyboard.IsKeyPressed(Keys.Escape))
                {
                    popup.Close();
                    return;
                }

                if (keyboard.IsKeyPressedRepeated(Keys.Up))
                    popup.MoveHighlight(-1);

                if (keyboard.IsKeyPressedRepeated(Keys.Down))
                    popup.MoveHighlight(1);

                if (keyboard.IsKeyPressed(Keys.Enter))
                    popup.Pick(popup.HighlightIndex);
            });
        {
            panel.Mutate()
                .IsFloatingV(true)
                .SizeRelativeV((0, 0))
                .SizeF(() => (popup.Anchor.SizeR.X, PanelHeight()))
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSpacingV(0)
                .ColorV(s.Palette.Raised)
                .PaddingV((
                    s.Metrics.DropdownPopupPadding,
                    s.Metrics.DropdownPopupPadding,
                    s.Metrics.DropdownPopupPadding,
                    s.Metrics.DropdownPopupPadding))
                .OffsetF(() =>
                {
                    var height = PanelHeight();
                    var inset = s.Metrics.LooseSpacing;
                    var anchor = popup.Anchor;
                    var position = anchor.PositionR - root.PositionR;
                    var x = Math.Min(position.X, root.SizeR.X - anchor.SizeR.X - inset);
                    var below = position.Y + anchor.SizeR.Y + s.Metrics.DropdownPopupGap;

                    if (below + height > root.SizeR.Y - inset)
                        below = Math.Max(inset, position.Y - height - s.Metrics.DropdownPopupGap);

                    return (x, below);
                })
                .Mutate(s.StrongBorder)
                .OnUpdateF(() =>
                {
                    if (builtRevision == popup.Revision || !popup.IsOpen)
                        return;

                    builtRevision = popup.Revision;
                    NodesClear(panel);

                    for (var i = 0; i < popup.Items.Count; i++)
                    {
                        var index = i;
                        var item = popup.Items[i];

                        Node(panel, out var row)
                            .Mutate(s.Board)
                            .SizeRelativeV((1, 0))
                            .SizeV((0, s.Metrics.DropdownOptionHeight))
                            .IsSelectableV(true)
                            .CursorF(() => CursorShape.Hand)
                            .ColorF(() => index == popup.HighlightIndex
                                ? s.Palette.Hover
                                : index == popup.SelectedIndex ? s.Palette.ActiveSurface : default)
                            .OnPressF(() => popup.Pick(index))
                            .OnUpdateF(() =>
                            {
                                if (row.IsHoveredR)
                                    popup.Highlight(index);
                            });
                        {
                            var textLeft = s.Metrics.TabTextPaddingLeft;

                            if (item.Swatch.W > 0)
                            {
                                Node(row)
                                    .Mutate(s.Swatch)
                                    .IsFloatingV(true)
                                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                                    .OffsetV((textLeft, 0))
                                    .ColorV(item.Swatch);
                                textLeft += s.Metrics.SwatchWidth + s.Metrics.CompactSpacing;
                            }

                            Node(row)
                                .Mutate(s.CellLabel)
                                .IsFloatingV(true)
                                .TextPaddingV((textLeft, 0, s.Metrics.TabTextPaddingRight, 0))
                                .TextV(item.Text);

                            if (index == popup.SelectedIndex)
                            {
                                Node(row)
                                    .IsFloatingV(true)
                                    .SizeRelativeV((0, 1))
                                    .SizeV((3, 0))
                                    .ColorV(s.Palette.Accent);

                                Node(row)
                                    .Mutate(s.MutedCellLabel)
                                    .IsFloatingV(true)
                                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                                    .TextPaddingV((0, 0, s.Metrics.TabTextPaddingRight, 0))
                                    .TextV("current");
                            }
                        }
                    }
                });
        }

        return popup;

        float PanelHeight() => (popup.Items.Count * s.Metrics.DropdownOptionHeight) + (s.Metrics.DropdownPopupPadding * 2);
    }
}
