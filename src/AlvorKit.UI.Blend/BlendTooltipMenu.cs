namespace AlvorKit.UI.Blend;

/// <summary>Builds a mouse-following tooltip layer that renders hovered-node tooltip text as a title plus muted detail lines.</summary>
public class BlendTooltipMenu(RootUiMouse uiMouse, BlendStyle style)
{
    public void Create(EntMut root)
    {
        const float tooltipOffsetX = 14f;
        const float tooltipOffsetY = 18f;
        const float shadowOffsetX = 3f;
        const float shadowOffsetY = 4f;
        const int bodyLineCount = 3;

        var inset = style.Metrics.LooseSpacing;
        var shadowColor = style.Palette.WithAlpha(default, 0.35f);
        Node(root, out var layer)
            .SizeRelativeV((1, 1))
            .IsDisabledF(() => Value().Length == 0);
        {
            Node(layer, out var shadow)
                .SizeRelativeV((0, 0))
                .ColorV(shadowColor);

            Node(layer, out var tooltip)
                .Mutate(style.Tooltip)
                .OffsetF(() => TooltipOffset(layer, tooltip));
            {
                Node(tooltip, out var titleRow)
                    .Mutate(style.HorizontalList)
                    .InnerSpacingV(style.Metrics.LooseSpacing);
                {
                    Node(titleRow)
                        .Mutate(style.Swatch)
                        .ColorF(Swatch)
                        .IsDisabledF(() => Swatch().W == 0);

                    Node(titleRow)
                        .Mutate(style.TooltipTitle)
                        .TextF(() => Line(0));
                }

                for (var i = 1; i <= bodyLineCount; i++)
                {
                    var lineIndex = i;
                    Node(tooltip)
                        .Mutate(style.TooltipLine)
                        .TextF(() => Line(lineIndex))
                        .IsDisabledF(() => Line(lineIndex).Length == 0);
                }
            }

            shadow.Mutate()
                .OffsetF(() => TooltipOffset(layer, tooltip) + (shadowOffsetX, shadowOffsetY))
                .SizeF(() => tooltip.SizeR);
        }

        ReadOnlySpan<char> Value() => uiMouse.Hovered.TooltipFV.Resolve();

        Vec4 Swatch() => uiMouse.Hovered.TooltipColorFV.Resolve();

        ReadOnlySpan<char> Line(int index)
        {
            var remaining = Value();
            for (var i = 0; i < index; i++)
            {
                var newline = remaining.IndexOf('\n');
                if (newline < 0)
                    return default;

                remaining = remaining[(newline + 1)..];
            }

            var end = remaining.IndexOf('\n');
            return end < 0 ? remaining : remaining[..end];
        }

        Vec2 TooltipOffset(EntMut root, EntMut tooltip)
        {
            var size = tooltip.SizeR;
            var mouse = uiMouse.Position;

            var x = mouse.X + tooltipOffsetX;
            if (x + size.X > root.SizeR.X - inset)
                x = Math.Max(inset, mouse.X - tooltipOffsetX - size.X);

            var y = mouse.Y + tooltipOffsetY;
            if (y + size.Y > root.SizeR.Y - inset)
                y = Math.Max(inset, mouse.Y - tooltipOffsetY - size.Y);

            return (x, y);
        }
    }
}
