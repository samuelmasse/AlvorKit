namespace AlvorKit;

/// <summary>
/// Builds a clipped vertical viewport, content list, wheel behavior, and a proportional scrollbar.
/// Callers remain responsible for styling and populating the returned viewport and content nodes.
/// </summary>
public class BlendScrollView(BlendStyle style)
{
    private const float DefaultWheelStep = 48f;
    private const float ScrollbarWidth = 5f;
    private const float MinimumThumbHeight = 18f;

    /// <summary>Builds a vertical scroll view and returns its independently resettable state.</summary>
    public BlendScrollHandle Vertical(
        EntMut parent,
        out EntMut viewport,
        out EntMut content,
        float wheelStep = DefaultWheelStep)
    {
        BlendScrollHandle handle = new();

        Node(parent, out var viewportNode);
        Node(viewportNode, out var contentNode);

        viewportNode
            .Mutate()
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSizingV(InnerSizing.None)
            .InnerScrollOffsetF(() =>
                (0, -MathF.Round(handle.Offset)))
            .IsScrollableV(true)
            .OnScrollF(wheel =>
            {
                handle.Offset = Math.Clamp(
                    handle.Offset - wheel.Y * wheelStep,
                    0,
                    Maximum());
            })
            .OnUpdateF(() =>
                handle.Offset = Math.Clamp(
                    handle.Offset,
                    0,
                    Maximum()));
        {
            contentNode
                .Mutate()
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeInnerSumRelativeV((0, 1))
                .InnerLayoutV(InnerLayout.VerticalList);

            Node(viewportNode, out var track)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((ScrollbarWidth, 0))
                .ColorF(() => style.Palette.AppBackground)
                .IsDisabledF(() => Maximum() <= 0);
            {
                Node(track)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Top)
                    .SizeRelativeV((1, 0))
                    .SizeF(() => (0, ThumbHeight()))
                    .OffsetF(() => (0, ThumbOffset()))
                    .ColorF(() => style.Palette.Accent);
            }
        }

        viewport = viewportNode;
        content = contentNode;
        return handle;

        float Maximum() =>
            MathF.Max(
                0,
                contentNode.SizeR.Y
                - viewportNode.SizeR.Y);

        float ThumbHeight()
        {
            if (contentNode.SizeR.Y <= 0)
                return viewportNode.SizeR.Y;

            return Math.Clamp(
                viewportNode.SizeR.Y
                * viewportNode.SizeR.Y
                / contentNode.SizeR.Y,
                MinimumThumbHeight,
                viewportNode.SizeR.Y);
        }

        float ThumbOffset()
        {
            var maximum = Maximum();
            if (maximum <= 0)
                return 0;

            return handle.Offset
                   / maximum
                   * MathF.Max(
                       0,
                       viewportNode.SizeR.Y
                       - ThumbHeight());
        }
    }
}
