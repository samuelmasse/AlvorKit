namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMenu(
    AppStyle s,
    AppHeaderMenu headerMenu,
    AppMetricsMenu metricsMenu,
    AppMemoryPanelMenu memoryPanelMenu,
    AppTimelinePanelMenu timelinePanelMenu)
{
    public void Create(EntMut root)
    {
        Node(root, out var shell)
            .Mutate(s.Root);
        {
            headerMenu.Create(shell);

            Node(shell, out var body)
                .Mutate(s.HorizontalFill);
            {
                metricsMenu.Create(body);

                Node(body, out var main)
                    .InnerLayoutV(InnerLayout.VerticalList)
                    .InnerSizingV(InnerSizing.VerticalWeight)
                    .InnerSpacingV(s.Spacing)
                    .SizeRelativeV((1, 1));
                {
                    memoryPanelMenu.Create(main);
                    timelinePanelMenu.Create(main);
                }
            }
        }
    }
}
