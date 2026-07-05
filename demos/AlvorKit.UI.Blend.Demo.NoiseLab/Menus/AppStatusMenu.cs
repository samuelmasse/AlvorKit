namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Builds the bottom status strip with generation and session readouts.</summary>
[App]
public class AppStatusMenu(
    RootText text,
    RootMetrics metrics,
    AppStyle s,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var status)
            .Mutate(s.StatusBar)
            .PaddingV(s.Metrics.StatusBarPadding);
        {
            Node(status, out var items)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(s.Metrics.StatusSpacing);
            {
                Item(items, true, () => text.Format("{0} FPS", metrics.FrameWindow.Ticks));
                Item(items, false, () => session.Auto ? "auto" : session.Dirty ? "manual · dirty" : "manual");
                Item(items, false, () => text.Format(
                    "{0}({1})",
                    session.Field.Fractals[session.Field.FractalIndex].Text,
                    session.Field.Sources[session.Field.SourceIndex].Text));
                Item(items, false, () => text.Format("seed {0}", session.Seed));
                Item(items, false, () => text.Format("float32 · {0:N0} samples", session.Field.Width * session.Field.Height));

                Node(items);

                Item(items, false, () => "AlvorKit.UI.Blend");
            }
        }

        void Item(EntMut items, bool emphasis, Func<ReadOnlySpan<char>> value)
        {
            Node(items)
                .Mutate(emphasis ? s.EmphasisLabel : s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextF(value);
        }
    }
}
