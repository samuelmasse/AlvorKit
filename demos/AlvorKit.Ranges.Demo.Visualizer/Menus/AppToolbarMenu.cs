namespace AlvorKit.Ranges.Demo.Visualizer;

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
        const float readoutWidth = 48f;

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
            ActionButton(controls, session.Playing ? "Pause" : "Play", session.TogglePlayback, session.Playing);
            ActionButton(controls, "Back", session.StepBackward);
            ActionButton(controls, "Step", session.StepForward);
            ActionButton(controls, "Reset", session.ResetScenario);
            Separator(controls);
            ActionButton(controls, "Prev", session.PreviousScenario);
            ActionButton(controls, "Next", session.NextScenario);
            ActionButton(controls, "Pack", session.JumpToPack);
            Separator(controls);
            ValueLabel(controls, "speed");
            SquareButton(controls, "-", session.Slower);
            ValueReadout(controls, () => text.Format("{0:0.##}x", session.Speed));
            SquareButton(controls, "+", session.Faster);
            Separator(controls);
            ActionButton(controls, "Labels", session.ToggleLabels, session.ShowLabels);
            ActionButton(controls, "Padding", session.TogglePadding, session.ShowPadding);
            Spacer(controls);
            ValueLabel(controls, "ui");
            SquareButton(controls, "-", uiScale.ScaleDown);
            ValueReadout(controls, () => text.Format("{0:F2}x", uiScale.Scale));
            SquareButton(controls, "+", uiScale.ScaleUp);
        }

        void ActionButton(EntMut parent, string label, Action action, bool active = false)
        {
            Node(parent)
                .Mutate(active ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(label)
                .OnPressF(action);
        }

        void SquareButton(EntMut parent, string label, Action action)
        {
            Node(parent)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(label)
                .OnPressF(action);
        }

        void ValueLabel(EntMut parent, string label)
        {
            Node(parent)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(label);
        }

        void ValueReadout(EntMut parent, Func<ReadOnlySpan<char>> value)
        {
            Node(parent)
                .Mutate(s.Text)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((readoutWidth, s.Metrics.ToolbarButtonHeight))
                .TextAlignmentV(Alignment.Center)
                .TextF(value);
        }

        void Separator(EntMut parent)
        {
            Node(parent)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((s.Metrics.Hairline, 0))
                .MarginV((s.Metrics.CompactSpacing, 2f, s.Metrics.CompactSpacing, 2f))
                .ColorV(s.Palette.Border);
        }

        static void Spacer(EntMut parent) =>
            Node(parent)
                .ColorV(default);
    }
}
