namespace AlvorKit;

/// <summary>Builds the bottom timeline dock: overlay-mode tabs, last-call caption, and the scrubbable lane.</summary>
[App]
public class AppTimelinePanelMenu(
    RootText text,
    BlendUi bl,
    AppLayout layout,
    AppSession session,
    AppTimelineMenu timelineMenu)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        Node(root, out var dock)
            .Mutate(bl.S.TopRule)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, layout.TimelineDockHeight))
            .ColorV(bl.S.Palette.Panel)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .InnerSpacingV(0);
        {
            Node(dock, out var tabs)
                .Mutate(bl.S.TabStrip);
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

            var metrics = bl.S.Metrics;
            Node(dock, out var caption)
                .Mutate(bl.S.HorizontalRow)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeV((0, metrics.MetricRowHeight + metrics.CompactSpacing))
                .PaddingV((metrics.LooseSpacing, metrics.CompactSpacing, metrics.LooseSpacing, 0));
            {
                Node(caption)
                    .Mutate(bl.S.MutedCellLabel)
                    .TextF(() => text.Format("last call: {0}", session.Runner.LastCallText));

                Node(caption)
                    .Mutate(bl.S.MutedCellLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextF(() => text.Format("{0}/{1}", session.Runner.StepIndex, session.Runner.Scenario.Commands.Length));
            }

            Node(dock, out var laneSlot)
                .PaddingV((metrics.LooseSpacing, metrics.CompactSpacing, metrics.LooseSpacing, metrics.LooseSpacing));
            {
                timelineMenu.Create(laneSlot);
            }
        }

        void BuildTabs(EntMut tabRow)
        {
            Tab(
                tabRow,
                AppTimelineOverlayMode.Commands,
                "Commands",
                "commands overlay\none cell per scripted command, colored by kind\nshortcut: T cycles modes");
            Tab(
                tabRow,
                AppTimelineOverlayMode.Used,
                "Used",
                "used overlay\nstore usage after each command as a heat ramp");
            Tab(
                tabRow,
                AppTimelineOverlayMode.Efficiency,
                "Efficiency",
                "efficiency overlay\npayload versus reserved bytes over time");
            Tab(
                tabRow,
                AppTimelineOverlayMode.FreeBlocks,
                "Free Blocks",
                "free blocks overlay\nfree gap count after each command");
            Tab(
                tabRow,
                AppTimelineOverlayMode.Events,
                "Events",
                "events overlay\nmarks pack compactions and store resizes");

            Node(tabRow)
                .Mutate(bl.S.TabFiller);
        }

        void Tab(EntMut parent, AppTimelineOverlayMode mode, string label, string tooltip)
        {
            var active = session.TimelineOverlayMode == mode;
            Node(parent, out var tab)
                .Mutate(active ? bl.S.ActiveTab : bl.S.Tab)
                .SizeWeightTypeV(SizeWeightType.Self)
                .IsSelectableV(true)
                .IsFocusableV(true)
                .CursorF(() => CursorShape.Hand)
                .TextV(label)
                .TooltipV(tooltip)
                .OnPressF(() => session.SelectTimelineOverlayMode(mode));
            {
                if (active)
                    bl.S.ActiveTabAccent(tab);
            }
        }
    }
}
