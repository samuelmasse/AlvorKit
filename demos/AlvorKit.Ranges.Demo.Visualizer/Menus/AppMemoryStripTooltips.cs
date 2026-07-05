namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Formats hovered strip regions as tooltip text: a title line plus muted detail lines.</summary>
[App]
public class AppMemoryStripTooltips(
    RootText text,
    AppSession session)
{
    public ReadOnlySpan<char> Free(AppMemoryStripView view, long index, long size, bool mutedTail) =>
        mutedTail
            ? text.Format(
                "tail free block\n{0} B at byte {1}\nomitted from the active zoom",
                size,
                index)
            : text.Format(
                "free block\n{0} B at byte {1}\nreusable for a fitting request",
                size,
                index);

    public ReadOnlySpan<char> Padding(AppMemoryStripView view, AllocatorRangeVisual range, long size, bool leading) =>
        leading
            ? text.Format(
                "leading padding, slot {0}\n{1} B from alignment {2}\npayload starts at byte {3}",
                range.Slot,
                size,
                range.Alignment,
                range.PayloadIndex)
            : text.Format(
                "trailing padding, slot {0}\n{1} B reserved for alignment {2}\nup to {3} extra bytes can be needed",
                range.Slot,
                size,
                range.Alignment,
                range.Alignment - 1);

    public ReadOnlySpan<char> Retained(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "retained capacity, slot {0}\n{1} B kept after a shrink\nreused on growth or freed by pack",
            range.Slot,
            range.RetainedExtraSize);

    public ReadOnlySpan<char> Payload(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "slot {0} payload\nlogical {1} B, capacity {2} B\nreserved {3} B at byte {4}, alignment {5}",
            range.Slot,
            range.Size,
            range.CapacitySize,
            range.ReservedSize,
            range.Index,
            range.Alignment);

    public ReadOnlySpan<char> LatestRequest(AppMemoryStripView view, AllocatorRangeVisual range) =>
        text.Format(
            "slot {0}, latest {1}\nlogical {2} B, capacity {3} B\nreserved {4} B, padding {5} B, retained {6} B\nrequest from event {7}",
            range.Slot,
            session.Runner.LastCommand.Kind,
            range.Size,
            range.CapacitySize,
            range.ReservedSize,
            range.ReservedSize - range.CapacitySize,
            range.RetainedExtraSize,
            session.Runner.StepIndex);
}
