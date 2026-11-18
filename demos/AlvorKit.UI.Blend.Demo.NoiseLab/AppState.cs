namespace AlvorKit;

/// <summary>Runs the Noise Lab: mounts the Blend UI and drives auto-regeneration.</summary>
[App]
public class AppState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    AppBlendUi bl,
    AppSession session,
    AppMenu menu) : State
{
    private EntMut menuNode;

    public override void Load()
    {
        screen.Title = "AlvorKit Noise Lab";
        menuNode = Node(ui.Default.Root)
            .SizeRelativeV((1, 1));
        {
            menu.Create(menuNode);
        }
        screen.IsVisible = true;
    }

    /// <summary>Removes the UI and its script; the root scopes retain ownership of graph and GL resources.</summary>
    public override void Unload()
    {
        if (menuNode != default)
            NodesRemove(ui.Default.Root, menuNode);

        ui.Mouse.Release();
    }

    public override void Update(double delta) => session.Update();

    public override void Render() => backbuffer.Clear(bl.S.Palette.AppBackground);
}
