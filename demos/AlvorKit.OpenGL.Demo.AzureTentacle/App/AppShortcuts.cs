namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Maps keyboard shortcuts onto animation and camera commands.</summary>
[App]
public class AppShortcuts(
    RootKeyboard keyboard,
    AppSession session)
{
    public void Update()
    {
        if (session.CameraCaptured && keyboard.IsKeyPressed(Keys.Escape))
            session.ReleaseCamera();

        var previous = keyboard.IsKeyPressed(Keys.Left) || keyboard.IsKeyPressed(Keys.Up);
        var next = keyboard.IsKeyPressed(Keys.Right) || keyboard.IsKeyPressed(Keys.Down);

        if (previous && !next)
            session.SelectPreviousAnimation();
        else if (next && !previous)
            session.SelectNextAnimation();
    }
}
