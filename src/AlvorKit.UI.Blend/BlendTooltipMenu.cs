namespace AlvorKit.UI.Blend;

/// <summary>Builds a mouse-following tooltip layer fed by hovered-node tooltip text.</summary>
public class BlendTooltipMenu(RootUiMouse uiMouse, BlendStyle style)
{
    public void Create(EntMut root)
    {
        const float tooltipOffset = 14f;
        const float tooltipLift = 30f;

        var inset = style.Metrics.LooseSpacing;
        Node(root, out var layer)
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => Value().Length == 0);
        {
            Node(layer, out var tooltip)
                .Mutate(style.Tooltip)
                .TextF(Value)
                .OffsetF(() => TooltipOffset(layer, tooltip));
        }

        ReadOnlySpan<char> Value() => uiMouse.Hovered.TooltipFV.Resolve();

        Vec2 TooltipOffset(EntMut root, EntMut tooltip)
        {
            var desired = uiMouse.Position + (tooltipOffset, -tooltipLift);
            var max = Vec2.Max((0, 0), root.SizeR - tooltip.SizeR - (inset, inset));
            return (
                Math.Clamp(desired.X, inset, max.X),
                Math.Clamp(desired.Y, inset, max.Y));
        }
    }
}
