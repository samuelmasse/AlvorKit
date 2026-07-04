namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryPanelMenu(
    RootText text,
    AppStyle style,
    AppSession session,
    AppMemoryChartsMenu memoryChartsMenu)
{
    public void Create(EntMut root)
    {
        const float headerHeight = 40f;
        const float headerStatsOffsetY = 22f;

        root.Mutate(style.PanelList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .SizeRelativeV((1, 1));

        Node(root, out var header)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, headerHeight));
        {
            Node(header)
                .Mutate(style.Heading)
                .TextV("backing store")
                .AlignmentV(Alignment.Left | Alignment.Top);

            Node(header)
                .Mutate(style.MutedLabel)
                .FontSizeV(style.FontSizeBody)
                .TextF(() => text.Format(
                    "size {0}, used {1}, free spans {2}",
                    session.Runner.Current.Size,
                    session.Runner.Current.Used,
                    session.Runner.Current.FreeSpans.Length))
                .OffsetV((0, headerStatsOffsetY))
                .AlignmentV(Alignment.Left | Alignment.Top);

            Node(header)
                .Mutate(style.MutedLabel)
                .FontSizeV(style.FontSizeBody)
                .TextF(() => text.Format("last allocator call: {0}", session.Runner.LastCallText))
                .OffsetV((-style.FloatingTextInset, 0))
                .AlignmentV(Alignment.Right | Alignment.Top);
        }

        Node(root, out var legend)
            .Mutate(style.HorizontalList)
            .SizeWeightTypeV(SizeWeightType.Self);
        {
            for (var i = 0; i < 5; i++)
                LegendItem(legend, i);
        }

        Node(root, out var modeRow)
            .Mutate(style.HorizontalList)
            .SizeWeightTypeV(SizeWeightType.Self);
        {
            ButtonF(modeRow, 176f, () => text.Format("memory {0}", MemoryModeName(session.MemoryOverlayMode)), session.NextMemoryOverlayMode);
            MetricLabel(modeRow, 160f, () => text.Format("used/backing {0:0.0}%", UsedRatio() * 100));
            MetricLabel(modeRow, 178f, () => text.Format("payload/backing {0:0.0}%", PayloadBackingRatio() * 100));
            MetricLabel(modeRow, 184f, () => text.Format("payload/reserved {0:0.0}%", PayloadReservedRatio() * 100));
        }

        Node(root, out var aggregateRow)
            .Mutate(style.HorizontalList)
            .SizeWeightTypeV(SizeWeightType.Self);
        {
            MetricLabel(aggregateRow, 142f, () => text.Format("free blocks {0}", session.Runner.Current.FreeBlockCount));
            MetricLabel(aggregateRow, 158f, () => text.Format("largest free {0}", LargestFreeSpan()));
            MetricLabel(aggregateRow, 160f, () => text.Format("external frag {0:0.0}%", ExternalFragmentationRatio() * 100));
            MetricLabel(aggregateRow, 132f, () => text.Format("outliers {0}", session.OutlierSlotCount));
        }

        Node(root)
            .SizeRelativeV((1, 0))
            .ColorV(style.PanelInsetColor)
            .Mutate(memoryChartsMenu.Create);

        void LegendItem(EntMut parent, int index)
        {
            Node(parent, out var item)
                .Mutate(style.HorizontalList)
                .InnerSpacingV(style.SpacingXS)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                Node(item)
                    .SizeRelativeV((0, 0))
                    .SizeV((style.SwatchWidth, style.SwatchHeight))
                    .AlignmentV(Alignment.Vertical)
                    .ColorF(() => MemoryLegendColor(index));
                Node(item)
                    .Mutate(style.MutedLabel)
                    .TextF(() => MemoryLegendText(index));
            }
        }

        void ButtonF(EntMut parent, float width, Func<ReadOnlySpan<char>> label, Action action)
        {
            Node(parent)
                .Mutate(style.Button)
                .SizeV((width, style.ButtonHeight))
                .TextF(label)
                .OnPressF(action);
        }

        void MetricLabel(EntMut parent, float width, Func<ReadOnlySpan<char>> value)
        {
            Node(parent)
                .Mutate(style.MutedLabel)
                .FontSizeV(style.FontSizeBody)
                .SizeV((width, style.ButtonHeight))
                .SizeTextRelativeV((0, 0))
                .TextAlignmentV(Alignment.Center)
                .TextF(value)
                .AlignmentV(Alignment.Vertical);
        }

        string MemoryModeName(AppMemoryOverlayMode mode) => mode switch
        {
            AppMemoryOverlayMode.Allocations => "allocations",
            AppMemoryOverlayMode.Occupancy => "occupancy",
            AppMemoryOverlayMode.Density => "density",
            AppMemoryOverlayMode.Efficiency => "efficiency",
            AppMemoryOverlayMode.Fragmentation => "fragmentation",
            AppMemoryOverlayMode.Slack => "slack",
            AppMemoryOverlayMode.Churn => "churn",
            AppMemoryOverlayMode.Outliers => "outliers",
            AppMemoryOverlayMode.Relocation => "relocation",
            _ => "memory",
        };

        Vec4 MemoryLegendColor(int index) => session.MemoryOverlayMode switch
        {
            AppMemoryOverlayMode.Allocations => index switch
            {
                0 => style.FreeBlockColor,
                1 => style.AllocationColor(0),
                2 => style.RetainedColor,
                3 => style.PaddingColor,
                _ => style.LatestRequestFillColor,
            },
            AppMemoryOverlayMode.Occupancy => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.OccupancyReservedColor,
                2 => style.OccupancyPayloadColor,
                3 => style.DensityHighColor,
                _ => style.HighlightColor,
            },
            AppMemoryOverlayMode.Density => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.DensityLowColor,
                2 => style.DensityMidColor,
                3 => style.DensityHighColor,
                _ => default,
            },
            AppMemoryOverlayMode.Efficiency => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.EfficiencyWasteColor,
                2 => style.EfficiencyMixedColor,
                3 => style.EfficiencyGoodColor,
                _ => default,
            },
            AppMemoryOverlayMode.Fragmentation => index switch
            {
                0 => style.OverlayOccupiedColor,
                1 => style.FragmentTinyColor,
                2 => style.FragmentMediumColor,
                3 => style.FragmentLargeColor,
                _ => style.TailFreeBlockColor,
            },
            AppMemoryOverlayMode.Slack => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.AllocationColor(0),
                2 => style.RetainedColor,
                3 => style.PaddingColor,
                _ => style.LatestRequestFillColor,
            },
            AppMemoryOverlayMode.Churn => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.ChurnIdleColor,
                2 => style.ChurnRecentColor,
                3 => style.HighlightColor,
                _ => default,
            },
            AppMemoryOverlayMode.Outliers => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.OverlayOccupiedColor,
                2 => style.OutlierColor,
                3 => style.DensityHighColor,
                _ => default,
            },
            AppMemoryOverlayMode.Relocation => index switch
            {
                0 => style.OverlayFreeColor,
                1 => style.RelocationReusedColor,
                2 => style.RelocationMovedColor,
                3 => style.RelocationNewColor,
                _ => style.OverlayOccupiedColor,
            },
            _ => default,
        };

        string MemoryLegendText(int index) => session.MemoryOverlayMode switch
        {
            AppMemoryOverlayMode.Allocations => index switch
            {
                0 => "free block",
                1 => "live payload",
                2 => "retained",
                3 => "padding",
                _ => "latest request",
            },
            AppMemoryOverlayMode.Occupancy => index switch
            {
                0 => "free",
                1 => "reserved",
                2 => "payload",
                3 => "dense",
                _ => "active",
            },
            AppMemoryOverlayMode.Density => index switch
            {
                0 => "empty",
                1 => "low",
                2 => "mixed",
                3 => "full",
                _ => "",
            },
            AppMemoryOverlayMode.Efficiency => index switch
            {
                0 => "free",
                1 => "waste",
                2 => "mixed",
                3 => "efficient",
                _ => "",
            },
            AppMemoryOverlayMode.Fragmentation => index switch
            {
                0 => "occupied",
                1 => "tiny holes",
                2 => "medium holes",
                3 => "large holes",
                _ => "tail",
            },
            AppMemoryOverlayMode.Slack => index switch
            {
                0 => "free",
                1 => "payload",
                2 => "retained",
                3 => "padding",
                _ => "latest",
            },
            AppMemoryOverlayMode.Churn => index switch
            {
                0 => "free",
                1 => "idle live",
                2 => "recent",
                3 => "latest",
                _ => "",
            },
            AppMemoryOverlayMode.Outliers => index switch
            {
                0 => "free",
                1 => "normal",
                2 => "outlier",
                3 => "severe",
                _ => "",
            },
            AppMemoryOverlayMode.Relocation => index switch
            {
                0 => "free",
                1 => "reused",
                2 => "moved",
                3 => "new",
                _ => "unchanged",
            },
            _ => "",
        };

        double UsedRatio() => Ratio(session.Runner.Current.Used, session.Runner.Current.Size);

        double PayloadBackingRatio() => Ratio(PayloadBytes(), session.Runner.Current.Size);

        double PayloadReservedRatio() => Ratio(PayloadBytes(), ReservedBytes());

        double ExternalFragmentationRatio()
        {
            var freeBytes = FreeBytes();
            return freeBytes <= 0 ? 0 : 1.0 - Ratio(LargestFreeSpan(), freeBytes);
        }

        long PayloadBytes()
        {
            var total = 0L;
            var ranges = session.Runner.Current.Ranges;
            for (var i = 0; i < ranges.Length; i++)
                total += ranges[i].Size;

            return total;
        }

        long ReservedBytes()
        {
            var total = 0L;
            var ranges = session.Runner.Current.Ranges;
            for (var i = 0; i < ranges.Length; i++)
                total += ranges[i].ReservedSize;

            return total;
        }

        long FreeBytes()
        {
            var total = 0L;
            var spans = session.Runner.Current.FreeSpans;
            for (var i = 0; i < spans.Length; i++)
                total += spans[i].Size;

            return total;
        }

        long LargestFreeSpan()
        {
            var largest = 0L;
            var spans = session.Runner.Current.FreeSpans;
            for (var i = 0; i < spans.Length; i++)
                largest = Math.Max(largest, spans[i].Size);

            return largest;
        }

        double Ratio(double numerator, double denominator) => denominator <= 0 ? 0 : numerator / denominator;
    }
}
