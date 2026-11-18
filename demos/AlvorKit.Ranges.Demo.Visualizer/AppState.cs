namespace AlvorKit;

/// <summary>Runs the interactive slow-motion range allocator visualizer.</summary>
[App]
public class AppState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    RootBlendUi bl,
    AppSession session,
    AppShortcuts shortcuts,
    AppMenu visualizerMenu,
    AppScenarioPickerMenu scenarioPickerMenu,
    AppUiScaleMenu uiScaleMenu) : State
{
    private EntMut dashboardNode;
    private EntMut pickerNode;
    private EntMut uiScaleNode;
    private EntMut tooltipNode;

    /// <summary>Initializes the first scenario, builds the UI, and shows the window.</summary>
    public override void Load()
    {
        screen.Title = "AlvorKit.Ranges.Demo.Visualizer";
        session.LoadInitial();
        dashboardNode = Node(ui.Default.Root)
            .OrderValueV(0)
            .SizeRelativeV((1, 1));
        {
            visualizerMenu.Create(dashboardNode);
        }

        pickerNode = Node(ui.Default.Root)
            .OrderValueV(1)
            .SizeRelativeV((1, 1));
        {
            scenarioPickerMenu.Create(pickerNode);
        }

        uiScaleNode = Node(ui.Default.Root)
            .OrderValueV(2);
        {
            uiScaleMenu.Create(uiScaleNode);
        }

        tooltipNode = Node(ui.Default.Root)
            .OrderValueV(3)
            .SizeRelativeV((1, 1));
        {
            bl.Tooltip.Create(tooltipNode);
        }
        screen.IsVisible = true;
    }

    /// <summary>Unmounts the visualizer UI and releases pointer input.</summary>
    public override void Unload()
    {
        if (dashboardNode != default)
            NodesRemove(ui.Default.Root, dashboardNode);

        if (pickerNode != default)
            NodesRemove(ui.Default.Root, pickerNode);

        if (uiScaleNode != default)
            NodesRemove(ui.Default.Root, uiScaleNode);

        if (tooltipNode != default)
            NodesRemove(ui.Default.Root, tooltipNode);

        ui.Mouse.Release();
    }

    /// <summary>Advances the scripted allocator playback.</summary>
    public override void Update(double delta)
    {
        shortcuts.Update();
        session.Update(delta);
    }

    /// <summary>Clears the OpenGL backbuffer before the UI draws.</summary>
    public override void Render() => backbuffer.Clear(bl.S.Palette.AppBackground);
}
