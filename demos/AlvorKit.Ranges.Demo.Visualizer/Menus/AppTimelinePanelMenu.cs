namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppTimelinePanelMenu(
    RootText text,
    AppStyle style,
    AppSession session,
    AppTimelineMenu timelineMenu)
{
    public void Create(EntMut root)
    {
        const float panelHeight = 78f;
        const float stripHeight = 18f;

        root.Mutate(style.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, panelHeight));

        Node(root)
            .Mutate(style.Heading)
            .TextF(() => text.Format(
                "timeline {0}/{1}",
                session.Runner.StepIndex,
                session.Runner.Scenario.Commands.Length))
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(root)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => session.Runner.LastCommand.Label)
            .OffsetV((-style.FloatingTextInset, 0))
            .AlignmentV(Alignment.Right | Alignment.Top);

        Node(root)
            .SizeRelativeV((1, 0))
            .SizeV((0, stripHeight))
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((0, -style.FloatingTextInset))
            .ColorV(style.PanelInsetColor)
            .Mutate(timelineMenu.Create);
    }
}
