namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the memory viewport: mode/legend header strip above the backing-store charts.</summary>
[App]
public class AppMemoryPanelMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppMemoryLegend legend,
    AppMemoryStats stats,
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
                    .TextF(() => text.Format("memory: {0}", legend.ModeName()))
                    .TooltipV("memory overlay mode\ncycles what the strips visualize\nshortcut: M")
                    .OnPressF(session.NextMemoryOverlayMode);

                for (var i = 0; i < legendEntryCount; i++)
                    LegendItem(header, i);

                Node(header)
                    .ColorV(default);

                Node(header)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .IsSelectableV(true)
                    .TooltipV("used share\nused bytes divided by backing size")
                    .TextF(() => text.Format("used {0:0.0}%", stats.UsedRatio() * 100));

                Node(header)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .IsSelectableV(true)
                    .TooltipV("external fragmentation\none minus largest free block over total free bytes\nhigh values mean free space is split into small gaps")
                    .TextF(() => text.Format("frag {0:0.0}%", stats.ExternalFragmentationRatio() * 100));

                Node(header)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .IsSelectableV(true)
                    .TooltipV("largest free span\nbiggest single allocation that would still fit")
                    .TextF(() => text.Format("largest free {0}", stats.LargestFreeSpan()));

                Node(header)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .IsSelectableV(true)
                    .TooltipV("outlier slots\nslots with heavy traffic: many ops, reallocs, or large sizes")
                    .TextF(() => text.Format("outliers {0}", session.OutlierSlotCount));
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
                .InnerSpacingV(s.Metrics.CompactSpacing)
                .IsSelectableV(true)
                .TooltipF(() => legend.EntryTooltip(index))
                .TooltipColorF(() => legend.EntryColor(index));
            {
                Node(item)
                    .Mutate(s.Swatch)
                    .ColorF(() => legend.EntryColor(index));

                Node(item)
                    .Mutate(s.MutedLabel)
                    .AlignmentV(Alignment.Vertical)
                    .TextF(() => legend.EntryLabel(index));
            }
        }
    }
}
