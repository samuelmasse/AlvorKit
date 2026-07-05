namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryStripLabels(
    RootText text,
    AppStyle s,
    AppSession session,
    AppMemoryStripGeometry geometry)
{
    public void Create(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
    {
        if (!geometry.Intersects(view, range.Index, range.ReservedSize))
            return;

        SlotLabel(strip, view, range);

        if (!view.DetailedLabels)
            return;

        RequestLabel(strip, view, range);
        CapacityLabel(strip, view, range);

        void SlotLabel(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
        {
            const float labelOffsetX = 4f;
            const float slotLabelOffsetY = 3f;
            const float minimumSlotLabelWidth = 28f;

            Node(strip)
                .Mutate(s.Label)
                .FontSizeV(s.FontSizeSmall)
                .TextV(text.Format("#{0}", range.Slot).ToString())
                .OffsetF(() => geometry.SegmentOffset(strip, view, range.Index) + (labelOffsetX, slotLabelOffsetY))
                .IsDisabledF(() =>
                    !session.ShowLabels
                    || geometry.SegmentSize(strip, view, range.Index, range.ReservedSize).X <= minimumSlotLabelWidth);
        }

        void RequestLabel(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
        {
            const float labelOffsetX = 4f;
            const float detailLabelOffsetY = 18f;

            Node(strip)
                .Mutate(s.MutedLabel)
                .TextF(() => text.Format("{0}B requested", range.Size))
                .OffsetF(() => geometry.SegmentOffset(strip, view, range.Index) + (labelOffsetX, detailLabelOffsetY))
                .IsDisabledF(() => !session.ShowLabels || !CanFitDetailLabel(strip, view, range));
        }

        void CapacityLabel(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
        {
            const float labelOffsetX = 4f;
            const float extendedLabelOffsetY = 32f;

            Node(strip)
                .Mutate(s.MutedLabel)
                .TextF(() => range.RetainedExtraSize == 0
                    ? text.Format("{0}B capacity", range.CapacitySize)
                    : text.Format("{0}B capacity, {1}B retained", range.CapacitySize, range.RetainedExtraSize))
                .OffsetF(() => geometry.SegmentOffset(strip, view, range.Index) + (labelOffsetX, extendedLabelOffsetY))
                .IsDisabledF(() => !session.ShowLabels || !CanFitExtendedLabel(strip, view, range));
        }

        bool CanFitDetailLabel(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
        {
            const float minimumDetailLabelWidth = 74f;
            const float minimumDetailLabelHeight = 38f;

            var segment = geometry.SegmentSize(strip, view, range.Index, range.ReservedSize);
            return segment.X > minimumDetailLabelWidth && segment.Y > minimumDetailLabelHeight;
        }

        bool CanFitExtendedLabel(EntMut strip, AppMemoryStripView view, AllocatorRangeVisual range)
        {
            const float minimumExtendedLabelWidth = 116f;
            const float minimumExtendedLabelHeight = 58f;

            var segment = geometry.SegmentSize(strip, view, range.Index, range.ReservedSize);
            return segment.X > minimumExtendedLabelWidth && segment.Y > minimumExtendedLabelHeight;
        }
    }
}
