namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds centered byte-count labels over a memory strip's blocks and free spans.</summary>
[App]
public class AppMemoryStripLabels(
    RootText text,
    AppStyle s,
    AppSession session,
    AppMemoryStripGeometry geometry)
{
    public void Create(EntMut strip, AppMemoryStripView view)
    {
        const int maximumLabeledSegments = 1000;
        const float minimumSlotLabelWidth = 64f;
        const float minimumFreeLabelWidth = 76f;

        var snapshot = view.Snapshot;
        if (snapshot.Ranges.Length + snapshot.FreeSpans.Length > maximumLabeledSegments)
            return;

        if (view.DetailedLabels)
        {
            for (var i = 0; i < snapshot.Ranges.Length; i++)
            {
                var range = snapshot.Ranges[i];
                if (!geometry.Intersects(view, range.Index, range.ReservedSize))
                    continue;

                Node(strip)
                    .Mutate(s.EmphasisText)
                    .FontSizeV(s.FontSizeSmall)
                    .TextColorV(s.BlockLabelColor)
                    .TextAlignmentV(Alignment.Center)
                    .SizeRelativeV((0, 0))
                    .OffsetF(() => geometry.SegmentOffset(strip, view, range.Index))
                    .SizeF(() => geometry.SegmentSize(strip, view, range.Index, range.ReservedSize))
                    .TextF(() => range.CapacitySize == range.Size
                        ? text.Format("s{0}  {1} B", range.Slot, range.Size)
                        : text.Format("s{0}  {1} / {2} B", range.Slot, range.Size, range.CapacitySize))
                    .IsDisabledF(() =>
                        !session.ShowLabels
                        || geometry.SegmentSize(strip, view, range.Index, range.ReservedSize).X <= minimumSlotLabelWidth);
            }
        }

        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            if (!geometry.Intersects(view, span.Index, span.Size))
                continue;

            var tail = span.Index + span.Size == snapshot.Size;
            Node(strip)
                .Mutate(s.EmphasisText)
                .FontSizeV(s.FontSizeSmall)
                .TextColorV(s.FreeLabelColor)
                .TextAlignmentV(Alignment.Center)
                .SizeRelativeV((0, 0))
                .OffsetF(() => geometry.SegmentOffset(strip, view, span.Index))
                .SizeF(() => geometry.SegmentSize(strip, view, span.Index, span.Size))
                .TextF(() => tail
                    ? text.Format("tail free  {0} B", span.Size)
                    : text.Format("free  {0} B", span.Size))
                .IsDisabledF(() =>
                    !session.ShowLabels
                    || geometry.SegmentSize(strip, view, span.Index, span.Size).X <= minimumFreeLabelWidth);
        }
    }
}
