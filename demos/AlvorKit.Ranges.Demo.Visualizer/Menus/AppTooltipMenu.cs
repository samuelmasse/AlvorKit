namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppTooltipMenu(
    RootUiMouse uiMouse,
    AppStyle s)
{
    public void Create(EntMut root)
    {
        const float tooltipOffset = 14f;
        const float tooltipLift = 30f;

        Node(root, out var layer)
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => Value().Length == 0);
        {
            Node(layer, out var tooltip)
                .Mutate(s.Tooltip)
                .TextF(Value)
                .OffsetF(() => TooltipOffset(layer, tooltip));
        }

        ReadOnlySpan<char> Value() => uiMouse.Hovered.TooltipFV.Resolve();

        Vec2 TooltipOffset(EntMut root, EntMut tooltip)
        {
            var desired = uiMouse.Position + (tooltipOffset, -tooltipLift);
            var max = Vec2.Max((0, 0), root.SizeR - tooltip.SizeR - (s.Spacing, s.Spacing));
            return (
                Math.Clamp(desired.X, s.Spacing, max.X),
                Math.Clamp(desired.Y, s.Spacing, max.Y));
        }
    }
}
