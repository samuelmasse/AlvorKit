namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the bottom timeline dock: overlay-mode tabs, last-call caption, and the scrubbable lane.</summary>
[App]
public class AppTimelinePanelMenu(
    RootText text,
    AppStyle s,
    AppLayout layout,
    AppSession session,
    AppTimelineMenu timelineMenu)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        Node(root, out var dock)
            .Mutate(s.TopRule)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, layout.TimelineDockHeight))
            .ColorV(s.Palette.Panel)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .InnerSpacingV(0);
        {
            Node(dock, out var tabs)
                .Mutate(s.TabStrip);
            {
                var lastRevision = pendingRevision;
                Node(tabs, out var tabRow)
                    .SizeRelativeV((1, 1))
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(0)
                    .OnUpdateF(() =>
                    {
                        if (lastRevision == session.UiRevision)
                            return;

                        lastRevision = session.UiRevision;
                        NodesClear(tabRow);
                        BuildTabs(tabRow);
                    });
            }

            Node(dock, out var caption)
                .Mutate(s.HorizontalRow)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeV((0, s.Metrics.MetricRowHeight + s.Metrics.CompactSpacing))
                .PaddingV((s.Metrics.LooseSpacing, s.Metrics.CompactSpacing, s.Metrics.LooseSpacing, 0));
            {
                Node(caption)
                    .Mutate(s.MutedCellLabel)
                    .TextF(() => text.Format("last call: {0}", session.Runner.LastCallText));

                Node(caption)
                    .Mutate(s.MutedCellLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextF(() => text.Format("{0}/{1}", session.Runner.StepIndex, session.Runner.Scenario.Commands.Length));
            }

            Node(dock, out var laneSlot)
                .PaddingV((s.Metrics.LooseSpacing, s.Metrics.CompactSpacing, s.Metrics.LooseSpacing, s.Metrics.LooseSpacing));
            {
                timelineMenu.Create(laneSlot);
            }
        }

        void BuildTabs(EntMut tabRow)
        {
            Tab(tabRow, AppTimelineOverlayMode.Commands, "Commands");
            Tab(tabRow, AppTimelineOverlayMode.Used, "Used");
            Tab(tabRow, AppTimelineOverlayMode.Efficiency, "Efficiency");
            Tab(tabRow, AppTimelineOverlayMode.FreeBlocks, "Free Blocks");
            Tab(tabRow, AppTimelineOverlayMode.Events, "Events");

            Node(tabRow)
                .Mutate(s.TabFiller);
        }

        void Tab(EntMut parent, AppTimelineOverlayMode mode, string label)
        {
            var active = session.TimelineOverlayMode == mode;
            Node(parent, out var tab)
                .Mutate(active ? s.ActiveTab : s.Tab)
                .SizeWeightTypeV(SizeWeightType.Self)
                .IsSelectableV(true)
                .IsFocusableV(true)
                .CursorF(() => CursorShape.Hand)
                .TextV(label)
                .OnPressF(() => session.SelectTimelineOverlayMode(mode));
            {
                if (active)
                    s.ActiveTabAccent(tab);
            }
        }
    }
}
