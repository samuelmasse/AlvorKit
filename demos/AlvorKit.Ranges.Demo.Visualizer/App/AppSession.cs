namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Owns the allocator visualizer model and the commands the menus can issue.</summary>
[App]
public class AppSession
{
    private const int RecentSlotCapacity = 128;
    private const int MinSpeedPower = -2;
    private const int MaxSpeedPower = 11;
    private const int DefaultSpeedPower = 3;

    private readonly AllocatorScenario[] scenarios = AllocatorScenario.CreateAll();
    private readonly AllocatorScenarioRunner runner = new();
    private readonly int[] recentSlots = new int[RecentSlotCapacity];
    private int scenarioIndex;
    private float stepAccumulator;
    private float animationPhase = 1f;
    private int speedPower = DefaultSpeedPower;
    private bool playing = true;
    private bool showLabels = true;
    private bool showPadding = true;
    private bool scenarioPickerOpen;
    private int visualRevision;
    private int recentSlotCount;
    private AppMemoryOverlayMode memoryOverlayMode;
    private AppTimelineOverlayMode timelineOverlayMode;
    private bool[] outlierSlots = [];
    private int outlierSlotCount;
    private int[] outlierOperationCounts = [];
    private int[] outlierReallocCounts = [];
    private long[] outlierMinSizes = [];
    private long[] outlierMaxSizes = [];
    private AppRangeMotionKind[] rangeMotionBySlot = [];
    private long[] previousRangeIndexBySlot = [];
    private bool[] previousRangeExistsBySlot = [];
    private int rangeMotionRevision = -1;

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
    public AppMemoryOverlayMode MemoryOverlayMode => memoryOverlayMode;
    public AppTimelineOverlayMode TimelineOverlayMode => timelineOverlayMode;
    public int OutlierSlotCount => outlierSlotCount;
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

    public void ToggleLabels()
    {
        showLabels = !showLabels;
        visualRevision++;
    }

    public void TogglePadding()
    {
        showPadding = !showPadding;
        visualRevision++;
    }

    public void NextMemoryOverlayMode()
    {
        memoryOverlayMode = Next(memoryOverlayMode);
        visualRevision++;
    }

    public void NextTimelineOverlayMode()
    {
        timelineOverlayMode = Next(timelineOverlayMode);
        visualRevision++;
    }

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
        RebuildRecentSlots();
        stepAccumulator = 0f;
        animationPhase = 0f;
        visualRevision++;
    }

    public void StepForward()
    {
        playing = false;
        if (runner.StepForward())
        {
            RecordRecentSlot(runner.LastCommand);
            animationPhase = 0f;
            visualRevision++;
        }
    }

    public void StepBackward()
    {
        playing = false;
        if (runner.StepBackward())
        {
            RebuildRecentSlots();
            animationPhase = 1f;
            visualRevision++;
        }
    }

    public void JumpToPack()
    {
        playing = false;
        if (runner.JumpToPack())
        {
            RebuildRecentSlots();
            animationPhase = 0f;
            visualRevision++;
        }
    }

    public bool TryRecentSlotAge(int slot, out int age)
    {
        for (var i = 0; i < recentSlotCount; i++)
        {
            if (recentSlots[i] != slot)
                continue;

            age = i;
            return true;
        }

        age = 0;
        return false;
    }

    public bool IsOutlierSlot(int slot) =>
        slot >= 0 && slot < outlierSlots.Length && outlierSlots[slot];

    public float OutlierIntensity(int slot)
    {
        if (slot < 0 || slot >= outlierSlots.Length)
            return 0f;

        var sizeSpan = outlierMinSizes[slot] == long.MaxValue ? 0 : outlierMaxSizes[slot] - outlierMinSizes[slot];
        var operationScore = Math.Clamp(outlierOperationCounts[slot] / 32f, 0f, 1f);
        var reallocScore = Math.Clamp(outlierReallocCounts[slot] / 24f, 0f, 1f);
        var sizeScore = Math.Clamp(sizeSpan / 8192f, 0f, 1f);
        return Math.Max(operationScore, Math.Max(reallocScore, sizeScore));
    }

    public AppRangeMotionKind RangeMotionForSlot(int slot)
    {
        EnsureRangeMotion();
        return slot >= 0 && slot < rangeMotionBySlot.Length
            ? rangeMotionBySlot[slot]
            : AppRangeMotionKind.None;
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
        AnalyzeScenarioSlots(runner.Scenario);
        ClearRecentSlots();
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
                ClearRecentSlots();
                animationPhase = 1f;
                visualRevision++;
                continue;
            }

            RecordRecentSlot(runner.LastCommand);
            animationPhase = 0f;
            visualRevision++;
        }
    }

    private void ClearRecentSlots()
    {
        Array.Fill(recentSlots, -1);
        recentSlotCount = 0;
    }

    private void RecordRecentSlot(AllocatorCommand command)
    {
        if (command.Kind is not (AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free))
            return;

        var slot = command.Slot;
        var existingIndex = -1;
        for (var i = 0; i < recentSlotCount; i++)
        {
            if (recentSlots[i] == slot)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex < 0 && recentSlotCount < recentSlots.Length)
            recentSlotCount++;
        else if (existingIndex < 0)
            existingIndex = recentSlots.Length - 1;

        for (var i = existingIndex < 0 ? recentSlotCount - 1 : existingIndex; i > 0; i--)
            recentSlots[i] = recentSlots[i - 1];

        recentSlots[0] = slot;
    }

    private void RebuildRecentSlots()
    {
        ClearRecentSlots();

        var commands = runner.Scenario.Commands;
        var start = Math.Max(0, runner.StepIndex - recentSlots.Length);
        for (var i = start; i < runner.StepIndex; i++)
            RecordRecentSlot(commands[i]);
    }

    private void AnalyzeScenarioSlots(AllocatorScenario scenario)
    {
        EnsureOutlierCapacity(scenario.HandleSlotCount);
        Array.Fill(outlierSlots, false);
        Array.Fill(outlierOperationCounts, 0);
        Array.Fill(outlierReallocCounts, 0);
        Array.Fill(outlierMinSizes, long.MaxValue);
        Array.Fill(outlierMaxSizes, 0);

        var commands = scenario.Commands;
        for (var i = 0; i < commands.Length; i++)
        {
            var command = commands[i];
            if (command.Kind is not (AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free))
                continue;

            outlierOperationCounts[command.Slot]++;
            if (command.Kind is not (AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc))
                continue;

            if (command.Kind == AllocatorCommandKind.Realloc)
                outlierReallocCounts[command.Slot]++;

            outlierMinSizes[command.Slot] = Math.Min(outlierMinSizes[command.Slot], command.Size);
            outlierMaxSizes[command.Slot] = Math.Max(outlierMaxSizes[command.Slot], command.Size);
        }

        outlierSlotCount = 0;
        for (var i = 0; i < scenario.HandleSlotCount; i++)
        {
            var sizeSpan = outlierMinSizes[i] == long.MaxValue ? 0 : outlierMaxSizes[i] - outlierMinSizes[i];
            var outlier =
                outlierOperationCounts[i] >= 16 ||
                outlierReallocCounts[i] >= 8 ||
                outlierMaxSizes[i] >= 4096 ||
                sizeSpan >= 2048;
            outlierSlots[i] = outlier;
            if (outlier)
                outlierSlotCount++;
        }
    }

    private void EnsureOutlierCapacity(int count)
    {
        if (outlierSlots.Length >= count)
            return;

        outlierSlots = new bool[count];
        outlierOperationCounts = new int[count];
        outlierReallocCounts = new int[count];
        outlierMinSizes = new long[count];
        outlierMaxSizes = new long[count];
    }

    private void EnsureRangeMotion()
    {
        if (rangeMotionRevision == visualRevision)
            return;

        var count = runner.Scenario.HandleSlotCount;
        EnsureRangeMotionCapacity(count);
        Array.Fill(rangeMotionBySlot, AppRangeMotionKind.None);
        Array.Fill(previousRangeExistsBySlot, false);

        var previousRanges = runner.Previous.Ranges;
        for (var i = 0; i < previousRanges.Length; i++)
        {
            var range = previousRanges[i];
            previousRangeIndexBySlot[range.Slot] = range.Index;
            previousRangeExistsBySlot[range.Slot] = true;
        }

        var currentRanges = runner.Current.Ranges;
        for (var i = 0; i < currentRanges.Length; i++)
        {
            var range = currentRanges[i];
            if (!previousRangeExistsBySlot[range.Slot])
                rangeMotionBySlot[range.Slot] = AppRangeMotionKind.New;
            else if (previousRangeIndexBySlot[range.Slot] == range.Index)
                rangeMotionBySlot[range.Slot] = AppRangeMotionKind.Reused;
            else
                rangeMotionBySlot[range.Slot] = AppRangeMotionKind.Moved;
        }

        rangeMotionRevision = visualRevision;
    }

    private void EnsureRangeMotionCapacity(int count)
    {
        if (rangeMotionBySlot.Length >= count)
            return;

        rangeMotionBySlot = new AppRangeMotionKind[count];
        previousRangeIndexBySlot = new long[count];
        previousRangeExistsBySlot = new bool[count];
    }

    private static float SpeedFromPower(int power) =>
        power >= 0 ? 1 << power : 1f / (1 << -power);

    private static TEnum Next<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        var index = Array.IndexOf(values, value);
        return values[(index + 1) % values.Length];
    }
}
