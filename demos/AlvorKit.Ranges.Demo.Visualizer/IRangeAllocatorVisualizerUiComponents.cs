namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Demo-local UI metadata used by the range allocator visualizer.</summary>
[Components(SkipBuilder = true)]
internal interface IRangeAllocatorVisualizerUiComponents
{
    /// <summary>Tooltip text displayed when hovering this node.</summary>
    UiText TooltipFV { get; set; }
}
