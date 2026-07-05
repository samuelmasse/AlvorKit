namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Runs the interactive slow-motion range allocator visualizer.</summary>
[App]
public class AppState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    AppStyle s,
    AppSession session,
    AppShortcuts shortcuts,
    AppMenu visualizerMenu,
    AppScenarioPickerMenu scenarioPickerMenu,
    AppUiScaleMenu uiScaleMenu,
    AppTooltipMenu tooltipMenu) : State
{
    private EntMut dashboardNode;
    private EntMut pickerNode;
    private EntMut uiScaleNode;
    private EntMut tooltipNode;
    private bool uiScriptAdded;

    /// <summary>Initializes the first scenario, builds the UI, and shows the window.</summary>
    public override void Load()
    {
        screen.Title = "AlvorKit.Ranges.Demo.Visualizer";
        session.LoadInitial();
        scripts.Add(uiScript);
        uiScriptAdded = true;
        dashboardNode = Node(ui)
            .OrderValueV(0)
            .SizeRelativeV((1, 1));
        {
            visualizerMenu.Create(dashboardNode);
        }

        pickerNode = Node(ui)
            .OrderValueV(1)
            .SizeRelativeV((1, 1));
        {
            scenarioPickerMenu.Create(pickerNode);
        }

        uiScaleNode = Node(ui)
            .OrderValueV(2);
        {
            uiScaleMenu.Create(uiScaleNode);
        }

        tooltipNode = Node(ui)
            .OrderValueV(3)
            .SizeRelativeV((1, 1));
        {
            tooltipMenu.Create(tooltipNode);
        }
        screen.IsVisible = true;
    }

    /// <summary>Removes the visualizer UI and the UI script from the root loop.</summary>
    public override void Unload()
    {
        if (dashboardNode != default)
            NodesRemove(ui, dashboardNode);

        if (pickerNode != default)
            NodesRemove(ui, pickerNode);

        if (uiScaleNode != default)
            NodesRemove(ui, uiScaleNode);

        if (tooltipNode != default)
            NodesRemove(ui, tooltipNode);

        if (uiScriptAdded)
        {
            scripts.Remove(uiScript);
            uiScriptAdded = false;
        }
    }

    /// <summary>Advances the scripted allocator playback.</summary>
    public override void Update(double delta)
    {
        shortcuts.Update();
        session.Update(delta);
    }

    /// <summary>Clears the OpenGL backbuffer before the UI draws.</summary>
    public override void Render() => backbuffer.Clear(s.Palette.AppBackground);
}
