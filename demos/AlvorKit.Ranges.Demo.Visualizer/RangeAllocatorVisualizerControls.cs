namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Names the controls used by the range allocator visualizer.</summary>
internal sealed record RangeAllocatorVisualizerControls(
    Control PlayPause,
    Control StepForward,
    Control StepBackward,
    Control ResetScenario,
    Control NextScenario,
    Control PreviousScenario,
    Control Faster,
    Control Slower,
    Control UiScaleUp,
    Control UiScaleDown,
    Control JumpPack,
    Control ToggleLabels,
    Control TogglePadding,
    Control ToggleTrails) : ControlList;
