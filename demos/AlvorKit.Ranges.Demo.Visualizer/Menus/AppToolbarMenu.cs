namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the playback/speed/toggle tool strip; rebuilds its controls when the session UI revision changes.</summary>
[App]
public class AppToolbarMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppUiScale uiScale)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        Node(root, out var toolbar)
            .Mutate(s.Toolbar)
            .PaddingV(s.Metrics.ToolbarPadding);
        {
            var lastRevision = pendingRevision;
            Node(toolbar, out var controls)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(s.Metrics.ToolbarSpacing)
                .OnUpdateF(() =>
                {
                    if (lastRevision == session.UiRevision)
                        return;

                    lastRevision = session.UiRevision;
                    NodesClear(controls);
                    BuildControls(controls);
                });
        }

        void BuildControls(EntMut controls)
        {
            Node(controls)
                .Mutate(session.Playing ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(session.Playing ? "Pause" : "Play")
                .TooltipV(session.Playing
                    ? "pause\nstops the scripted playback\nshortcut: space"
                    : "play\nresumes the scripted playback\nshortcut: space")
                .OnPressF(session.TogglePlayback);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Back")
                .TooltipV("step back\nrewinds one command and pauses\nshortcut: left arrow")
                .OnPressF(session.StepBackward);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Step")
                .TooltipV("step forward\nruns the next command and pauses\nshortcut: right arrow")
                .OnPressF(session.StepForward);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Reset")
                .TooltipV("reset scenario\nreloads the scenario at step 0\nshortcut: R")
                .OnPressF(session.ResetScenario);

            Separator(controls);
            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Prev")
                .TooltipV("previous scenario\nshortcut: shift+tab or wheel up")
                .OnPressF(session.PreviousScenario);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Next")
                .TooltipV("next scenario\nshortcut: tab or wheel down")
                .OnPressF(session.NextScenario);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Pack")
                .TooltipV("jump to pack\nplays forward to the next pack compaction\nshortcut: P")
                .OnPressF(session.JumpToPack);

            Separator(controls);
            SpeedGroup(controls);
            Separator(controls);
            Node(controls)
                .Mutate(session.ShowLabels ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Labels")
                .TooltipV("block labels\ntoggles size labels on memory blocks\nshortcut: L")
                .OnPressF(session.ToggleLabels);

            Node(controls)
                .Mutate(session.ShowPadding ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Padding")
                .TooltipV("padding overlay\ntoggles alignment padding in the strips\nshortcut: A")
                .OnPressF(session.TogglePadding);

            Node(controls)
                .ColorV(default);

            UiScaleGroup(controls);
        }

        void SpeedGroup(EntMut controls)
        {
            Node(controls)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("speed");

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("-")
                .TooltipV("slower\nhalves the playback speed\nshortcut: -")
                .OnPressF(session.Slower);

            Readout(controls, "playback speed\ncommands per second multiplier", () => text.Format("{0:0.##}x", session.Speed));

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("+")
                .TooltipV("faster\ndoubles the playback speed\nshortcut: +")
                .OnPressF(session.Faster);
        }

        void UiScaleGroup(EntMut controls)
        {
            Node(controls)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("ui");

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("-")
                .TooltipV("ui scale down\nshortcut: shift+-")
                .OnPressF(uiScale.ScaleDown);

            Readout(controls, "ui scale\nlogical to physical pixel multiplier", () => text.Format("{0:F2}x", uiScale.Scale));

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("+")
                .TooltipV("ui scale up\nshortcut: shift+=")
                .OnPressF(uiScale.ScaleUp);
        }

        void Readout(EntMut controls, string tooltip, Func<ReadOnlySpan<char>> value)
        {
            const float readoutWidth = 48f;

            Node(controls)
                .Mutate(s.Text)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((readoutWidth, s.Metrics.ToolbarButtonHeight))
                .TextAlignmentV(Alignment.Center)
                .TextF(value)
                .IsSelectableV(true)
                .TooltipV(tooltip);
        }

        void Separator(EntMut controls)
        {
            const float separatorInsetY = 2f;

            Node(controls)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((s.Metrics.Hairline, 0))
                .MarginV((s.Metrics.CompactSpacing, separatorInsetY, s.Metrics.CompactSpacing, separatorInsetY))
                .ColorV(s.Palette.Border);
        }
    }
}
