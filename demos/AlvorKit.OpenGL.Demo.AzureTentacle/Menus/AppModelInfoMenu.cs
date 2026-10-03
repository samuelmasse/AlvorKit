namespace AlvorKit;

[App]
public class AppModelInfoMenu(
    BlendUi bl,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var panel)
            .Mutate(bl.S.PanelFitList);
        {
            Node(panel, out var header)
                .Mutate(bl.S.HeaderStrip)
                .SizeV((0, layout.RailHeaderHeight))
                .InnerSpacingV(bl.S.Metrics.LooseSpacing);
            {
                Node(header)
                    .Mutate(bl.S.EmphasisLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("Azure Tentacle");

                Node(header)
                    .Mutate(bl.S.MutedLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("GLB");
            }

            Node(panel, out var summary)
                .Mutate(bl.S.InsetPanelList)
                .InnerSpacingV(7f);
            {
                for (var index = 0; index < session.ModelStatCount; index++)
                {
                    Node(summary, out var row)
                        .Mutate(bl.S.HorizontalRow)
                        .SizeV((0, 22f))
                        .InnerSpacingV(8f);
                    {
                        Node(row)
                            .Mutate(bl.S.MutedCellLabel)
                            .SizeWeightTypeV(SizeWeightType.Self)
                            .SizeRelativeV((0, 1))
                            .SizeV((layout.ModelStatLabelWidth, 0))
                            .TextV(session.ModelStatLabelAt(index));

                        Node(row)
                            .Mutate(bl.S.EmphasisCellLabel)
                            .TextV(session.ModelStatValueAt(index));
                    }
                }
            }
        }
    }
}
