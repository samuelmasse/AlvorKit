namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppAnimationMenu(
    AppStyle s,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var panel)
            .Mutate(s.PanelFillList);
        {
            Node(panel, out var header)
                .Mutate(s.HeaderStrip)
                .SizeV((0, layout.RailHeaderHeight))
                .InnerSpacingV(s.Metrics.ToolbarSpacing);
            {
                Node(header)
                    .Mutate(s.EmphasisLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("Animations");

                Node(header)
                    .Mutate(s.ToolbarButton)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextV("Prev")
                    .OnClickF(session.SelectPreviousAnimation);

                Node(header)
                    .Mutate(s.ActiveToolbarButton)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextV("Next")
                    .OnClickF(session.SelectNextAnimation);
            }

            Node(panel, out var list)
                .Mutate(s.ListBody);
            {
                for (var index = 0; index < session.AnimationLineCount; index++)
                {
                    var animationIndex = index;
                    Node(list, out var row)
                        .Mutate(s.SelectableListRow)
                        .ColorF(() => animationIndex == session.SelectedAnimationIndex
                            ? s.Palette.ActiveSurface
                            : row.IsFocusedR || row.IsHoveredR ? s.Palette.Hover : default)
                        .OnClickF(() => session.SelectAnimation(animationIndex));
                    {
                        Node(row)
                            .SizeWeightTypeV(SizeWeightType.Self)
                            .SizeRelativeV((0, 1))
                            .SizeV((layout.AnimationAccentWidth, 0))
                            .ColorF(() => animationIndex == session.SelectedAnimationIndex ? s.Palette.Accent : default);

                        Node(row)
                            .Mutate(s.CellLabel)
                            .TextV(session.AnimationLabelAt(animationIndex))
                            .TextColorF(() => animationIndex == session.SelectedAnimationIndex
                                ? s.Palette.Text
                                : s.Palette.MutedText);

                        Node(row)
                            .Mutate(s.MutedCellLabel)
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
