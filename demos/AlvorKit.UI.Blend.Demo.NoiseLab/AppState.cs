namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Runs the Noise Lab: mounts the Blend UI and drives auto-regeneration.</summary>
[App]
public class AppState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    AppStyle s,
    AppSession session,
    AppMenu menu) : State
{
    private EntMut menuNode;
    private bool uiScriptAdded;

    public override void Load()
    {
        screen.Title = "AlvorKit Noise Lab";
        scripts.Add(uiScript);
        uiScriptAdded = true;
        menuNode = Node(ui)
            .SizeRelativeV((1, 1));
        {
            menu.Create(menuNode);
        }
        screen.IsVisible = true;
    }

    public override void Unload()
    {
        if (menuNode != default)
            NodesRemove(ui, menuNode);

        if (uiScriptAdded)
        {
            scripts.Remove(uiScript);
            uiScriptAdded = false;
        }

        session.Field.Dispose();
    }

    public override void Update(double delta) => session.Update();

    public override void Render() => backbuffer.Clear(s.Palette.AppBackground);
}
