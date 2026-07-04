namespace AlvorKit.UI.Blend.Demo;

/// <summary>Runs the stripped editor shell as a reusable Blend style demo.</summary>
[App]
public class EditorShellState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    EditorShellStyle style,
    EditorShellMenu menu) : State
{
    private EntMut menuNode;
    private bool uiScriptAdded;

    public override void Load()
    {
        screen.Title = "AlvorKit Studio - Editor Shell";
        screen.Size = (3840u, 2160u);
        scripts.Add(uiScript);
        uiScriptAdded = true;
        menuNode = Node(ui).Mutate(menu.Create);
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
    }

    public override void Render() => backbuffer.Clear(style.Palette.AppBackground);
}
