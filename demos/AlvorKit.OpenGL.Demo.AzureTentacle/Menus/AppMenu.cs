namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppMenu(
    AppStyle s,
    AppLayout layout,
    AppSession session,
    AppModelInfoMenu modelInfoMenu,
    AppAnimationMenu animationMenu,
    AppStatusMenu statusMenu)
{
    public void Create(EntMut root)
    {
        Node(root, out var shell)
            .Mutate(s.OverlayBoard);
        {
            Node(shell)
                .SizeRelativeV((1, 1))
                .IsSelectableV(true)
                .CursorF(() => session.CameraCaptured ? CursorShape.Default : CursorShape.Hand)
                .OnClickF(() =>
                {
                    if (!session.CameraCaptured)
                        session.CaptureCamera();
                });

            statusMenu.Create(shell);

            Node(shell, out var sidebar)
                .Mutate(s.RailSurface)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((layout.RailWidth, 0));
            {
                modelInfoMenu.Create(sidebar);
                animationMenu.Create(sidebar);
            }
        }
    }
}
