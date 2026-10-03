namespace AlvorKit;

[App]
public class AppAnimationMenu(
    BlendUi bl,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var panel)
            .Mutate(bl.S.PanelFillList);
        {
            Node(panel, out var header)
                .Mutate(bl.S.HeaderStrip)
                .SizeV((0, layout.RailHeaderHeight))
                .InnerSpacingV(bl.S.Metrics.ToolbarSpacing);
            {
                Node(header)
                    .Mutate(bl.S.EmphasisLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("Animations");

                Node(header)
                    .Mutate(bl.S.ToolbarButton)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextV("Prev")
                    .OnClickF(session.SelectPreviousAnimation);

                Node(header)
                    .Mutate(bl.S.ActiveToolbarButton)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextV("Next")
                    .OnClickF(session.SelectNextAnimation);
            }

            Node(panel, out var list)
                .Mutate(bl.S.ListBody);
            {
                for (var index = 0; index < session.AnimationLineCount; index++)
                {
                    var animationIndex = index;
                    Node(list, out var row)
                        .Mutate(bl.S.SelectableListRow)
                        .ColorF(() => animationIndex == session.SelectedAnimationIndex
                            ? bl.S.Palette.ActiveSurface
                            : row.IsFocusedR || row.IsHoveredR ? bl.S.Palette.Hover : default)
                        .OnClickF(() => session.SelectAnimation(animationIndex));
                    {
                        Node(row)
                            .SizeWeightTypeV(SizeWeightType.Self)
                            .SizeRelativeV((0, 1))
                            .SizeV((layout.AnimationAccentWidth, 0))
                            .ColorF(() => animationIndex == session.SelectedAnimationIndex ? bl.S.Palette.Accent : default);

                        Node(row)
                            .Mutate(bl.S.CellLabel)
                            .TextV(session.AnimationLabelAt(animationIndex))
                            .TextColorF(() => animationIndex == session.SelectedAnimationIndex
                                ? bl.S.Palette.Text
                                : bl.S.Palette.MutedText);

                        Node(row)
                            .Mutate(bl.S.MutedCellLabel)
                            .SizeWeightTypeV(SizeWeightType.Self)
                            .SizeRelativeV((0, 1))
                            .SizeTextRelativeV((1, 0))
                            .TextV(session.AnimationDurationLabelAt(animationIndex))
                            .TextAlignmentV(Alignment.Right | Alignment.Vertical);
                    }
                }
            }
        }
    }
}
