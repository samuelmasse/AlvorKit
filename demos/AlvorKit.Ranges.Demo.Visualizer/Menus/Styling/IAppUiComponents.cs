namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Demo-local UI metadata used by the range allocator visualizer.</summary>
[Components(SkipBuilder = true)]
public interface IAppUiComponents
{
    /// <summary>Tooltip text displayed when hovering this node.</summary>
    UiText TooltipFV { get; set; }
}
