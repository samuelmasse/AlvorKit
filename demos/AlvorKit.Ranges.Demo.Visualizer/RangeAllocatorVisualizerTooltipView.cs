namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Displays tooltip text from the currently hovered visualizer UI node.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerTooltipView(
    RootUiMouse uiMouse,
    RangeAllocatorVisualizerStyle style)
{
    /// <summary>Creates the root-level tooltip overlay.</summary>
    internal void Create(EntMut root)
    {
        root.Mutate()
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => Value().Length == 0);

        EntMut tooltip = default;
        Node(root, out tooltip)
            .Mutate(style.Tooltip)
            .TextF(Value)
            .OffsetF(() => TooltipOffset(root, tooltip));
    }

    private ReadOnlySpan<char> Value() => uiMouse.Hovered.TooltipFV.Resolve();

    private Vec2 TooltipOffset(EntMut root, EntMut tooltip)
    {
        var desired = uiMouse.Position + (style.TooltipOffset, -style.TooltipLift);
        var max = Vec2.Max((0, 0), root.SizeR - tooltip.SizeR - (style.Spacing, style.Spacing));
        return (
            Math.Clamp(desired.X, style.Spacing, max.X),
            Math.Clamp(desired.Y, style.Spacing, max.Y));
    }
}
