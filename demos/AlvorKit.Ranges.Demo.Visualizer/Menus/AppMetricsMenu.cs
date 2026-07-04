namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMetricsMenu(
    RootText text,
    AppStyle style,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const float metricsWidth = 320f;

        var metricsSectionGap = style.SpacingXS;
        root.Mutate(style.PanelList)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((0, 1))
            .SizeV((metricsWidth, 0));

        Node(root)
            .Mutate(style.Heading)
            .SizeWeightTypeV(SizeWeightType.Self)
            .TextV("allocator state");

        Metric(root, "scenario op", () => session.Runner.LastCommand.Label);
        Metric(root, "method", () => session.Runner.LastMethodText);
        Metric(root, "args", () => ArgumentSummary(session.Runner.LastCommand));
        Metric(root, "kind", () => text.Format("{0}", session.Runner.LastCommand.Kind));

        Spacer(root, metricsSectionGap);
        Metric(root, "block slot", () => TouchedValue(range => range.Slot));
        Metric(root, "request B", () => RequestValue(session.Runner));
        Metric(root, "logical B", () => TouchedValue(range => range.Size));
        Metric(root, "capacity B", () => TouchedValue(range => range.CapacitySize));
        Metric(root, "retained extra B", () => TouchedValue(range => range.RetainedExtraSize));
        Metric(root, "reserved B", () => TouchedValue(range => range.ReservedSize));
        Metric(root, "padding B", () => TouchedValue(range => range.ReservedSize - range.CapacitySize));

        Spacer(root, metricsSectionGap);
        Metric(root, "backing size", () => text.Format("{0}", session.Runner.Current.Size));
        Metric(root, "used", () => text.Format("{0}", session.Runner.Current.Used));
        Metric(root, "live ranges", () => text.Format("{0}", session.Runner.Current.LiveCount));
        Metric(root, "free blocks", () => text.Format("{0}", session.Runner.Current.FreeBlockCount));
        Metric(root, "free sizes", () => text.Format("{0}", session.Runner.Current.FreeSizeCount));
        Metric(root, "pooled nodes", () => text.Format("{0}", session.Runner.Current.PooledNodeCount));
        Metric(root, "packs", () => text.Format("{0}", session.Runner.Current.PackCount));
        Metric(root, "resizes", () => text.Format("{0}", session.Runner.Current.ResizeCount));
        Metric(root, "op ticks", () => text.Format("{0}", session.Runner.Current.OperationTicks));
        Metric(root, "op managed B", () => text.Format("{0}", session.Runner.Current.OperationManagedBytes));

        void Metric(EntMut parent, string name, Func<ReadOnlySpan<char>> value)
        {
            const float valueOffsetX = 116f;

            Node(parent, out var row)
                .Mutate(style.MetricRow)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                Node(row)
                    .Mutate(style.MutedLabel)
                    .TextV(name)
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(row)
                    .Mutate(style.Label)
                    .FontSizeV(style.FontSizeSmall)
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
