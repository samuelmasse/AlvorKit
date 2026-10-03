namespace AlvorKit;

/// <summary>Builds the editor shell: menu bar, toolbar, workspace docks, and status bar.</summary>
[App]
public class AppMenu(
    BlendUi bl,
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
            .Mutate(bl.S.Root);
        {
            MenuBar(shell);
            toolbarMenu.Create(shell);

            Node(shell, out var workspace)
                .ColorV(bl.S.Palette.AppBackground)
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
                .Mutate(bl.S.MenuBar)
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(0)
                .PaddingV(bl.S.Metrics.MenuBarPadding);
            {
                Node(menuBar, out var brand)
                    .Mutate(bl.S.RightRule)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeInnerSumRelativeV((1, 0))
                    .PaddingV(bl.S.Metrics.BrandPadding)
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSpacingV(bl.S.Metrics.LooseSpacing);
                {
                    Node(brand)
                        .AlignmentV(Alignment.Vertical)
                        .SizeRelativeV((0, 0))
                        .SizeV((layout.BrandMarkSize, layout.BrandMarkSize))
                        .ColorV(bl.S.Palette.Accent);

                    Node(brand)
                        .Mutate(bl.S.EmphasisText)
                        .SizeRelativeV((0, 1))
                        .SizeTextRelativeV((1, 0))
                        .TextPaddingV((0, 0, bl.S.Metrics.RightGlyphPadding, 0))
                        .TextV("Ranges Visualizer");
                }

                Node(menuBar)
                    .Mutate(bl.S.MenuItem)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 0))
                    .SizeTextRelativeV((1, 0))
                    .SizeV((0, bl.S.Metrics.MenuBarHeight))
                    .TextF(() => session.Runner.Scenario.Name)
                    .TooltipV("active scenario\nclick to open the scenario picker\ntab or mouse wheel also switch")
                    .OnPressF(session.OpenScenarioPicker);

                Node(menuBar)
                    .ColorV(default);

                Node(menuBar)
                    .Mutate(bl.S.MutedText)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextPaddingV((0, 0, bl.S.Metrics.RightGlyphPadding, 0))
                    .TextF(() => session.Runner.Scenario.Description);
            }
        }
    }
}
