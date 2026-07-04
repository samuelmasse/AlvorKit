namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds allocator memory and timeline charts from ordinary UI nodes.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerMemoryView(
    RootText text,
    RootUiMouse uiMouse,
    RootMouse mouse,
    RangeAllocatorVisualizerStyle style)
{
    private const float LabelHeight = 16f;
    private const float DetailGap = 28f;
    private const float SegmentMinWidth = 1.5f;
    private const float EdgeWidth = 2f;
    private const float TimelineMarkerWidth = 3f;
    private const float TimelineDividerWidth = 1f;

    /// <summary>Creates a live, node-only memory chart that rebuilds when allocator snapshots change.</summary>
    internal void CreateMemoryCharts(EntMut root, RangeAllocatorVisualizerState state)
    {
        var lastRevision = -1;
        EntMut content = default;
        Node(root, out content)
            .SizeRelativeV((1, 1))
            .ColorV(style.PanelInsetColor)
            .OnUpdateF(() =>
            {
                if (lastRevision == state.VisualRevision)
                    return;

                lastRevision = state.VisualRevision;
                NodesClear(content);
                BuildMemoryCharts(content, state);
            });
    }

    /// <summary>Creates a live, node-only timeline that supports click and drag scrubbing.</summary>
    internal void CreateTimeline(EntMut root, RangeAllocatorVisualizerState state)
    {
        var lastRevision = -1;
        var scrubbing = false;
        var mainWasDown = false;
        EntMut content = default;
        void BeginScrub()
        {
            scrubbing = true;
            ScrubTimeline(content, state);
        }

        Node(root, out content)
            .SizeRelativeV((1, 1))
            .ColorV(style.PanelInsetColor)
            .OnUpdateF(() =>
            {
                var mainDown = mouse.IsMainDown();
                if (!mainDown)
                    scrubbing = false;
                else if (!mainWasDown && TimelinePointerOverCell(content, state.Runner.Scenario.Commands.Length))
                    scrubbing = true;

                if (scrubbing)
                    ScrubTimeline(content, state);

                mainWasDown = mainDown;
                if (lastRevision == state.VisualRevision)
                    return;

                lastRevision = state.VisualRevision;
                NodesClear(content);
                BuildTimeline(content, state, BeginScrub);
            });
    }

    private void BuildMemoryCharts(EntMut root, RangeAllocatorVisualizerState state)
    {
        var snapshot = state.Runner.Current;
        var detailEnd = DetailEnd(snapshot, out var tailOmitted);

        Node(root)
            .Mutate(style.MutedLabel)
            .TextV("full backing store")
            .OffsetF(() => MemoryLayout(root).OverviewLabel);

        Node(root, out var overview)
            .Mutate(ChartStrip)
            .TooltipF(() => StoreTooltip("full backing store", snapshot))
            .OffsetF(() => MemoryLayout(root).OverviewStrip)
            .SizeF(() => MemoryLayout(root).OverviewStripSize);
        BuildStrip(overview, state, snapshot, 0, snapshot.Size, "full backing store", muteTail: true, detailedLabels: false);

        Node(root)
            .Mutate(style.MutedLabel)
            .TextF(() => DetailLabel(detailEnd, tailOmitted))
            .OffsetF(() => MemoryLayout(root).DetailLabel);

        Node(root, out var detail)
            .Mutate(ChartStrip)
            .TooltipF(() => DetailTooltip(detailEnd, tailOmitted))
            .OffsetF(() => MemoryLayout(root).DetailStrip)
            .SizeF(() => MemoryLayout(root).DetailStripSize);
        BuildStrip(detail, state, snapshot, 1, detailEnd, "active region zoom", muteTail: false, detailedLabels: true);
    }

    private void BuildStrip(
        EntMut strip,
        RangeAllocatorVisualizerState state,
        AllocatorSnapshot snapshot,
        long viewStart,
        long viewEnd,
        string viewName,
        bool muteTail,
        bool detailedLabels)
    {
        if (viewEnd <= viewStart)
            return;

        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            if (!Intersects(viewStart, viewEnd, span.Index, span.Size))
                continue;

            var mutedTail = muteTail && span.Index + span.Size == snapshot.Size && snapshot.Ranges.Length > 0;
            var color = mutedTail
                ? AllocatorVisualFacts.TailFree
                : AllocatorVisualFacts.Free;
            Rect(strip, viewStart, viewEnd, span.Index, span.Size, () => color)
                .IsSelectableV(true)
                .TooltipF(() => FreeTooltip(viewName, span.Index, span.Size, mutedTail));

            if (span.Size > 0)
                Rect(strip, viewStart, viewEnd, span.Index, span.Size, () => AllocatorVisualFacts.FreeEdge, height: 2f);
        }

        var activeSlot = AllocatorVisualFacts.ActiveSlot(state.Runner);
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            if (!Intersects(viewStart, viewEnd, range.Index, range.ReservedSize))
                continue;

            var color = AllocatorVisualFacts.Palette(range.Slot);
            Rect(strip, viewStart, viewEnd, range.Index, range.ReservedSize, () => AllocatorVisualFacts.Dim(color, 0.42f));

            if (range.LeadingPadding > 0)
            {
                Rect(strip, viewStart, viewEnd, range.Index, range.LeadingPadding, () => AllocatorVisualFacts.Padding)
                    .IsSelectableV(true)
                    .TooltipF(() => PaddingTooltip(viewName, range, range.LeadingPadding, leading: true))
                    .IsDisabledF(() => !state.ShowPadding);
            }

            if (range.TrailingPadding > 0)
            {
                var trailingIndex = range.PayloadIndex + range.CapacitySize;
                Rect(strip, viewStart, viewEnd, trailingIndex, range.TrailingPadding, () => AllocatorVisualFacts.Padding)
                    .IsSelectableV(true)
                    .TooltipF(() => PaddingTooltip(viewName, range, range.TrailingPadding, leading: false))
                    .IsDisabledF(() => !state.ShowPadding);
            }

            if (range.RetainedExtraSize > 0)
            {
                var retainedIndex = range.PayloadIndex + range.Size;
                Rect(strip, viewStart, viewEnd, retainedIndex, range.RetainedExtraSize, () => AllocatorVisualFacts.Retained)
                    .IsSelectableV(true)
                    .TooltipF(() => RetainedTooltip(viewName, range));
            }

            Rect(strip, viewStart, viewEnd, range.PayloadIndex, range.Size, () => PayloadColor(state, range, color))
                .IsSelectableV(true)
                .TooltipF(() => PayloadTooltip(viewName, range));

            if (AllocatorVisualFacts.IsLatestPayloadRequest(state.Runner, range.Slot))
            {
                Rect(strip, viewStart, viewEnd, range.PayloadIndex, range.Size, () => AllocatorVisualFacts.RequestFill)
                    .IsSelectableV(true)
                    .TooltipF(() => LatestRequestTooltip(viewName, state.Runner, range));
                Rect(
                    strip,
                    viewStart,
                    viewEnd,
                    range.PayloadIndex + Math.Max(0, range.Size - 1),
                    1,
                    () => AllocatorVisualFacts.RequestEdge,
                    width: EdgeWidth);
            }

            if (range.Slot == activeSlot)
                ActiveFrame(strip, viewStart, viewEnd, range.Index, range.ReservedSize);

            RangeLabel(strip, state, range, viewStart, viewEnd, detailedLabels);
        }

        Outline(strip, () => (0.22f, 0.25f, 0.28f, 1f));
    }

    private void BuildTimeline(EntMut lane, RangeAllocatorVisualizerState state, Action beginScrub)
    {
        var commands = state.Runner.Scenario.Commands;
        if (commands.Length == 0)
            return;

        for (var i = 0; i < commands.Length; i++)
        {
            var index = i;
            var command = commands[index];
            Node(lane, out var cell)
                .SizeRelativeV((0, 0))
                .OffsetF(() => TimelineCellOffset(lane, commands.Length, index))
                .SizeF(() => TimelineCellSize(lane, commands.Length))
                .ColorF(() => TimelineCellColor(state, command.Kind, index))
                .IsSelectableV(true)
                .IsSilentFocusableV(true)
                .CursorF(() => CursorShape.ResizeHorizontal)
                .TooltipF(() => CommandTooltip(index, command))
                .OnPressF(beginScrub);

            if (index < commands.Length - 1)
                TimelineDivider(cell);
        }

        Node(lane)
            .SizeRelativeV((0, 1))
            .SizeV((TimelineMarkerWidth, 0))
            .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, state.Runner.StepIndex))
            .ColorV(AllocatorVisualFacts.Highlight);

        Node(lane)
            .SizeRelativeV((0, 0))
            .SizeF(() => (TimelineMarkerWidth + 4f, 3f))
            .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, state.Runner.StepIndex) - (2f, 0))
            .ColorV(AllocatorVisualFacts.Highlight);

        Node(lane)
            .SizeRelativeV((0, 1))
            .SizeF(() => TimelineCellSize(lane, commands.Length))
            .OffsetF(() => TimelineCellOffset(lane, commands.Length, TimelineHoverIndex(lane, commands.Length)))
            .ColorV((1f, 1f, 1f, 0.18f))
            .IsDisabledF(() => !TimelinePointerOverCell(lane, commands.Length));

        Outline(lane, () => TimelinePointerOverCell(lane, commands.Length)
            ? style.AccentColor
            : (0.16f, 0.2f, 0.23f, 1f));
    }

    private void ChartStrip(EntMut ent) => ent.Mutate()
        .SizeRelativeV((0, 0))
        .IsSelectableV(true)
        .ColorV(style.PanelInsetColor)
        .InnerAlignmentSnapV(1f);

    private EntMutator<EntMut> Rect(
        EntMut parent,
        long viewStart,
        long viewEnd,
        long index,
        long size,
        Func<Vec4> color,
        float? width = null,
        float? height = null)
    {
        Node(parent, out var node)
            .SizeRelativeV((0, 0))
            .OffsetF(() => SegmentOffset(parent, viewStart, viewEnd, index, size))
            .SizeF(() =>
            {
                var segment = SegmentSize(parent, viewStart, viewEnd, index, size);
                return (
                    width ?? segment.X,
                    height ?? segment.Y);
            })
            .ColorF(color);
        return node.Mutate();
    }

    private void RangeLabel(
        EntMut strip,
        RangeAllocatorVisualizerState state,
        AllocatorRangeVisual range,
        long viewStart,
        long viewEnd,
        bool detailed)
    {
        if (!Intersects(viewStart, viewEnd, range.Index, range.ReservedSize))
            return;

        Node(strip)
            .Mutate(style.Label)
            .FontSizeV(style.FontSizeSmall)
            .TextV(text.Format("#{0}", range.Slot).ToString())
            .OffsetF(() => SegmentOffset(strip, viewStart, viewEnd, range.Index, range.ReservedSize) + (4f, 3f))
            .IsDisabledF(() => !state.ShowLabels || SegmentSize(strip, viewStart, viewEnd, range.Index, range.ReservedSize).X <= 28f);

        if (!detailed)
            return;

        Node(strip)
            .Mutate(style.MutedLabel)
            .TextF(() => text.Format("{0}B requested", range.Size))
            .OffsetF(() => SegmentOffset(strip, viewStart, viewEnd, range.Index, range.ReservedSize) + (4f, 18f))
            .IsDisabledF(() => !state.ShowLabels || !CanFitDetailLabel(strip, viewStart, viewEnd, range));

        Node(strip)
            .Mutate(style.MutedLabel)
            .TextF(() => range.RetainedExtraSize == 0
                ? text.Format("{0}B capacity", range.CapacitySize)
                : text.Format("{0}B capacity, {1}B retained", range.CapacitySize, range.RetainedExtraSize))
            .OffsetF(() => SegmentOffset(strip, viewStart, viewEnd, range.Index, range.ReservedSize) + (4f, 32f))
            .IsDisabledF(() => !state.ShowLabels || !CanFitExtendedLabel(strip, viewStart, viewEnd, range));
    }

    private bool CanFitDetailLabel(EntMut strip, long viewStart, long viewEnd, AllocatorRangeVisual range)
    {
        var segment = SegmentSize(strip, viewStart, viewEnd, range.Index, range.ReservedSize);
        return segment.X > 74f && segment.Y > 38f;
    }

    private bool CanFitExtendedLabel(EntMut strip, long viewStart, long viewEnd, AllocatorRangeVisual range)
    {
        var segment = SegmentSize(strip, viewStart, viewEnd, range.Index, range.ReservedSize);
        return segment.X > 116f && segment.Y > 58f;
    }

    private void ActiveFrame(EntMut parent, long viewStart, long viewEnd, long index, long size)
    {
        var pad = 2f;
        Rect(parent, viewStart, viewEnd, index, size, () => (1f, 0.96f, 0.55f, 0.16f))
            .OffsetF(() => SegmentOffset(parent, viewStart, viewEnd, index, size) - (pad, pad))
            .SizeF(() => SegmentSize(parent, viewStart, viewEnd, index, size) + (pad * 2f, pad * 2f));

        Node(parent)
            .SizeRelativeV((0, 0))
            .OffsetF(() => SegmentOffset(parent, viewStart, viewEnd, index, size) - (pad, pad))
            .SizeF(() => (SegmentSize(parent, viewStart, viewEnd, index, size).X + pad * 2f, 2f))
            .ColorV(AllocatorVisualFacts.Highlight);

        Node(parent)
            .SizeRelativeV((0, 0))
            .OffsetF(() =>
            {
                var offset = SegmentOffset(parent, viewStart, viewEnd, index, size);
                var segment = SegmentSize(parent, viewStart, viewEnd, index, size);
                return (offset.X - pad, offset.Y + segment.Y + pad - 2f);
            })
            .SizeF(() => (SegmentSize(parent, viewStart, viewEnd, index, size).X + pad * 2f, 2f))
            .ColorV(AllocatorVisualFacts.Highlight);

        Node(parent)
            .SizeRelativeV((0, 0))
            .OffsetF(() => SegmentOffset(parent, viewStart, viewEnd, index, size) - (pad, pad))
            .SizeF(() => (2f, SegmentSize(parent, viewStart, viewEnd, index, size).Y + pad * 2f))
            .ColorV(AllocatorVisualFacts.Highlight);

        Node(parent)
            .SizeRelativeV((0, 0))
            .OffsetF(() =>
            {
                var offset = SegmentOffset(parent, viewStart, viewEnd, index, size);
                var segment = SegmentSize(parent, viewStart, viewEnd, index, size);
                return (offset.X + segment.X + pad - 2f, offset.Y - pad);
            })
            .SizeF(() => (2f, SegmentSize(parent, viewStart, viewEnd, index, size).Y + pad * 2f))
            .ColorV(AllocatorVisualFacts.Highlight);
    }

    private void Outline(EntMut parent, Func<Vec4> color)
    {
        Node(parent)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .SizeRelativeV((1, 0))
            .SizeV((0, 1f))
            .ColorF(color);

        Node(parent)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .SizeRelativeV((1, 0))
            .SizeV((0, 1f))
            .ColorF(color);

        Node(parent)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .SizeRelativeV((0, 1))
            .SizeV((1f, 0))
            .ColorF(color);

        Node(parent)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 1))
            .SizeV((1f, 0))
            .ColorF(color);
    }

    private Vec4 PayloadColor(RangeAllocatorVisualizerState state, AllocatorRangeVisual range, Vec4 color)
    {
        var active = AllocatorVisualFacts.ActiveSlot(state.Runner) == range.Slot;
        var pulse = active ? 0.72f + 0.28f * MathF.Sin(state.AnimationPhase * MathF.PI) : 1f;
        if (active && range.Size < range.CapacitySize && AllocatorVisualFacts.IsLatestPayloadRequest(state.Runner, range.Slot))
            pulse *= 0.58f;

        return AllocatorVisualFacts.Dim(color, pulse);
    }

    private void ScrubTimeline(EntMut lane, RangeAllocatorVisualizerState state)
    {
        var count = state.Runner.Scenario.Commands.Length;
        if (count == 0 || lane.SizeR.X <= 0)
            return;

        var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
        var cell = Math.Clamp((int)MathF.Floor(localX / lane.SizeR.X * count), 0, count - 1);
        state.JumpToStep(cell + 1);
    }

    private int TimelineHoverIndex(EntMut lane, int count)
    {
        if (count <= 0 || lane.SizeR.X <= 0)
            return 0;

        var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
        return Math.Clamp((int)MathF.Floor(localX / lane.SizeR.X * count), 0, count - 1);
    }

    private Vec2 TimelineCellOffset(EntMut lane, int count, int index)
    {
        if (count <= 0)
            return default;

        var cellWidth = lane.SizeR.X / count;
        return (index * cellWidth, 0);
    }

    private Vec2 TimelineCellSize(EntMut lane, int count)
    {
        if (count <= 0)
            return default;

        return (Math.Max(1f, lane.SizeR.X / count), lane.SizeR.Y);
    }

    private void TimelineDivider(EntMut cell)
    {
        Node(cell)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 1))
            .SizeV((TimelineDividerWidth, 0))
            .ColorV(style.BackgroundColor);
    }

    private Vec2 TimelineMarkerOffset(EntMut lane, int count, int step)
    {
        if (count <= 0)
            return default;

        return (Math.Clamp(step, 0, count) / (float)count * lane.SizeR.X - TimelineMarkerWidth * 0.5f, 0);
    }

    private Vec4 TimelineCellColor(RangeAllocatorVisualizerState state, AllocatorCommandKind kind, int index)
    {
        var color = AllocatorVisualFacts.CommandColor(kind);
        if (index >= state.Runner.StepIndex)
            color = AllocatorVisualFacts.Dim(color, 0.28f);

        return color;
    }

    private bool TimelinePointerInside(EntMut lane) =>
        uiMouse.Position.X >= lane.PositionR.X &&
        uiMouse.Position.X <= lane.PositionR.X + lane.SizeR.X &&
        uiMouse.Position.Y >= lane.PositionR.Y &&
        uiMouse.Position.Y <= lane.PositionR.Y + lane.SizeR.Y;

    private bool TimelinePointerOverCell(EntMut lane, int count)
        => count > 0 && lane.SizeR.X > 0 && TimelinePointerInside(lane);

    private ReadOnlySpan<char> StoreTooltip(string viewName, AllocatorSnapshot snapshot) =>
        text.Format(
            "{0}: {1} bytes total, {2} used, {3} live ranges; hover colored spans for exact details",
            viewName,
            snapshot.Size,
            snapshot.Used,
            snapshot.LiveCount);

    private ReadOnlySpan<char> DetailTooltip(long detailEnd, bool tailOmitted) =>
        tailOmitted
            ? text.Format("active region zoom: bytes 1..{0}; large empty tail is omitted so live ranges are readable", detailEnd)
            : text.Format("active region zoom: bytes 1..{0}; this scenario currently fits in the detailed view", detailEnd);

    private ReadOnlySpan<char> FreeTooltip(string viewName, long index, long size, bool mutedTail) =>
        mutedTail
            ? text.Format(
                "{0}: tail free block, {1} bytes at byte {2}; muted because the active zoom omits that empty tail",
                viewName,
                size,
                index)
            : text.Format(
                "{0}: free block, {1} bytes at byte {2}; allocator can reuse this gap for a fitting request",
                viewName,
                size,
                index);

    private ReadOnlySpan<char> PaddingTooltip(string viewName, AllocatorRangeVisual range, long size, bool leading) =>
        leading
            ? text.Format(
                "{0}: leading padding of alloc #{1}, {2} bytes; alignment {3} moves payload start to byte {4}",
                viewName,
                range.Slot,
                size,
                range.Alignment,
                range.PayloadIndex)
            : text.Format(
                "{0}: trailing padding of alloc #{1}, {2} bytes; reserved because alignment can need up to {3} extra bytes",
                viewName,
                range.Slot,
                size,
                range.Alignment - 1);

    private ReadOnlySpan<char> RetainedTooltip(string viewName, AllocatorRangeVisual range) =>
        text.Format(
            "{0}: retained capacity of alloc #{1}, {2} bytes; a shrink kept spare capacity until growth or pack",
            viewName,
            range.Slot,
            range.RetainedExtraSize);

    private ReadOnlySpan<char> PayloadTooltip(string viewName, AllocatorRangeVisual range) =>
        text.Format(
            "{0}: alloc #{1} payload, {2} bytes at byte {3}; capacity {4} bytes, alignment {5}",
            viewName,
            range.Slot,
            range.Size,
            range.PayloadIndex,
            range.CapacitySize,
            range.Alignment);

    private ReadOnlySpan<char> LatestRequestTooltip(
        string viewName,
        AllocatorScenarioRunner runner,
        AllocatorRangeVisual range) =>
        text.Format(
            "{0}: latest request touched alloc #{1}, {2} bytes from event #{3}; white overlay marks the allocator call result",
            viewName,
            range.Slot,
            runner.LastCommand.Size,
            runner.StepIndex);

    private ReadOnlySpan<char> CommandTooltip(int index, AllocatorCommand command)
    {
        var eventIndex = index + 1;
        return command.Kind switch
        {
            AllocatorCommandKind.Alloc => text.Format(
                "scripted event #{0}: alloc slot {1}, alignment {2}, size {3}B; {4}",
                eventIndex,
                command.Slot,
                command.Alignment,
                command.Size,
                command.Label),
            AllocatorCommandKind.Realloc => text.Format(
                "scripted event #{0}: realloc slot {1}, alignment {2}, size {3}B; {4}",
                eventIndex,
                command.Slot,
                command.Alignment,
                command.Size,
                command.Label),
            AllocatorCommandKind.Free => text.Format(
                "scripted event #{0}: free slot {1}; {2}",
                eventIndex,
                command.Slot,
                command.Label),
            AllocatorCommandKind.Pack => text.Format(
                "scripted event #{0}: pack live ranges; {1}",
                eventIndex,
                command.Label),
            _ => text.Format("scripted event #{0}: {1}", eventIndex, command.Label),
        };
    }

    private Vec2 SegmentOffset(EntMut strip, long viewStart, long viewEnd, long index, long size)
    {
        var clippedStart = Math.Max(index, viewStart);
        var scale = strip.SizeR.X / Math.Max(1f, viewEnd - viewStart);
        return ((clippedStart - viewStart) * scale, 0);
    }

    private Vec2 SegmentSize(EntMut strip, long viewStart, long viewEnd, long index, long size)
    {
        var clippedStart = Math.Max(index, viewStart);
        var clippedEnd = Math.Min(index + size, viewEnd);
        if (clippedEnd <= clippedStart)
            return default;

        var scale = strip.SizeR.X / Math.Max(1f, viewEnd - viewStart);
        return (Math.Max(SegmentMinWidth, (clippedEnd - clippedStart) * scale), strip.SizeR.Y);
    }

    private MemoryChartLayout MemoryLayout(EntMut root)
    {
        var inset = style.SpacingS;
        var width = Math.Max(0, root.SizeR.X - inset * 2f);
        var height = Math.Max(0, root.SizeR.Y - inset * 2f);
        var detailHeight = Math.Min(126f, Math.Max(54f, height * 0.34f));
        var overviewHeight = Math.Max(48f, height - detailHeight - DetailGap - LabelHeight * 2f);
        var total = overviewHeight + detailHeight + DetailGap + LabelHeight * 2f;
        if (total > height)
        {
            var overflow = total - height;
            detailHeight = Math.Max(36f, detailHeight - overflow);
        }

        var overviewLabel = new Vec2(inset, inset);
        var overviewStrip = overviewLabel + (0, LabelHeight);
        var detailLabel = overviewStrip + (0, overviewHeight + 12f);
        var detailStrip = detailLabel + (0, LabelHeight);
        var detailBottom = Math.Max(36f, root.SizeR.Y - inset - detailStrip.Y);
        return new(
            overviewLabel,
            overviewStrip,
            (width, overviewHeight),
            detailLabel,
            detailStrip,
            (width, detailBottom));
    }

    private ReadOnlySpan<char> DetailLabel(long detailEnd, bool tailOmitted) =>
        tailOmitted
            ? text.Format("active region zoom: bytes 1..{0}; large tail free block is omitted here", detailEnd)
            : text.Format("active region zoom: bytes 1..{0}; full usable range", detailEnd);

    private static long DetailEnd(AllocatorSnapshot snapshot, out bool tailOmitted)
    {
        tailOmitted = false;
        if (snapshot.Ranges.Length == 0 || snapshot.FreeSpans.Length == 0)
            return snapshot.Size;

        var lastFree = snapshot.FreeSpans[^1];
        if (lastFree.Index + lastFree.Size != snapshot.Size)
            return snapshot.Size;

        var activeEnd = Math.Max(2, lastFree.Index);
        tailOmitted = lastFree.Size > activeEnd;
        return tailOmitted ? activeEnd : snapshot.Size;
    }

    private static bool Intersects(long viewStart, long viewEnd, long index, long size) =>
        Math.Max(index, viewStart) < Math.Min(index + size, viewEnd);

    private readonly record struct MemoryChartLayout(
        Vec2 OverviewLabel,
        Vec2 OverviewStrip,
        Vec2 OverviewStripSize,
        Vec2 DetailLabel,
        Vec2 DetailStrip,
        Vec2 DetailStripSize);
}
