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
        const int legendEntryCount = 5;

        Node(root, out var viewport)
            .Mutate(s.PanelFillList);
        {
            Node(viewport, out var header)
                .Mutate(s.HeaderStrip)
                .SizeV((0, s.Metrics.ViewportHeaderHeight))
                .InnerSpacingV(s.Metrics.ToolbarSpacing)
                .PaddingV(s.Metrics.ViewportHeaderPadding);
            {
                Node(header)
                    .Mutate(s.Chip)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextF(() => text.Format("memory: {0}", MemoryModeName(session.MemoryOverlayMode)))
                    .OnPressF(session.NextMemoryOverlayMode);

                for (var i = 0; i < legendEntryCount; i++)
                    LegendItem(header, i);

                Node(header)
                    .ColorV(default);

                Stat(header, () => text.Format("used {0:0.0}%", UsedRatio() * 100));
                Stat(header, () => text.Format("frag {0:0.0}%", ExternalFragmentationRatio() * 100));
                Stat(header, () => text.Format("largest free {0}", LargestFreeSpan()));
                Stat(header, () => text.Format("outliers {0}", session.OutlierSlotCount));
            }

            Node(viewport, out var charts)
                .ColorV(s.Palette.AppBackground);
            {
                memoryChartsMenu.Create(charts);
            }
        }

        void LegendItem(EntMut parent, int index)
        {
            Node(parent, out var item)
                .Mutate(s.HorizontalList)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .InnerSpacingV(s.Metrics.CompactSpacing);
            {
                Node(item)
                    .Mutate(s.Swatch)
                    .ColorF(() => MemoryLegendColor(index));

                Node(item)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextF(() => MemoryLegendText(index));
            }
        }

        void Stat(EntMut parent, Func<ReadOnlySpan<char>> value)
        {
            Node(parent)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextF(value);
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

        double ExternalFragmentationRatio()
        {
            var freeBytes = FreeBytes();
            return freeBytes <= 0 ? 0 : 1.0 - Ratio(LargestFreeSpan(), freeBytes);
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
