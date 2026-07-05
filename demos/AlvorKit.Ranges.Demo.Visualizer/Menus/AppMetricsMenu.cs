namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMetricsMenu(
    RootText text,
    AppStyle s,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const float metricsWidth = 320f;

        var metricsSectionGap = s.SpacingXS;
        Node(root, out var panel)
            .Mutate(s.PanelList)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((0, 1))
            .SizeV((metricsWidth, 0));
        {
            Node(panel)
                .Mutate(s.Heading)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("allocator state");

            Metric(panel, "scenario op", () => session.Runner.LastCommand.Label);
            Metric(panel, "method", () => session.Runner.LastMethodText);
            Metric(panel, "args", () => ArgumentSummary(session.Runner.LastCommand));
            Metric(panel, "kind", () => text.Format("{0}", session.Runner.LastCommand.Kind));

            Spacer(panel, metricsSectionGap);
            Metric(panel, "block slot", () => TouchedValue(range => range.Slot));
            Metric(panel, "request B", () => RequestValue(session.Runner));
            Metric(panel, "logical B", () => TouchedValue(range => range.Size));
            Metric(panel, "capacity B", () => TouchedValue(range => range.CapacitySize));
            Metric(panel, "retained extra B", () => TouchedValue(range => range.RetainedExtraSize));
            Metric(panel, "reserved B", () => TouchedValue(range => range.ReservedSize));
            Metric(panel, "padding B", () => TouchedValue(range => range.ReservedSize - range.CapacitySize));

            Spacer(panel, metricsSectionGap);
            Metric(panel, "backing size", () => text.Format("{0}", session.Runner.Current.Size));
            Metric(panel, "used", () => text.Format("{0}", session.Runner.Current.Used));
            Metric(panel, "live ranges", () => text.Format("{0}", session.Runner.Current.LiveCount));
            Metric(panel, "free blocks", () => text.Format("{0}", session.Runner.Current.FreeBlockCount));
            Metric(panel, "free sizes", () => text.Format("{0}", session.Runner.Current.FreeSizeCount));
            Metric(panel, "pooled nodes", () => text.Format("{0}", session.Runner.Current.PooledNodeCount));
            Metric(panel, "packs", () => text.Format("{0}", session.Runner.Current.PackCount));
            Metric(panel, "resizes", () => text.Format("{0}", session.Runner.Current.ResizeCount));
            Metric(panel, "op ticks", () => text.Format("{0}", session.Runner.Current.OperationTicks));
            Metric(panel, "op managed B", () => text.Format("{0}", session.Runner.Current.OperationManagedBytes));
        }

        void Metric(EntMut parent, string name, Func<ReadOnlySpan<char>> value)
        {
            const float valueOffsetX = 116f;

            Node(parent, out var row)
                .Mutate(s.MetricRow)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                Node(row)
                    .Mutate(s.MutedLabel)
                    .TextV(name)
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(row)
                    .Mutate(s.Label)
                    .FontSizeV(s.FontSizeSmall)
                    .TextF(value)
                    .OffsetV((valueOffsetX, 0))
                    .AlignmentV(Alignment.Left | Alignment.Top);
            }
        }

        void Spacer(EntMut parent, float height)
        {
            Node(parent)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, height));
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
