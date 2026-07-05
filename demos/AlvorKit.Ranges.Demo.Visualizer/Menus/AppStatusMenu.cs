namespace AlvorKit.Ranges.Demo.Visualizer;

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
            Item(status, true, () => text.Format("{0} FPS", metrics.FrameWindow.Ticks));
            Item(status, false, () => session.Playing ? "playing" : "paused");
            Item(status, false, () => text.Format("scenario {0}/{1}", session.ScenarioIndex + 1, session.ScenarioCount));
            Item(status, false, () => text.Format("step {0}/{1}", session.Runner.StepIndex, session.Runner.Scenario.Commands.Length));
            Item(status, false, () => text.Format("speed {0:0.##}x", session.Speed));
            Spacer(status);
            Item(status, false, () => text.Format("ui {0:F2}x", uiScale.Scale));
            Item(status, false, () => "space play  L labels  A padding  M/T modes");
        }

        void Item(EntMut parent, bool strong, Func<ReadOnlySpan<char>> value)
        {
            Node(parent)
                .Mutate(strong ? s.EmphasisText : s.MutedText)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeTextRelativeV((1, 0))
                .TextPaddingV((0, 0, s.Metrics.RightGlyphPadding, 0))
                .TextF(value);
        }

        static void Spacer(EntMut parent) =>
            Node(parent)
                .ColorV(default);
    }
}
