namespace AlvorKit.UI.Blend.Demo;

/// <summary>Runs the stripped editor shell as a reusable Blend style demo.</summary>
[App]
public class EditorShellState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    EditorShellStyle s,
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
    }

    public override void Render() => backbuffer.Clear(s.Palette.AppBackground);
}
