namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the bottom status strip with frame, playback, scenario, and ui-scale readouts.</summary>
[App]
public class AppStatusMenu(
    RootText text,
    RootMetrics metrics,
    AppStyle s,
    AppSession session,
    AppUiScale uiScale)
{
    public void Create(EntMut root)
    {
        Node(root, out var status)
            .Mutate(s.StatusBar)
            .InnerLayoutV(InnerLayout.HorizontalList)
            .InnerSizingV(InnerSizing.HorizontalWeight)
            .InnerSpacingV(s.Metrics.StatusSpacing)
            .PaddingV(s.Metrics.StatusBarPadding);
        {
            Node(status)
                .Mutate(s.EmphasisText)
                .Mutate(StatusItem)
                .TextF(() => text.Format("{0} FPS", metrics.FrameWindow.Ticks));

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextF(() => session.Playing ? "playing" : "paused");

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextF(() => text.Format("scenario {0}/{1}", session.ScenarioIndex + 1, session.ScenarioCount));

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextF(() => text.Format("step {0}/{1}", session.Runner.StepIndex, session.Runner.Scenario.Commands.Length));

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextF(() => text.Format("speed {0:0.##}x", session.Speed));

            Node(status)
                .ColorV(default);

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextF(() => text.Format("ui {0:F2}x", uiScale.Scale));

            Node(status)
                .Mutate(s.MutedText)
                .Mutate(StatusItem)
                .TextV("space play  L labels  A padding  M/T modes");
        }

        void StatusItem(EntMut item)
        {
            item.Mutate()
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeTextRelativeV((1, 0))
                .TextPaddingV((0, 0, s.Metrics.RightGlyphPadding, 0));
        }
    }
}
