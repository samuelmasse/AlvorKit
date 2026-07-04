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
            LegendItem(legend, style.FreeBlockColor, "free block");
            LegendItem(legend, style.AllocationColor(0), "live payload");
            LegendItem(legend, style.RetainedColor, "retained");
            LegendItem(legend, style.PaddingColor, "padding");
            LegendItem(legend, style.LatestRequestFillColor, "latest request");
        }

        Node(root)
            .SizeRelativeV((1, 0))
            .ColorV(style.PanelInsetColor)
            .Mutate(memoryChartsMenu.Create);

        void LegendItem(EntMut parent, Vec4 color, string label)
        {
            Node(parent, out var item)
                .Mutate(style.HorizontalList)
                .InnerSpacingV(style.SpacingXS)
                .SizeWeightTypeV(SizeWeightType.Self);
            {
                Node(item).Mutate(ent => style.Swatch(ent, color));
                Node(item)
                    .Mutate(style.MutedLabel)
                    .TextV(label);
            }
        }
    }
}
