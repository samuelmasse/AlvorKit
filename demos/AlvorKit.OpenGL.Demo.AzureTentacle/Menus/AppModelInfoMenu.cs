namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppModelInfoMenu(
    AppStyle s,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var panel)
            .Mutate(s.PanelFitList);
        {
            Node(panel, out var header)
                .Mutate(s.HeaderStrip)
                .SizeV((0, layout.RailHeaderHeight))
                .InnerSpacingV(s.Metrics.LooseSpacing);
            {
                Node(header)
                    .Mutate(s.EmphasisLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("Azure Tentacle");

                Node(header)
                    .Mutate(s.MutedLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .AlignmentV(Alignment.Vertical)
                    .TextV("GLB");
            }

            Node(panel, out var summary)
                .Mutate(s.InsetPanelList)
                .InnerSpacingV(7f);
            {
                for (var index = 0; index < session.ModelStatCount; index++)
                {
                    Node(summary, out var row)
                        .Mutate(s.HorizontalRow)
                        .SizeV((0, 22f))
                        .InnerSpacingV(8f);
                    {
                        Node(row)
                            .Mutate(s.MutedCellLabel)
                            .SizeWeightTypeV(SizeWeightType.Self)
                            .SizeRelativeV((0, 1))
                            .SizeV((layout.ModelStatLabelWidth, 0))
                            .TextV(session.ModelStatLabelAt(index));

                        Node(row)
                            .Mutate(s.EmphasisCellLabel)
                            .TextV(session.ModelStatValueAt(index));
                    }
                }
            }
        }
    }
}
