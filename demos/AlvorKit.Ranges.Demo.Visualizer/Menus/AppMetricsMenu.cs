namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the left allocator-state dock with label/value rows for the latest operation and store totals.</summary>
[App]
public class AppMetricsMenu(
    RootText text,
    AppStyle s,
    AppLayout layout,
    AppSession session)
{
    public void Create(EntMut root)
    {
        Node(root, out var dock)
            .Mutate(s.Dock)
            .Mutate(s.RightRule)
            .SizeV((layout.MetricsDockWidth, 0));
        {
            Node(dock, out var title)
                .Mutate(s.PanelTitle)
                .SizeV((0, s.Metrics.ViewportHeaderHeight))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(s.Metrics.LooseSpacing)
                .PaddingV(s.Metrics.PanelTitlePadding);
            {
                Node(title)
                    .Mutate(s.EmphasisText)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextV("Allocator State");

                Node(title)
                    .ColorV(default);

                Node(title)
                    .Mutate(s.MutedText)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextPaddingV((0, 0, s.Metrics.RightGlyphPadding, 0))
                    .TextF(() => text.Format("{0}", session.Runner.LastCommand.Kind));
            }

            Node(dock, out var body)
                .Mutate(s.Panel)
                .SizeRelativeV((1, 1))
                .PaddingV(s.Metrics.InsetPanelPadding)
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSpacingV(0);
            {
                Metric(
                    body,
                    "scenario op",
                    "scenario op\nthe last scripted command that ran",
                    () => session.Runner.LastCommand.Label);
                Metric(
                    body,
                    "method",
                    "method\nallocator API call made by the last command",
                    () => session.Runner.LastMethodText);
                Metric(
                    body,
                    "args",
                    "args\narguments passed to that allocator call",
                    () => ArgumentSummary(session.Runner.LastCommand));
                Metric(
                    body,
                    "kind",
                    "kind\ncommand family: Alloc, Realloc, Free, or Pack",
                    () => text.Format("{0}", session.Runner.LastCommand.Kind));

                Node(body)
                    .Mutate(SectionGap);

                Metric(
                    body,
                    "block slot",
                    "block slot\nhandle slot touched by the last command",
                    () => TouchedValue(range => range.Slot));
                Metric(
                    body,
                    "request B",
                    "request bytes\nsize the caller asked for in the last alloc or realloc",
                    () => RequestValue(session.Runner));
                Metric(
                    body,
                    "logical B",
                    "logical bytes\npayload size the caller can actually use",
                    () => TouchedValue(range => range.Size));
                Metric(
                    body,
                    "capacity B",
                    "capacity bytes\nusable capacity kept for the block\ncan exceed logical after a shrink",
                    () => TouchedValue(range => range.CapacitySize));
                Metric(
                    body,
                    "retained extra B",
                    "retained extra bytes\nspare capacity kept after a shrink\nreused on growth or reclaimed by pack",
                    () => TouchedValue(range => range.RetainedExtraSize));
                Metric(
                    body,
                    "reserved B",
                    "reserved bytes\ntotal footprint in the store\ncapacity plus alignment padding",
                    () => TouchedValue(range => range.ReservedSize));
                Metric(
                    body,
                    "padding B",
                    "padding bytes\nalignment overhead: reserved minus capacity",
                    () => TouchedValue(range => range.ReservedSize - range.CapacitySize));

                Node(body)
                    .Mutate(SectionGap);

                Metric(
                    body,
                    "backing size",
                    "backing size\ntotal bytes the backing store currently owns",
                    () => text.Format("{0}", session.Runner.Current.Size));
                Metric(
                    body,
                    "used",
                    "used bytes\nbytes occupied by reserved blocks\nincludes padding and retained capacity",
                    () => text.Format("{0}", session.Runner.Current.Used));
                Metric(
                    body,
                    "live ranges",
                    "live ranges\nallocations currently alive",
                    () => text.Format("{0}", session.Runner.Current.LiveCount));
                Metric(
                    body,
                    "free blocks",
                    "free blocks\ncontiguous free gaps in the store",
                    () => text.Format("{0}", session.Runner.Current.FreeBlockCount));
                Metric(
                    body,
                    "free sizes",
                    "free sizes\ndistinct block sizes in the free-size index",
                    () => text.Format("{0}", session.Runner.Current.FreeSizeCount));
                Metric(
                    body,
                    "pooled nodes",
                    "pooled nodes\nrecycled bookkeeping nodes ready for reuse",
                    () => text.Format("{0}", session.Runner.Current.PooledNodeCount));
                Metric(
                    body,
                    "packs",
                    "packs\ncompactions run so far in this scenario",
                    () => text.Format("{0}", session.Runner.Current.PackCount));
                Metric(
                    body,
                    "resizes",
                    "resizes\nbacking store growths so far in this scenario",
                    () => text.Format("{0}", session.Runner.Current.ResizeCount));
                Metric(
                    body,
                    "op ticks",
                    "op ticks\nstopwatch ticks the last allocator call took",
                    () => text.Format("{0}", session.Runner.Current.OperationTicks));
                Metric(
                    body,
                    "op managed B",
                    "op managed bytes\nmanaged heap bytes the last call allocated\nzero means garbage-free",
                    () => text.Format("{0}", session.Runner.Current.OperationManagedBytes));
            }
        }

        void Metric(EntMut parent, string name, string tooltip, Func<ReadOnlySpan<char>> value)
        {
            Node(parent, out var row)
                .Mutate(s.MetricRow)
                .IsSelectableV(true)
                .TooltipV(tooltip);
            {
                Node(row)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                    .TextV(name);

                Node(row)
                    .Mutate(s.Label)
                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                    .OffsetV((layout.MetricValueOffsetX, 0))
                    .TextF(value);
            }
        }

        void SectionGap(EntMut gap)
        {
            gap.Mutate()
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, s.Metrics.LooseSpacing));
        }

        ReadOnlySpan<char> TouchedValue<T>(Func<AllocatorRangeVisual, T> value)
        {
            if (!session.TryTouchedRange(out var range))
                return "none";

            return text.Format("{0}", value(range));
        }

        ReadOnlySpan<char> RequestValue(AllocatorScenarioRunner runner) =>
            runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc
                ? text.Format("{0}", runner.LastCommand.Size)
                : "none";

        ReadOnlySpan<char> ArgumentSummary(AllocatorCommand command) => command.Kind switch
        {
            AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc =>
                text.Format("slot {0}, align {1}, size {2}", command.Slot, command.Alignment, command.Size),
            AllocatorCommandKind.Free => text.Format("slot {0}", command.Slot),
            AllocatorCommandKind.Pack => "none",
            _ => "scenario not stepped",
        };
    }
}
