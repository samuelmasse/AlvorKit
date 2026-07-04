namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Owns the allocator visualizer model and the commands the menus can issue.</summary>
[App]
public class AppSession
{
    private const int MinSpeedPower = -2;
    private const int MaxSpeedPower = 6;
    private const int DefaultSpeedPower = 3;

    private readonly AllocatorScenario[] scenarios = AllocatorScenario.CreateAll();
    private readonly AllocatorScenarioRunner runner = new();
    private int scenarioIndex;
    private float stepAccumulator;
    private float animationPhase = 1f;
    private int speedPower = DefaultSpeedPower;
    private bool playing = true;
    private bool showLabels = true;
    private bool showPadding = true;
    private bool scenarioPickerOpen;
    private int visualRevision;

    public AllocatorScenarioRunner Runner => runner;
    public int ScenarioIndex => scenarioIndex;
    public int ScenarioCount => scenarios.Length;
    public float AnimationPhase => animationPhase;
    public float Speed => SpeedFromPower(speedPower);
    public bool Playing => playing;
    public bool ShowLabels => showLabels;
    public bool ShowPadding => showPadding;
    public bool ScenarioPickerOpen => scenarioPickerOpen;
    public int VisualRevision => visualRevision;
    public int ActiveSlot =>
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free
            ? runner.LastCommand.Slot
            : -1;

    public void LoadInitial() => LoadScenario(0);

    public void Update(double delta)
    {
        animationPhase = Math.Min(1f, animationPhase + (float)delta * 2.8f);
        if (playing)
            AdvancePlayback((float)delta);
    }

    public void TogglePlayback() => playing = !playing;

    public void ToggleLabels() => showLabels = !showLabels;

    public void TogglePadding() => showPadding = !showPadding;

    public void Faster() => speedPower = Math.Min(MaxSpeedPower, speedPower + 1);

    public void Slower() => speedPower = Math.Max(MinSpeedPower, speedPower - 1);

    public void NextScenario() => LoadScenario(scenarioIndex + 1);

    public void PreviousScenario() => LoadScenario(scenarioIndex - 1);

    public AllocatorScenario ScenarioAt(int index) => scenarios[index];

    public void OpenScenarioPicker() => scenarioPickerOpen = true;

    public void CloseScenarioPicker() => scenarioPickerOpen = false;

    public void SelectScenario(int index)
    {
        LoadScenario(index);
        CloseScenarioPicker();
    }

    public void ResetScenario() => LoadScenario(scenarioIndex);

    public void JumpToStep(int step)
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

    public void StepForward()
    {
        playing = false;
        if (runner.StepForward())
        {
            animationPhase = 0f;
            visualRevision++;
        }
    }

    public void StepBackward()
    {
        playing = false;
        if (runner.StepBackward())
        {
            animationPhase = 1f;
            visualRevision++;
        }
    }

    public void JumpToPack()
    {
        playing = false;
        if (runner.JumpToPack())
        {
            animationPhase = 0f;
            visualRevision++;
        }
    }

    public bool IsLatestPayloadRequest(int slot) =>
        runner.LastCommand.Slot == slot &&
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc;

    public bool TryTouchedRange(out AllocatorRangeVisual range)
    {
        var command = runner.LastCommand;
        if (command.Kind is not (AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free))
        {
            range = default;
            return false;
        }

        var snapshot = command.Kind == AllocatorCommandKind.Free ? runner.Previous : runner.Current;
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            if (snapshot.Ranges[i].Slot != command.Slot)
                continue;

            range = snapshot.Ranges[i];
            return true;
        }

        range = default;
        return false;
    }

    private void LoadScenario(int index)
    {
        scenarioIndex = ((index % scenarios.Length) + scenarios.Length) % scenarios.Length;
        runner.Load(scenarios[scenarioIndex]);
        stepAccumulator = 0f;
        animationPhase = 1f;
        visualRevision++;
    }

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

    private static float SpeedFromPower(int power) =>
        power >= 0 ? 1 << power : 1f / (1 << -power);
}
