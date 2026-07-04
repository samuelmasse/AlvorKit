namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppTooltipMenu(
    RootUiMouse uiMouse,
    AppStyle style)
{
    public void Create(EntMut root)
    {
        const float tooltipOffset = 14f;
        const float tooltipLift = 30f;

        root.Mutate()
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => Value().Length == 0);

        Node(root, out var tooltip)
            .Mutate(style.Tooltip)
            .TextF(Value)
            .OffsetF(() => TooltipOffset(root, tooltip));

        ReadOnlySpan<char> Value() => uiMouse.Hovered.TooltipFV.Resolve();

        Vec2 TooltipOffset(EntMut root, EntMut tooltip)
        {
            var desired = uiMouse.Position + (tooltipOffset, -tooltipLift);
            var max = Vec2.Max((0, 0), root.SizeR - tooltip.SizeR - (style.Spacing, style.Spacing));
            return (
                Math.Clamp(desired.X, style.Spacing, max.X),
                Math.Clamp(desired.Y, style.Spacing, max.Y));
        }
    }
}
