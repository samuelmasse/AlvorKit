namespace AlvorKit.UI.Blend;

/// <summary>Reusable pixel metrics for Blender-inspired editor UI.</summary>
public readonly record struct BlendMetrics(
    float MenuBarHeight,
    float ToolbarHeight,
    float StatusBarHeight,
    float PanelTitleHeight,
    float ViewportHeaderHeight,
    float TabStripHeight,
    float AssetToolbarHeight,
    float ButtonHeight,
    float ChipHeight,
    float SquareButtonSize,
    float ControlRadius,
    float ControlBorderWidth,
    float Hairline)
{
    /// <summary>Gets the metrics used by the stripped editor-shell HTML reference.</summary>
    public static BlendMetrics EditorShell { get; } = new(
        25f,
        35f,
        22f,
        28f,
        29f,
        29f,
        31f,
        26f,
        23f,
        26f,
        2f,
        1f,
        1f);
}
