namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMenu(
    AppStyle style,
    AppHeaderMenu headerMenu,
    AppMetricsMenu metricsMenu,
    AppMemoryPanelMenu memoryPanelMenu,
    AppTimelinePanelMenu timelinePanelMenu)
{
    public void Create(EntMut root)
    {
        root.Mutate(style.Root);

        Node(root)
            .Mutate(headerMenu.Create);

        Node(root, out var body)
            .Mutate(style.HorizontalFill);
        {
            Node(body)
                .Mutate(metricsMenu.Create);

            Node(body, out var main)
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSizingV(InnerSizing.VerticalWeight)
                .InnerSpacingV(style.Spacing)
                .SizeRelativeV((1, 1));
            {
                Node(main)
                    .Mutate(memoryPanelMenu.Create);

                Node(main)
                    .Mutate(timelinePanelMenu.Create);
            }
        }
    }
}
