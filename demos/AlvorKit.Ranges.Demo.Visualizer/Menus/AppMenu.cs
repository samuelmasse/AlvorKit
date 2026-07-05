namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the editor shell: menu bar, toolbar, workspace docks, and status bar.</summary>
[App]
public class AppMenu(
    AppStyle s,
    AppLayout layout,
    AppSession session,
    AppToolbarMenu toolbarMenu,
    AppMetricsMenu metricsMenu,
    AppMemoryPanelMenu memoryPanelMenu,
    AppTimelinePanelMenu timelinePanelMenu,
    AppStatusMenu statusMenu)
{
    public void Create(EntMut root)
    {
        Node(root, out var shell)
            .Mutate(s.Root);
        {
            MenuBar(shell);
            toolbarMenu.Create(shell);

            Node(shell, out var workspace)
                .ColorV(s.Palette.AppBackground)
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(0);
            {
                metricsMenu.Create(workspace);

                Node(workspace, out var center)
                    .SizeRelativeV((1, 1))
                    .InnerLayoutV(InnerLayout.VerticalList)
                    .InnerSizingV(InnerSizing.VerticalWeight)
                    .InnerSpacingV(0);
                {
                    memoryPanelMenu.Create(center);
                    timelinePanelMenu.Create(center);
                }
            }

            statusMenu.Create(shell);
        }

        void MenuBar(EntMut parent)
        {
            Node(parent, out var menuBar)
                .Mutate(s.MenuBar)
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(0)
                .PaddingV(s.Metrics.MenuBarPadding);
            {
                Node(menuBar, out var brand)
                    .Mutate(s.RightRule)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeInnerSumRelativeV((1, 0))
                    .PaddingV(s.Metrics.BrandPadding)
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSpacingV(s.Metrics.LooseSpacing);
                {
                    Node(brand)
                        .AlignmentV(Alignment.Vertical)
                        .SizeRelativeV((0, 0))
                        .SizeV((layout.BrandMarkSize, layout.BrandMarkSize))
                        .ColorV(s.Palette.Accent);

                    Node(brand)
                        .Mutate(s.EmphasisText)
                        .SizeRelativeV((0, 1))
                        .SizeTextRelativeV((1, 0))
                        .TextPaddingV((0, 0, s.Metrics.RightGlyphPadding, 0))
                        .TextV("Ranges Visualizer");
                }

                Node(menuBar)
                    .Mutate(s.MenuItem)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 0))
                    .SizeTextRelativeV((1, 0))
                    .SizeV((0, s.Metrics.MenuBarHeight))
                    .TextF(() => session.Runner.Scenario.Name)
                    .OnPressF(session.OpenScenarioPicker);

                Node(menuBar)
                    .ColorV(default);

                Node(menuBar)
                    .Mutate(s.MutedText)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextPaddingV((0, 0, s.Metrics.RightGlyphPadding, 0))
                    .TextF(() => session.Runner.Scenario.Description);
            }
        }
    }
}
