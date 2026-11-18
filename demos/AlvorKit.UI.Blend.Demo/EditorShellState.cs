namespace AlvorKit;

/// <summary>Runs the stripped editor shell as a reusable Blend style demo.</summary>
[App]
public class EditorShellState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    RootBlendUi bl,
    EditorShellMenu menu) : State
{
    private EntMut menuNode;

    public override void Load()
    {
        screen.Title = "AlvorKit Studio - Editor Shell";
        screen.Size = (3840u, 2160u);
        menuNode = Node(ui.Default.Root)
            .SizeRelativeV((1, 1));
        {
            menu.Create(menuNode);
        }
        screen.IsVisible = true;
    }

    public override void Unload()
    {
        if (menuNode != default)
            NodesRemove(ui.Default.Root, menuNode);

        ui.Mouse.Release();
    }

    public override void Render() => backbuffer.Clear(bl.S.Palette.AppBackground);
}
