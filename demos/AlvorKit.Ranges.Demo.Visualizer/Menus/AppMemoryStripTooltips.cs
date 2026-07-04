namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryStripTooltips(
    RootText text,
    AppSession session)
{
    public ReadOnlySpan<char> Free(AppMemoryStripView view, long index, long size, bool mutedTail) =>
        mutedTail
            ? text.Format(
                "{0}: tail free block, {1} bytes at byte {2}; muted because the active zoom omits that empty tail",
                view.ViewName,
                size,
                index)
            : text.Format(
                "{0}: free block, {1} bytes at byte {2}; allocator can reuse this gap for a fitting request",
                view.ViewName,
                size,
                index);

    public ReadOnlySpan<char> Padding(AppMemoryStripView view, AllocatorRangeVisual range, long size, bool leading) =>
        leading
            ? text.Format(
                "{0}: leading padding of alloc #{1}, {2} bytes; alignment {3} moves payload start to byte {4}",
                view.ViewName,
                range.Slot,
                size,
                range.Alignment,
                range.PayloadIndex)
            : text.Format(
                "{0}: trailing padding of alloc #{1}, {2} bytes; reserved because alignment can need up to {3} extra bytes",
                view.ViewName,
                range.Slot,
                size,
                range.Alignment - 1);

    public ReadOnlySpan<char> Retained(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "{0}: retained capacity of alloc #{1}, {2} bytes; a shrink kept spare capacity until growth or pack",
            view.ViewName,
            range.Slot,
            range.RetainedExtraSize);

    public ReadOnlySpan<char> Payload(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "{0}: alloc #{1} payload, {2} bytes at byte {3}; capacity {4} bytes, alignment {5}",
            view.ViewName,
            range.Slot,
            range.Size,
            range.PayloadIndex,
            range.CapacitySize,
            range.Alignment);

    public ReadOnlySpan<char> LatestRequest(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "{0}: latest request touched alloc #{1}, {2} bytes from event #{3}; white overlay marks the allocator call result",
            view.ViewName,
            range.Slot,
            session.Runner.LastCommand.Size,
            session.Runner.StepIndex);
}
