namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppTimelinePanelMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppTimelineMenu timelineMenu)
{
    public void Create(EntMut root)
    {
        const float panelHeight = 78f;
        const float stripHeight = 18f;
        const float modeOffsetX = 164f;

        Node(root, out var panel)
            .Mutate(s.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, panelHeight));
        {
            Node(panel)
                .Mutate(s.Heading)
                .TextF(() => text.Format(
                    "timeline {0}/{1}",
                    session.Runner.StepIndex,
                    session.Runner.Scenario.Commands.Length))
                .AlignmentV(Alignment.Left | Alignment.Top);

            Node(panel)
                .Mutate(s.MutedLabel)
                .FontSizeV(s.FontSizeBody)
                .TextF(() => session.Runner.LastCommand.Label)
                .OffsetV((-s.FloatingTextInset, 0))
                .AlignmentV(Alignment.Right | Alignment.Top);

            Node(panel)
                .Mutate(s.Button)
                .SizeV((188f, s.ButtonHeight))
                .TextF(() => text.Format("timeline {0}", TimelineModeName(session.TimelineOverlayMode)))
                .OffsetV((modeOffsetX, 0))
                .AlignmentV(Alignment.Left | Alignment.Top)
                .OnPressF(session.NextTimelineOverlayMode);

            Node(panel, out var timelineSlot)
                .SizeRelativeV((1, 0))
                .SizeV((0, stripHeight))
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .OffsetV((0, -s.FloatingTextInset))
                .ColorV(s.PanelInsetColor);
            {
                timelineMenu.Create(timelineSlot);
            }
        }

        static string TimelineModeName(AppTimelineOverlayMode mode) => mode switch
        {
            AppTimelineOverlayMode.Commands => "commands",
            AppTimelineOverlayMode.Used => "used",
            AppTimelineOverlayMode.Efficiency => "efficiency",
            AppTimelineOverlayMode.FreeBlocks => "free blocks",
            AppTimelineOverlayMode.Events => "events",
            _ => "timeline",
        };
    }
}
