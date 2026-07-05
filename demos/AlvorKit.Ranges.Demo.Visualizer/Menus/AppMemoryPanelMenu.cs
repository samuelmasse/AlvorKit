namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryPanelMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppMemoryChartsMenu memoryChartsMenu)
{
    public void Create(EntMut root)
    {
        const float headerHeight = 40f;
        const float headerStatsOffsetY = 22f;

        Node(root, out var panel)
            .Mutate(s.PanelList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .SizeRelativeV((1, 1));
        {
            Node(panel, out var header)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, headerHeight));
            {
                Node(header)
                    .Mutate(s.Heading)
                    .TextV("backing store")
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(header)
                    .Mutate(s.MutedLabel)
                    .FontSizeV(s.FontSizeBody)
                    .TextF(() => text.Format(
                        "size {0}, used {1}, free spans {2}",
                        session.Runner.Current.Size,
                        session.Runner.Current.Used,
                        session.Runner.Current.FreeSpans.Length))
                    .OffsetV((0, headerStatsOffsetY))
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(header)
                    .Mutate(s.MutedLabel)
                    .FontSizeV(s.FontSizeBody)
                    .TextF(() => text.Format("last allocator call: {0}", session.Runner.LastCallText))
                    .OffsetV((-s.FloatingTextInset, 0))
                    .AlignmentV(Alignment.Right | Alignment.Top);
            }

            Node(panel, out var legend)
                .Mutate(s.HorizontalList)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                for (var i = 0; i < 5; i++)
                    LegendItem(legend, i);
            }

            Node(panel, out var modeRow)
                .Mutate(s.HorizontalList)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                ButtonF(modeRow, 176f, () => text.Format("memory {0}", MemoryModeName(session.MemoryOverlayMode)), session.NextMemoryOverlayMode);
                MetricLabel(modeRow, 160f, () => text.Format("used/backing {0:0.0}%", UsedRatio() * 100));
                MetricLabel(modeRow, 178f, () => text.Format("payload/backing {0:0.0}%", PayloadBackingRatio() * 100));
                MetricLabel(modeRow, 184f, () => text.Format("payload/reserved {0:0.0}%", PayloadReservedRatio() * 100));
            }

            Node(panel, out var aggregateRow)
                .Mutate(s.HorizontalList)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                MetricLabel(aggregateRow, 142f, () => text.Format("free blocks {0}", session.Runner.Current.FreeBlockCount));
                MetricLabel(aggregateRow, 158f, () => text.Format("largest free {0}", LargestFreeSpan()));
                MetricLabel(aggregateRow, 160f, () => text.Format("external frag {0:0.0}%", ExternalFragmentationRatio() * 100));
                MetricLabel(aggregateRow, 132f, () => text.Format("outliers {0}", session.OutlierSlotCount));
            }

            Node(panel, out var chartsSlot)
                .SizeRelativeV((1, 0))
                .ColorV(s.PanelInsetColor);
            {
                memoryChartsMenu.Create(chartsSlot);
            }
        }

        void LegendItem(EntMut parent, int index)
        {
            Node(parent, out var item)
                .Mutate(s.HorizontalList)
                .InnerSpacingV(s.SpacingXS)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                Node(item)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.SwatchWidth, s.SwatchHeight))
                    .AlignmentV(Alignment.Vertical)
                    .ColorF(() => MemoryLegendColor(index));
                Node(item)
                    .Mutate(s.MutedLabel)
                    .TextF(() => MemoryLegendText(index));
            }
        }

        void ButtonF(EntMut parent, float width, Func<ReadOnlySpan<char>> label, Action action)
        {
            Node(parent)
                .Mutate(s.Button)
                .SizeV((width, s.ButtonHeight))
                .TextF(label)
                .OnPressF(action);
        }

        void MetricLabel(EntMut parent, float width, Func<ReadOnlySpan<char>> value)
        {
            Node(parent)
                .Mutate(s.MutedLabel)
                .FontSizeV(s.FontSizeBody)
                .SizeV((width, s.ButtonHeight))
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
                0 => s.FreeBlockColor,
                1 => s.AllocationColor(0),
                2 => s.RetainedColor,
                3 => s.PaddingColor,
                _ => s.LatestRequestFillColor,
            },
            AppMemoryOverlayMode.Occupancy => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.OccupancyReservedColor,
                2 => s.OccupancyPayloadColor,
                3 => s.DensityHighColor,
                _ => s.HighlightColor,
            },
            AppMemoryOverlayMode.Density => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.DensityLowColor,
                2 => s.DensityMidColor,
                3 => s.DensityHighColor,
                _ => default,
            },
            AppMemoryOverlayMode.Efficiency => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.EfficiencyWasteColor,
                2 => s.EfficiencyMixedColor,
                3 => s.EfficiencyGoodColor,
                _ => default,
            },
            AppMemoryOverlayMode.Fragmentation => index switch
            {
                0 => s.OverlayOccupiedColor,
                1 => s.FragmentTinyColor,
                2 => s.FragmentMediumColor,
                3 => s.FragmentLargeColor,
                _ => s.TailFreeBlockColor,
            },
            AppMemoryOverlayMode.Slack => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.AllocationColor(0),
                2 => s.RetainedColor,
                3 => s.PaddingColor,
                _ => s.LatestRequestFillColor,
            },
            AppMemoryOverlayMode.Churn => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.ChurnIdleColor,
                2 => s.ChurnRecentColor,
                3 => s.HighlightColor,
                _ => default,
            },
            AppMemoryOverlayMode.Outliers => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.OverlayOccupiedColor,
                2 => s.OutlierColor,
                3 => s.DensityHighColor,
                _ => default,
            },
            AppMemoryOverlayMode.Relocation => index switch
            {
                0 => s.OverlayFreeColor,
                1 => s.RelocationReusedColor,
                2 => s.RelocationMovedColor,
                3 => s.RelocationNewColor,
                _ => s.OverlayOccupiedColor,
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
