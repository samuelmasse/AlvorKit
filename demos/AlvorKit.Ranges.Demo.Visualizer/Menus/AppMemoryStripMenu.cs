namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryStripMenu(
    AppStyle style,
    AppSession session,
    AppMemoryStripGeometry geometry,
    AppMemoryStripLabels labels,
    AppMemoryStripTooltips tooltips)
{
    public void Create(EntMut root, AppMemoryStripView view)
    {
        if (view.ViewEnd <= view.ViewStart)
            return;

        var snapshot = view.Snapshot;
        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            if (!geometry.Intersects(view, span.Index, span.Size))
                continue;

            var mutedTail = view.MuteTail && span.Index + span.Size == snapshot.Size && snapshot.Ranges.Length > 0;
            var color = mutedTail ? style.TailFreeBlockColor : style.FreeBlockColor;
            Rect(root, view, span.Index, span.Size, () => color)
                .IsSelectableV(true)
                .TooltipF(() => tooltips.Free(view, span.Index, span.Size, mutedTail));

            if (span.Size > 0)
            {
                const float freeBlockEdgeHeight = 2f;

                Rect(root, view, span.Index, span.Size, () => style.FreeBlockEdgeColor, height: freeBlockEdgeHeight);
            }
        }

        var activeSlot = session.ActiveSlot;
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            if (!geometry.Intersects(view, range.Index, range.ReservedSize))
                continue;

            const float reservedDimFactor = 0.42f;

            var color = style.AllocationColor(range.Slot);
            Rect(root, view, range.Index, range.ReservedSize, () => style.Dim(color, reservedDimFactor));

            if (range.LeadingPadding > 0)
            {
                Rect(root, view, range.Index, range.LeadingPadding, () => style.PaddingColor)
                    .IsSelectableV(true)
                    .TooltipF(() => tooltips.Padding(view, range, range.LeadingPadding, leading: true))
                    .IsDisabledF(() => !session.ShowPadding);
            }

            if (range.TrailingPadding > 0)
            {
                var trailingIndex = range.PayloadIndex + range.CapacitySize;
                Rect(root, view, trailingIndex, range.TrailingPadding, () => style.PaddingColor)
                    .IsSelectableV(true)
                    .TooltipF(() => tooltips.Padding(view, range, range.TrailingPadding, leading: false))
                    .IsDisabledF(() => !session.ShowPadding);
            }

            if (range.RetainedExtraSize > 0)
            {
                var retainedIndex = range.PayloadIndex + range.Size;
                Rect(root, view, retainedIndex, range.RetainedExtraSize, () => style.RetainedColor)
                    .IsSelectableV(true)
                    .TooltipF(() => tooltips.Retained(view, range));
            }

            Rect(root, view, range.PayloadIndex, range.Size, () => PayloadColor(range, color))
                .IsSelectableV(true)
                .TooltipF(() => tooltips.Payload(view, range));

            if (session.IsLatestPayloadRequest(range.Slot))
            {
                Rect(root, view, range.PayloadIndex, range.Size, () => style.LatestRequestFillColor)
                    .IsSelectableV(true)
                    .TooltipF(() => tooltips.LatestRequest(view, range));

                const float latestRequestEdgeWidth = 2f;
                Rect(
                    root,
                    view,
                    range.PayloadIndex + Math.Max(0, range.Size - 1),
                    1,
                    () => style.LatestRequestEdgeColor,
                    width: latestRequestEdgeWidth);
            }

            if (range.Slot == activeSlot)
                ActiveFrame(root, view, range.Index, range.ReservedSize);

            labels.Create(root, view, range);
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

        Vec4 PayloadColor(AllocatorRangeVisual range, Vec4 color)
        {
            const float activePulseBase = 0.72f;
            const float activePulseRange = 0.28f;
            const float retainedRequestPulseFactor = 0.58f;

            var active = session.ActiveSlot == range.Slot;
            var pulse = active
                ? activePulseBase + activePulseRange * MathF.Sin(session.AnimationPhase * MathF.PI)
                : 1f;
            if (active && range.Size < range.CapacitySize && session.IsLatestPayloadRequest(range.Slot))
                pulse *= retainedRequestPulseFactor;

            return style.Dim(color, pulse);
        }
    }
}
