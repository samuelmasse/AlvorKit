namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppStatusMenu(
    RootText text,
    RootMetrics metrics,
    AppStyle style,
    AppSession session)
{
    public void Create(EntMut root)
    {
        root.Mutate(style.Panel);

        Node(root)
            .Mutate(style.Title)
            .TextF(() => text.Format(
                "{0} FPS  {1}",
                metrics.FrameWindow.Ticks,
                session.CameraCaptured ? "mouse look" : "menu mode"));

        Node(root)
            .Mutate(style.Label)
            .TextF(() => text.Format("Active: {0}", session.ActiveAnimationName));
    }
}
