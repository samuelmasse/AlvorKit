namespace AlvorStarter.Menus;

/// <summary>Runs the starter scene, including raw GL, sprite batch drawing, and UI.</summary>
[App]
public class AppStarterState(
    RootBackbuffer backbuffer,
    RootKeyboard keyboard,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    AppGlTriangle triangle,
    AppSpriteScene spriteScene,
    AppMainMenu menu,
    AppStyle s) : State
{
    private EntMut menuNode;

    /// <summary>Initializes graphics resources and mounts the starter menu.</summary>
    public override void Load()
    {
        screen.Title = "Alvor Starter";
        screen.IsVSyncEnabled = true;
        triangle.Load();
        scripts.Add(uiScript);
        menuNode = Node(ui)
            .SizeRelativeV((1, 1));
        {
            menu.Create(menuNode);
        }
        screen.IsVisible = true;
    }

    /// <summary>Unmounts the UI script and menu node.</summary>
    public override void Unload()
    {
        if (menuNode != default)
            NodesRemove(ui, menuNode);

        scripts.Remove(uiScript);
    }

    /// <summary>Updates input and sprite-batch animation state.</summary>
    public override void Update(double delta)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            screen.Close();

        spriteScene.Update(delta);
    }

    /// <summary>Clears the frame and draws the raw GL triangle.</summary>
    public override void Render()
    {
        backbuffer.Clear(s.Palette.AppBackground);
        triangle.Render();
    }

    /// <summary>Draws the direct sprite-batch starter layer.</summary>
    public override void Draw() => spriteScene.Draw();
}
