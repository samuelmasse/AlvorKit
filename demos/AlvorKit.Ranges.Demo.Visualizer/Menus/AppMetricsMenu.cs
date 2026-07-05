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
                Metric(body, "scenario op", () => session.Runner.LastCommand.Label);
                Metric(body, "method", () => session.Runner.LastMethodText);
                Metric(body, "args", () => ArgumentSummary(session.Runner.LastCommand));
                Metric(body, "kind", () => text.Format("{0}", session.Runner.LastCommand.Kind));

                Node(body)
                    .Mutate(SectionGap);

                Metric(body, "block slot", () => TouchedValue(range => range.Slot));
                Metric(body, "request B", () => RequestValue(session.Runner));
                Metric(body, "logical B", () => TouchedValue(range => range.Size));
                Metric(body, "capacity B", () => TouchedValue(range => range.CapacitySize));
                Metric(body, "retained extra B", () => TouchedValue(range => range.RetainedExtraSize));
                Metric(body, "reserved B", () => TouchedValue(range => range.ReservedSize));
                Metric(body, "padding B", () => TouchedValue(range => range.ReservedSize - range.CapacitySize));

                Node(body)
                    .Mutate(SectionGap);

                Metric(body, "backing size", () => text.Format("{0}", session.Runner.Current.Size));
                Metric(body, "used", () => text.Format("{0}", session.Runner.Current.Used));
                Metric(body, "live ranges", () => text.Format("{0}", session.Runner.Current.LiveCount));
                Metric(body, "free blocks", () => text.Format("{0}", session.Runner.Current.FreeBlockCount));
                Metric(body, "free sizes", () => text.Format("{0}", session.Runner.Current.FreeSizeCount));
                Metric(body, "pooled nodes", () => text.Format("{0}", session.Runner.Current.PooledNodeCount));
                Metric(body, "packs", () => text.Format("{0}", session.Runner.Current.PackCount));
                Metric(body, "resizes", () => text.Format("{0}", session.Runner.Current.ResizeCount));
                Metric(body, "op ticks", () => text.Format("{0}", session.Runner.Current.OperationTicks));
                Metric(body, "op managed B", () => text.Format("{0}", session.Runner.Current.OperationManagedBytes));
            }
        }

        void Metric(EntMut parent, string name, Func<ReadOnlySpan<char>> value)
        {
            Node(parent, out var row)
                .Mutate(s.MetricRow);
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
