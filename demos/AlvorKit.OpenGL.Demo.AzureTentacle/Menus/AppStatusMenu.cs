namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppStatusMenu(
    RootText text,
    RootMetrics metrics,
    AppStyle s,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var status)
            .Mutate(s.FloatingStatusStrip)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((layout.StatusInset, -layout.StatusInset))
            .SizeV((0, layout.StatusHeight))
            .InnerSpacingV(layout.StatusItemSpacing);
        {
            Node(status)
                .Mutate(s.EmphasisLabel)
                .SizeWeightTypeV(SizeWeightType.Self)
                .AlignmentV(Alignment.Vertical)
                .TextF(() => text.Format("{0} FPS", metrics.FrameWindow.Ticks));

            Node(status)
                .Mutate(s.MutedLabel)
                .SizeWeightTypeV(SizeWeightType.Self)
                .AlignmentV(Alignment.Vertical)
                .TextF(() => text.Format("Camera: {0}", session.CameraCaptured ? "mouse look" : "menu mode"));

            Node(status)
                .Mutate(s.EmphasisLabel)
                .SizeWeightTypeV(SizeWeightType.Self)
                .AlignmentV(Alignment.Vertical)
                .TextF(() => text.Format("Active: {0}", session.ActiveAnimationName));
        }
    }
}
