namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryChartsMenu(
    RootText text,
    AppStyle style,
    AppSession session,
    AppMemoryStripMenu stripMenu)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        var lastRevision = pendingRevision;
        Node(root, out var content)
            .SizeRelativeV((1, 1))
            .ColorV(style.PanelInsetColor)
            .OnUpdateF(() =>
            {
                if (lastRevision == session.VisualRevision)
                    return;

                lastRevision = session.VisualRevision;
                NodesClear(content);
                BuildMemoryCharts(content);
            });

        void BuildMemoryCharts(EntMut root)
        {
            var snapshot = session.Runner.Current;
            var detailEnd = DetailEnd(snapshot, out var tailOmitted);

            Node(root)
                .Mutate(style.MutedLabel)
                .TextV("full backing store")
                .OffsetF(() => MemoryLayout(root).OverviewLabel);

            Node(root, out var overview)
                .Mutate(ChartStrip)
                .TooltipF(() => StoreTooltip("full backing store", snapshot))
                .OffsetF(() => MemoryLayout(root).OverviewStrip)
                .SizeF(() => MemoryLayout(root).OverviewStripSize)
                .Mutate(node => stripMenu.Create(
                    node,
                    new(snapshot, 0, snapshot.Size, "full backing store", MuteTail: true, DetailedLabels: false)));

            Node(root)
                .Mutate(style.MutedLabel)
                .TextF(() => DetailLabel(detailEnd, tailOmitted))
                .OffsetF(() => MemoryLayout(root).DetailLabel);

            Node(root, out var detail)
                .Mutate(ChartStrip)
                .TooltipF(() => DetailTooltip(detailEnd, tailOmitted))
                .OffsetF(() => MemoryLayout(root).DetailStrip)
                .SizeF(() => MemoryLayout(root).DetailStripSize)
                .Mutate(node => stripMenu.Create(
                    node,
                    new(snapshot, 1, detailEnd, "active region zoom", MuteTail: false, DetailedLabels: true)));
        }

        void ChartStrip(EntMut ent) => ent.Mutate()
            .SizeRelativeV((0, 0))
            .IsSelectableV(true)
            .ColorV(style.PanelInsetColor)
            .InnerAlignmentSnapV(1f);

        ReadOnlySpan<char> StoreTooltip(string viewName, AllocatorSnapshot snapshot) =>
            text.Format(
                "{0}: {1} bytes total, {2} used, {3} live ranges; hover colored spans for exact details",
                viewName,
                snapshot.Size,
                snapshot.Used,
                snapshot.LiveCount);

        ReadOnlySpan<char> DetailTooltip(long detailEnd, bool tailOmitted) =>
            tailOmitted
                ? text.Format("active region zoom: bytes 1..{0}; large empty tail is omitted so live ranges are readable", detailEnd)
                : text.Format("active region zoom: bytes 1..{0}; this scenario currently fits in the detailed view", detailEnd);

        (Vec2 OverviewLabel, Vec2 OverviewStrip, Vec2 OverviewStripSize, Vec2 DetailLabel, Vec2 DetailStrip, Vec2 DetailStripSize)
            MemoryLayout(EntMut root)
        {
            const float detailGap = 28f;
            const float detailHeightRatio = 0.34f;
            const float maximumDetailHeight = 126f;
            const float minimumDetailHeight = 54f;
            const float minimumOverviewHeight = 48f;
            const float compressedMinimumDetailHeight = 36f;
            const float detailLabelGap = 12f;

            var inset = style.SpacingS;
            var labelHeight = style.MetricRowHeight;
            var width = Math.Max(0, root.SizeR.X - inset - inset);
            var height = Math.Max(0, root.SizeR.Y - inset - inset);
            var detailHeight = Math.Min(maximumDetailHeight, Math.Max(minimumDetailHeight, height * detailHeightRatio));
            var overviewHeight = Math.Max(minimumOverviewHeight, height - detailHeight - detailGap - labelHeight - labelHeight);
            var total = overviewHeight + detailHeight + detailGap + labelHeight + labelHeight;
            if (total > height)
            {
                var overflow = total - height;
                detailHeight = Math.Max(compressedMinimumDetailHeight, detailHeight - overflow);
            }

            var overviewLabel = new Vec2(inset, inset);
            var overviewStrip = overviewLabel + (0, labelHeight);
            var detailLabel = overviewStrip + (0, overviewHeight + detailLabelGap);
            var detailStrip = detailLabel + (0, labelHeight);
            var detailBottom = Math.Max(compressedMinimumDetailHeight, root.SizeR.Y - inset - detailStrip.Y);
            return (
                overviewLabel,
                overviewStrip,
                (width, overviewHeight),
                detailLabel,
                detailStrip,
                (width, detailBottom));
        }

        ReadOnlySpan<char> DetailLabel(long detailEnd, bool tailOmitted) =>
            tailOmitted
                ? text.Format("active region zoom: bytes 1..{0}; large tail free block is omitted here", detailEnd)
                : text.Format("active region zoom: bytes 1..{0}; full usable range", detailEnd);

        long DetailEnd(AllocatorSnapshot snapshot, out bool tailOmitted)
        {
            const long minimumActiveEnd = 2;

            tailOmitted = false;
            if (snapshot.Ranges.Length == 0 || snapshot.FreeSpans.Length == 0)
                return snapshot.Size;

            var lastFree = snapshot.FreeSpans[^1];
            if (lastFree.Index + lastFree.Size != snapshot.Size)
                return snapshot.Size;

            var activeEnd = Math.Max(minimumActiveEnd, lastFree.Index);
            tailOmitted = lastFree.Size > activeEnd;
            return tailOmitted ? activeEnd : snapshot.Size;
        }
    }
}
