namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryStripMenu(
    AppStyle style,
    AppSession session,
    AppMemoryStripGeometry geometry,
    AppMemoryStripTexture texture)
{
    public void Create(EntMut root, AppMemoryStripView view)
    {
        if (view.ViewEnd <= view.ViewStart)
            return;

        var snapshot = view.Snapshot;
        Node(root)
            .SizeRelativeV((1, 1))
            .TextureF(() => texture.Texture(view))
            .IsSelectableV(true)
            .TooltipF(() => texture.Tooltip(view, root));

        var activeSlot = session.ActiveSlot;
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            if (!geometry.Intersects(view, range.Index, range.ReservedSize))
                continue;

            if (range.Slot == activeSlot)
                ActiveFrame(root, view, range.Index, range.ReservedSize);
        }

        Outline(root, () => style.MemoryStripOutlineColor);

        EntMutator<EntMut> Rect(
            EntMut parent,
            AppMemoryStripView view,
            long index,
            long size,
            Func<Vec4> color,
            float? width = null,
            float? height = null)
        {
            Node(parent, out var node)
                .SizeRelativeV((0, 0))
                .OffsetF(() => geometry.SegmentOffset(parent, view, index))
                .SizeF(() =>
                {
                    var segment = geometry.SegmentSize(parent, view, index, size);
                    return (
                        width ?? segment.X,
                        height ?? segment.Y);
                })
                .ColorF(color);
            return node.Mutate();
        }

        void ActiveFrame(EntMut parent, AppMemoryStripView view, long index, long size)
        {
            const float edgeWidth = 2f;
            const float framePadding = 2f;

            Rect(parent, view, index, size, () => style.MemoryActiveFrameFillColor)
                .OffsetF(() => geometry.SegmentOffset(parent, view, index) - (framePadding, framePadding))
                .SizeF(() => geometry.SegmentSize(parent, view, index, size) + (framePadding + framePadding, framePadding + framePadding));

            Node(parent)
                .SizeRelativeV((0, 0))
                .OffsetF(() => geometry.SegmentOffset(parent, view, index) - (framePadding, framePadding))
                .SizeF(() => (geometry.SegmentSize(parent, view, index, size).X + framePadding + framePadding, edgeWidth))
                .ColorV(style.HighlightColor);

            Node(parent)
                .SizeRelativeV((0, 0))
                .OffsetF(() =>
                {
                    var offset = geometry.SegmentOffset(parent, view, index);
                    var segment = geometry.SegmentSize(parent, view, index, size);
                    return (offset.X - framePadding, offset.Y + segment.Y + framePadding - edgeWidth);
                })
                .SizeF(() => (geometry.SegmentSize(parent, view, index, size).X + framePadding + framePadding, edgeWidth))
                .ColorV(style.HighlightColor);

            Node(parent)
                .SizeRelativeV((0, 0))
                .OffsetF(() => geometry.SegmentOffset(parent, view, index) - (framePadding, framePadding))
                .SizeF(() => (edgeWidth, geometry.SegmentSize(parent, view, index, size).Y + framePadding + framePadding))
                .ColorV(style.HighlightColor);

            Node(parent)
                .SizeRelativeV((0, 0))
                .OffsetF(() =>
                {
                    var offset = geometry.SegmentOffset(parent, view, index);
                    var segment = geometry.SegmentSize(parent, view, index, size);
                    return (offset.X + segment.X + framePadding - edgeWidth, offset.Y - framePadding);
                })
                .SizeF(() => (edgeWidth, geometry.SegmentSize(parent, view, index, size).Y + framePadding + framePadding))
                .ColorV(style.HighlightColor);
        }

        void Outline(EntMut parent, Func<Vec4> color)
        {
            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, style.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, style.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((0, 1))
                .SizeV((style.RuleWidth, 0))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((style.RuleWidth, 0))
                .ColorF(color);
        }
    }
}
