namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppHeaderMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppUiScale uiScale,
    RootMetrics metrics)
{
    public void Create(EntMut root)
    {
        const float headerHeight = 112f;
        const float scenarioOffsetY = 32f;
        const float descriptionOffsetY = 52f;

        Node(root, out var header)
            .Mutate(s.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, headerHeight));
        {
            Node(header)
                .Mutate(s.Title)
                .TextV("RangeAllocator visualizer")
                .AlignmentV(Alignment.Left | Alignment.Top);

            Node(header)
                .Mutate(s.ScenarioLink)
                .TextF(() => session.Runner.Scenario.Name)
                .OffsetV((0, scenarioOffsetY))
                .AlignmentV(Alignment.Left | Alignment.Top)
                .OnPressF(session.OpenScenarioPicker);

            Node(header)
                .Mutate(s.MutedLabel)
                .FontSizeV(s.FontSizeBody)
                .TextF(() => session.Runner.Scenario.Description)
                .OffsetV((0, descriptionOffsetY))
                .AlignmentV(Alignment.Left | Alignment.Top);

            Node(header)
                .Mutate(s.Label)
                .TextF(() => text.Format(
                    "{0} FPS  {1}  scenario {2}/{3}  step {4}/{5}  speed {6:0.##}x  ui {7:F2}x",
                    metrics.FrameWindow.Ticks,
                    session.Playing ? "playing" : "paused",
                    session.ScenarioIndex + 1,
                    session.ScenarioCount,
                    session.Runner.StepIndex,
                    session.Runner.Scenario.Commands.Length,
                    session.Speed,
                    uiScale.Scale))
                .OffsetV((-s.FloatingTextInset, 0))
                .AlignmentV(Alignment.Right | Alignment.Top);

            Node(header, out var toolbar)
                .Mutate(s.VerticalList)
                .InnerSpacingV(s.SpacingS)
                .AlignmentV(Alignment.Right | Alignment.Bottom);
            {
                Node(toolbar, out var playbackRow)
                    .Mutate(s.HorizontalList)
                    .AlignmentV(Alignment.Right);
                {
                    const float playButtonWidth = 84f;
                    const float backButtonWidth = 72f;
                    const float stepButtonWidth = 72f;
                    const float resetButtonWidth = 82f;
                    const float previousButtonWidth = 86f;
                    const float nextButtonWidth = 78f;
                    const float packButtonWidth = 96f;

                    ButtonF(playbackRow, playButtonWidth, () => session.Playing ? "Pause" : "Play", session.TogglePlayback);
                    Button(playbackRow, backButtonWidth, "Back", session.StepBackward);
                    Button(playbackRow, stepButtonWidth, "Step", session.StepForward);
                    Button(playbackRow, resetButtonWidth, "Reset", session.ResetScenario);
                    Button(playbackRow, previousButtonWidth, "Prev", session.PreviousScenario);
                    Button(playbackRow, nextButtonWidth, "Next", session.NextScenario);
                    Button(playbackRow, packButtonWidth, "Pack", session.JumpToPack);
                }

                Node(toolbar, out var optionRow)
                    .Mutate(s.HorizontalList)
                    .AlignmentV(Alignment.Right);
                {
                    const float smallButtonWidth = 42f;
                    const float labelToggleWidth = 112f;
                    const float paddingToggleWidth = 122f;

                    ToolbarLabel(optionRow, () => text.Format("speed {0:0.##}x", session.Speed));
                    Button(optionRow, smallButtonWidth, "-", session.Slower);
                    Button(optionRow, smallButtonWidth, "+", session.Faster);
                    ToolbarLabel(optionRow, () => text.Format("ui {0:F2}x", uiScale.Scale));
                    Button(optionRow, smallButtonWidth, "-", uiScale.ScaleDown);
                    Button(optionRow, smallButtonWidth, "+", uiScale.ScaleUp);
                    ButtonF(optionRow, labelToggleWidth, () => session.ShowLabels ? "Labels on" : "Labels off", session.ToggleLabels);
                    ButtonF(optionRow, paddingToggleWidth, () => session.ShowPadding ? "Padding on" : "Padding off", session.TogglePadding);
                }
            }
        }

        void ToolbarLabel(EntMut parent, Func<ReadOnlySpan<char>> value)
        {
            const float toolbarLabelWidth = 78f;

            Node(parent)
                .Mutate(s.MutedLabel)
                .SizeV((toolbarLabelWidth, s.ButtonHeight))
                .SizeTextRelativeV((0, 0))
                .TextAlignmentV(Alignment.Center)
                .TextF(value)
                .AlignmentV(Alignment.Vertical);
        }

        void Button(EntMut parent, float width, string label, Action action) =>
            ButtonF(parent, width, () => label, action);

        void ButtonF(EntMut parent, float width, Func<ReadOnlySpan<char>> label, Action action)
        {
            Node(parent)
                .Mutate(s.Button)
                .SizeV((width, s.ButtonHeight))
                .TextF(label)
                .OnPressF(action);
        }
    }
}
