namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppHeaderMenu(
    RootText text,
    AppStyle style,
    AppSession session,
    AppUiScale uiScale)
{
    public void Create(EntMut root)
    {
        const float headerHeight = 112f;
        const float scenarioOffsetY = 32f;
        const float descriptionOffsetY = 52f;

        root.Mutate(style.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, headerHeight));

        Node(root)
            .Mutate(style.Title)
            .TextV("RangeAllocator visualizer")
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(root)
            .Mutate(style.ScenarioLink)
            .TextF(() => session.Runner.Scenario.Name)
            .OffsetV((0, scenarioOffsetY))
            .AlignmentV(Alignment.Left | Alignment.Top)
            .OnPressF(session.OpenScenarioPicker);

        Node(root)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => session.Runner.Scenario.Description)
            .OffsetV((0, descriptionOffsetY))
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(root)
            .Mutate(style.Label)
            .TextF(() => text.Format(
                "{0}  scenario {1}/{2}  step {3}/{4}  speed {5:0.##}x  ui {6:F2}x",
                session.Playing ? "playing" : "paused",
                session.ScenarioIndex + 1,
                session.ScenarioCount,
                session.Runner.StepIndex,
                session.Runner.Scenario.Commands.Length,
                session.Speed,
                uiScale.Scale))
            .OffsetV((-style.FloatingTextInset, 0))
            .AlignmentV(Alignment.Right | Alignment.Top);

        Node(root, out var toolbar)
            .Mutate(style.VerticalList)
            .InnerSpacingV(style.SpacingS)
            .AlignmentV(Alignment.Right | Alignment.Bottom);
        {
            Node(toolbar, out var playbackRow)
                .Mutate(style.HorizontalList)
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
                .Mutate(style.HorizontalList)
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

        void ToolbarLabel(EntMut parent, Func<ReadOnlySpan<char>> value)
        {
            const float toolbarLabelWidth = 78f;

            Node(parent)
                .Mutate(style.MutedLabel)
                .SizeV((toolbarLabelWidth, style.ButtonHeight))
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
                .Mutate(style.Button)
                .SizeV((width, style.ButtonHeight))
                .TextF(label)
                .OnPressF(action);
        }
    }
}
