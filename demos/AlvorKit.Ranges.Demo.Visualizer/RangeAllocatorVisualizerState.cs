namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Runs the interactive slow-motion range allocator visualizer.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScale rootScale,
    RootUiScale uiScale,
    RootKeyboard keyboard,
    RootMouse mouse,
    RootScripts scripts,
    RootUi ui,
    RootUiScript uiScript,
    RangeAllocatorVisualizerStyle style,
    RangeAllocatorVisualizerView view,
    RangeAllocatorVisualizerTooltipView tooltipView) : State
{
    private const int MinSpeedPower = -2;
    private const int MaxSpeedPower = 6;
    private const int DefaultSpeedPower = 3;

    private readonly AllocatorScenario[] scenarios = AllocatorScenario.CreateAll();
    private readonly AllocatorScenarioRunner runner = new();
    private EntMut dashboardNode;
    private EntMut pickerNode;
    private EntMut tooltipNode;
    private bool uiScriptAdded;
    private int scenarioIndex;
    private float stepAccumulator;
    private float animationPhase = 1f;
    private int speedPower = DefaultSpeedPower;
    private bool playing = true;
    private bool showLabels = true;
    private bool showPadding = true;
    private bool scenarioPickerOpen;
    private int visualRevision;

    /// <summary>Gets the scenario runner that backs the live display.</summary>
    internal AllocatorScenarioRunner Runner => runner;

    /// <summary>Gets the active scenario list index.</summary>
    internal int ScenarioIndex => scenarioIndex;

    /// <summary>Gets the total number of built-in scenarios.</summary>
    internal int ScenarioCount => scenarios.Length;

    /// <summary>Gets the current transition animation phase.</summary>
    internal float AnimationPhase => animationPhase;

    /// <summary>Gets the automatic playback speed.</summary>
    internal float Speed => SpeedFromPower(speedPower);

    /// <summary>Gets whether the scripted scenario advances automatically.</summary>
    internal bool Playing => playing;

    /// <summary>Gets whether allocation byte labels are drawn inside ranges when they fit.</summary>
    internal bool ShowLabels => showLabels;

    /// <summary>Gets whether alignment padding is drawn inside live ranges.</summary>
    internal bool ShowPadding => showPadding;

    /// <summary>Gets whether the scenario picker modal is open.</summary>
    internal bool ScenarioPickerOpen => scenarioPickerOpen;

    /// <summary>Gets a revision that changes whenever the allocator snapshot changes.</summary>
    internal int VisualRevision => visualRevision;

    /// <summary>Initializes the first scenario, builds the UI, and shows the window.</summary>
    public override void Load()
    {
        screen.Title = "AlvorKit.Ranges.Demo.Visualizer";
        LoadScenario(0);
        scripts.Add(uiScript);
        uiScriptAdded = true;
        dashboardNode = Node(ui)
            .OrderValueV(0)
            .Mutate(root => view.Create(root, this));
        pickerNode = Node(ui)
            .OrderValueV(1)
            .Mutate(root => view.CreateScenarioPicker(root, this));
        tooltipNode = Node(ui)
            .OrderValueV(2)
            .Mutate(tooltipView.Create);
        screen.IsVisible = true;
    }

    /// <summary>Removes the visualizer UI and the UI script from the root loop.</summary>
    public override void Unload()
    {
        if (dashboardNode != default)
            NodesRemove(ui, dashboardNode);

        if (pickerNode != default)
            NodesRemove(ui, pickerNode);

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
        UpdateShortcuts();

        animationPhase = Math.Min(1f, animationPhase + (float)delta * 2.8f);
        if (playing)
            AdvancePlayback((float)delta);
    }

    /// <summary>Clears the OpenGL backbuffer before the UI draws.</summary>
    public override void Render() => backbuffer.Clear(style.BackgroundColor);

    /// <summary>Toggles automatic playback.</summary>
    internal void TogglePlayback() => playing = !playing;

    /// <summary>Toggles labels inside allocation ranges.</summary>
    internal void ToggleLabels() => showLabels = !showLabels;

    /// <summary>Toggles alignment padding inside live ranges.</summary>
    internal void TogglePadding() => showPadding = !showPadding;

    /// <summary>Increases automatic playback speed.</summary>
    internal void Faster() => speedPower = Math.Min(MaxSpeedPower, speedPower + 1);

    /// <summary>Decreases automatic playback speed.</summary>
    internal void Slower() => speedPower = Math.Max(MinSpeedPower, speedPower - 1);

    /// <summary>Increases root UI scale.</summary>
    internal void UiScaleUp()
    {
        rootScale.Numerator = Math.Min(rootScale.Denominator * 4, rootScale.Numerator + 1);
        uiScale.Scale = rootScale.Scale;
    }

    /// <summary>Decreases root UI scale.</summary>
    internal void UiScaleDown()
    {
        rootScale.Numerator = Math.Max(1, rootScale.Numerator - 1);
        uiScale.Scale = rootScale.Scale;
    }

    /// <summary>Loads the next built-in scenario.</summary>
    internal void NextScenario() => LoadScenario(scenarioIndex + 1);

    /// <summary>Loads the previous built-in scenario.</summary>
    internal void PreviousScenario() => LoadScenario(scenarioIndex - 1);

    /// <summary>Gets a scenario by list index.</summary>
    internal AllocatorScenario ScenarioAt(int index) => scenarios[index];

    /// <summary>Opens the centered scenario picker modal.</summary>
    internal void OpenScenarioPicker() => scenarioPickerOpen = true;

    /// <summary>Closes the centered scenario picker modal.</summary>
    internal void CloseScenarioPicker() => scenarioPickerOpen = false;

    /// <summary>Loads a scenario from the picker and closes the modal.</summary>
    internal void SelectScenario(int index)
    {
        LoadScenario(index);
        CloseScenarioPicker();
    }

    /// <summary>Reloads the current scenario from its initial allocator state.</summary>
    internal void ResetScenario() => LoadScenario(scenarioIndex);

    /// <summary>Replays the current scenario to an exact timeline step and pauses playback.</summary>
    internal void JumpToStep(int step)
    {
        playing = false;
        var targetStep = Math.Clamp(step, 0, runner.Scenario.Commands.Length);
        if (targetStep == runner.StepIndex)
            return;

        runner.ReplayTo(targetStep);
        stepAccumulator = 0f;
        animationPhase = 0f;
        visualRevision++;
    }

    /// <summary>Applies one forward step and pauses playback.</summary>
    internal void StepForward()
    {
        playing = false;
        if (runner.StepForward())
        {
            animationPhase = 0f;
            visualRevision++;
        }
    }

    /// <summary>Replays one step backward and pauses playback.</summary>
    internal void StepBackward()
    {
        playing = false;
        if (runner.StepBackward())
        {
            animationPhase = 1f;
            visualRevision++;
        }
    }

    /// <summary>Jumps to the next pack operation and pauses playback on the resulting packed frame.</summary>
    internal void JumpToPack()
    {
        playing = false;
        if (runner.JumpToPack())
        {
            animationPhase = 0f;
            visualRevision++;
        }
    }

    /// <summary>Gets the current root UI scale value.</summary>
    internal float UiScale => uiScale.Scale;

    /// <summary>Loads a scenario by wrapping around the built-in scenario list.</summary>
    private void LoadScenario(int index)
    {
        scenarioIndex = ((index % scenarios.Length) + scenarios.Length) % scenarios.Length;
        runner.Load(scenarios[scenarioIndex]);
        stepAccumulator = 0f;
        animationPhase = 1f;
        visualRevision++;
    }

    /// <summary>Advances automatic playback by elapsed time.</summary>
    private void AdvancePlayback(float delta)
    {
        if (runner.Scenario.Commands.Length == 0)
            return;

        stepAccumulator += delta * Speed;
        while (stepAccumulator >= 1f)
        {
            stepAccumulator -= 1f;
            if (!runner.StepForward())
            {
                runner.ReplayTo(0);
                animationPhase = 1f;
                visualRevision++;
                continue;
            }

            animationPhase = 0f;
            visualRevision++;
        }
    }

    private void UpdateShortcuts()
    {
        var shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        var wheel = mouse.Wheel.Y;

        if (wheel > 0)
            PreviousScenario();
        else if (wheel < 0)
            NextScenario();

        if (scenarioPickerOpen && keyboard.IsKeyPressed(Keys.Escape))
            CloseScenarioPicker();

        if (keyboard.IsKeyPressed(Keys.Space))
            TogglePlayback();
        if (keyboard.IsKeyPressed(Keys.L))
            ToggleLabels();
        if (keyboard.IsKeyPressed(Keys.A))
            TogglePadding();
        if (keyboard.IsKeyPressedRepeated(Keys.Right))
            StepForward();
        if (keyboard.IsKeyPressedRepeated(Keys.Left))
            StepBackward();
        if (keyboard.IsKeyPressed(Keys.R))
            ResetScenario();
        if (keyboard.IsKeyPressed(Keys.P))
            JumpToPack();
        if (keyboard.IsKeyPressed(Keys.Tab))
        {
            if (shift)
                PreviousScenario();
            else
                NextScenario();
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Equal))
        {
            if (shift)
                UiScaleUp();
            else
                Faster();
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Minus))
        {
            if (shift)
                UiScaleDown();
            else
                Slower();
        }
    }

    private static float SpeedFromPower(int power) =>
        power >= 0 ? 1 << power : 1f / (1 << -power);
}
