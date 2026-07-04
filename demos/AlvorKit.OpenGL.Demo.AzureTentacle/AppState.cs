namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Runs the animated azure tentacle GLB demo with an AlvorKit UI overlay.</summary>
[App]
public class AppState(
    RootGl gl,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    AppSession session,
    AppShortcuts shortcuts,
    AppSceneRenderer sceneRenderer,
    AppMenu menu) : State
{
    private EntMut menuNode;
    private bool uiScriptAdded;

    /// <summary>Loads the model, mounts the menu UI, and shows the window.</summary>
    public override void Load()
    {
        screen.Title = "AlvorKit.OpenGL.Demo.AzureTentacle";
        gl.GetString(GlStringName.Version, out var version);
        gl.GetString(GlStringName.ShadingLanguageVersion, out var glsl);
        Console.WriteLine($"OpenGL {version} (GLSL {glsl}) - close the window to exit.");
        Console.WriteLine("Mouse look, WASD to move, Space up, Control down, Shift faster, Escape releases the cursor.");

        session.Load();
        scripts.Add(uiScript);
        uiScriptAdded = true;
        menuNode = Node(ui)
            .OrderValueV(0)
            .Mutate(menu.Create);
        screen.IsVisible = true;
    }

    /// <summary>Removes the menu UI and releases demo-owned resources.</summary>
    public override void Unload()
    {
        if (menuNode != default)
            NodesRemove(ui, menuNode);

        if (uiScriptAdded)
        {
            scripts.Remove(uiScript);
            uiScriptAdded = false;
        }

        session.Dispose();
    }

    /// <summary>Updates keyboard shortcuts, camera motion, and GLB animation playback.</summary>
    public override void Update(double delta)
    {
        shortcuts.Update();
        session.Update(delta);
    }

    /// <summary>Draws the three-dimensional model before the root UI script draws the menu overlay.</summary>
    public override void Render() => sceneRenderer.Render();
}
