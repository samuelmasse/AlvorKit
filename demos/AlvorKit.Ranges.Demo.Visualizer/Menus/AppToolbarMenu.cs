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
                .OnPressF(session.TogglePlayback);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Back")
                .OnPressF(session.StepBackward);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Step")
                .OnPressF(session.StepForward);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Reset")
                .OnPressF(session.ResetScenario);

            Separator(controls);
            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Prev")
                .OnPressF(session.PreviousScenario);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Next")
                .OnPressF(session.NextScenario);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Pack")
                .OnPressF(session.JumpToPack);

            Separator(controls);
            SpeedGroup(controls);
            Separator(controls);
            Node(controls)
                .Mutate(session.ShowLabels ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Labels")
                .OnPressF(session.ToggleLabels);

            Node(controls)
                .Mutate(session.ShowPadding ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Padding")
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
                .OnPressF(session.Slower);

            Readout(controls, () => text.Format("{0:0.##}x", session.Speed));

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("+")
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
                .OnPressF(uiScale.ScaleDown);

            Readout(controls, () => text.Format("{0:F2}x", uiScale.Scale));

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("+")
                .OnPressF(uiScale.ScaleUp);
        }

        void Readout(EntMut controls, Func<ReadOnlySpan<char>> value)
        {
            const float readoutWidth = 48f;

            Node(controls)
                .Mutate(s.Text)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((readoutWidth, s.Metrics.ToolbarButtonHeight))
                .TextAlignmentV(Alignment.Center)
                .TextF(value);
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
