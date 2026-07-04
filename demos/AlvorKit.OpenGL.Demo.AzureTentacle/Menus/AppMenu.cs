namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppMenu(
    AppStyle style,
    AppSession session,
    AppModelInfoMenu modelInfoMenu,
    AppAnimationMenu animationMenu,
    AppStatusMenu statusMenu)
{
    public void Create(EntMut root)
    {
        root.Mutate(style.Root);

        Node(root)
            .Mutate(style.SceneArea)
            .IsSelectableV(true)
            .CursorF(() => session.CameraCaptured ? CursorShape.Default : CursorShape.Hand)
            .OnPressF(() =>
            {
                if (!session.CameraCaptured)
                    session.CaptureCamera();
            });

        Node(root, out var sidebar)
            .Mutate(style.Sidebar);
        {
            Node(sidebar)
                .Mutate(statusMenu.Create);

            Node(sidebar)
                .Mutate(modelInfoMenu.Create);

            Node(sidebar)
                .Mutate(animationMenu.Create);
        }
    }
}
