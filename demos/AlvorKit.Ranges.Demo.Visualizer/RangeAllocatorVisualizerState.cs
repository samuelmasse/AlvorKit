namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Runs the interactive slow-motion range allocator visualizer.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootScale scale,
    RootControlsToml controlsToml,
    RangeAllocatorVisualizerControls controls,
    RangeAllocatorVisualizerRenderer renderer) : State
{
    private readonly AllocatorScenario[] scenarios = AllocatorScenario.CreateAll();
    private readonly AllocatorScenarioRunner runner = new();
    private int scenarioIndex;
    private float stepAccumulator;
    private float animationPhase = 1f;
    private float speed = 1f;
    private bool playing = true;
    private bool showLabels = true;
    private bool showPadding = true;
    private bool showTrails = true;

    /// <summary>Loads controls, initializes the first scenario, and shows the window.</summary>
    public override void Load()
    {
        controlsToml.AddFromFile(Path.Combine(AppContext.BaseDirectory, "RangeAllocatorVisualizer.Controls.toml"));
        screen.Title = "AlvorKit.Ranges.Demo.Visualizer";
        screen.IsVisible = true;
        LoadScenario(0);
    }

    /// <summary>Updates playback controls and advances the scripted allocator steps.</summary>
    public override void Update(double delta)
    {
        if (controls.PlayPause.Run())
            playing = !playing;
        if (controls.ToggleLabels.Run())
            showLabels = !showLabels;
        if (controls.TogglePadding.Run())
            showPadding = !showPadding;
        if (controls.ToggleTrails.Run())
            showTrails = !showTrails;
        if (controls.Faster.Run())
            speed = Math.Min(12f, speed * 1.25f + 0.05f);
        if (controls.Slower.Run())
            speed = Math.Max(0.1f, speed / 1.25f);
        if (controls.UiScaleUp.Run())
            scale.Numerator = Math.Min(scale.Denominator * 4, scale.Numerator + 1);
        if (controls.UiScaleDown.Run())
            scale.Numerator = Math.Max(1, scale.Numerator - 1);
        if (controls.NextScenario.Run())
            LoadScenario(scenarioIndex + 1);
        if (controls.PreviousScenario.Run())
            LoadScenario(scenarioIndex - 1);
        if (controls.ResetScenario.Run())
            LoadScenario(scenarioIndex);
        if (controls.JumpPack.Run())
            JumpToPack();
        if (controls.StepForward.Run())
            ManualStepForward();
        if (controls.StepBackward.Run())
            ManualStepBackward();

        animationPhase = Math.Min(1f, animationPhase + (float)delta * 2.8f);
        if (playing)
            AdvancePlayback((float)delta);
    }

    /// <summary>Clears the OpenGL backbuffer before two-dimensional drawing.</summary>
    public override void Render() => backbuffer.Clear(renderer.ClearColor);

    /// <summary>Draws allocator state, timeline, metrics, and pack movement trails.</summary>
    public override void Draw() =>
        renderer.Draw(
            runner.Scenario,
            runner,
            animationPhase,
            speed,
            playing,
            showLabels,
            showPadding,
            showTrails);

    /// <summary>Loads a scenario by wrapping around the built-in scenario list.</summary>
    private void LoadScenario(int index)
    {
        scenarioIndex = ((index % scenarios.Length) + scenarios.Length) % scenarios.Length;
        runner.Load(scenarios[scenarioIndex]);
        stepAccumulator = 0f;
        animationPhase = 1f;
    }

    /// <summary>Advances automatic playback by elapsed time.</summary>
    private void AdvancePlayback(float delta)
    {
        stepAccumulator += delta * speed;
        while (stepAccumulator >= 1f)
        {
            stepAccumulator -= 1f;
            if (!runner.StepForward())
            {
                playing = false;
                stepAccumulator = 0f;
                break;
            }

            animationPhase = 0f;
        }
    }

    /// <summary>Applies one forward step and pauses playback.</summary>
    private void ManualStepForward()
    {
        playing = false;
        if (runner.StepForward())
            animationPhase = 0f;
    }

    /// <summary>Replays one step backward and pauses playback.</summary>
    private void ManualStepBackward()
    {
        playing = false;
        if (runner.StepBackward())
            animationPhase = 1f;
    }

    /// <summary>Jumps to the next pack operation and pauses playback on the resulting packed frame.</summary>
    private void JumpToPack()
    {
        playing = false;
        if (runner.JumpToPack())
            animationPhase = 0f;
    }
}
