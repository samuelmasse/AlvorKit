namespace AlvorKit;

/// <summary>Builds the Noise Lab shell: menu bar, toolbar, parameter dock + viewport workspace, and status bar.</summary>
[App]
public class AppMenu(
    RootText text,
    BlendUi bl,
    AppSession session,
    AppToolbarMenu toolbarMenu,
    AppParamsMenu paramsMenu,
    AppViewportMenu viewportMenu,
    AppStatusMenu statusMenu)
{
    /// <summary>Mounts the lab panels and summaries of the selected typed controls.</summary>
    public void Create(EntMut root)
    {
        Node(root, out var layers)
            .SizeRelativeV((1, 1))
            .IsOrderedV(true);
        {
            Node(layers, out var popupLayer)
                .SizeRelativeV((1, 1))
                .OrderValueV(1);
            var popup = bl.Dropdown.Create(popupLayer);

            Node(layers, out var shell)
                .Mutate(bl.S.Root)
                .OrderValueV(0);
            {
                Node(shell, out var menuBar)
                    .Mutate(bl.S.MenuBar)
                    .PaddingV(bl.S.Metrics.MenuBarPadding);
                {
                    const float brandWidth = 176f;

                    Node(menuBar, out var menuRow)
                        .SizeRelativeV((1, 1))
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);

                    Node(menuRow, out var brand)
                        .Mutate(bl.S.Board)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((0, 1))
                        .SizeV((brandWidth, 0))
                        .Mutate(bl.S.RightRule);
                    {
                        const float markSize = 13f;
                        var brandInset = bl.S.Metrics.BrandPadding.X;

                        Node(brand, out var mark)
                            .IsFloatingV(true)
                            .AlignmentV(Alignment.Left | Alignment.Vertical)
                            .OffsetV((brandInset, 0))
                            .SizeRelativeV((0, 0))
                            .SizeV((markSize, markSize))
                            .ColorV(bl.S.Palette.ActiveSurface);
                        var hairline = bl.S.Metrics.Hairline;
                        var accent = bl.S.Palette.Accent;
                        BlendStyle.Rule(mark, Alignment.Top | Alignment.Left, (1, 0), (0, hairline), accent);
                        BlendStyle.Rule(mark, Alignment.Bottom | Alignment.Left, (1, 0), (0, hairline), accent);
                        BlendStyle.Rule(mark, Alignment.Top | Alignment.Left, (0, 1), (hairline, 0), accent);
                        BlendStyle.Rule(mark, Alignment.Top | Alignment.Right, (0, 1), (hairline, 0), accent);

                        Node(brand)
                            .Mutate(bl.S.EmphasisCellLabel)
                            .IsFloatingV(true)
                            .TextAlignmentV(Alignment.Left | Alignment.Vertical)
                            .TextPaddingV((brandInset + markSize + bl.S.Metrics.LooseSpacing, 0, 0, 0))
                            .TextV("Noise Lab");
                    }

                    Node(menuRow);

                    Node(menuRow)
                        .Mutate(bl.S.MutedCellLabel)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((0, 1))
                        .SizeTextRelativeV((1, 0))
                        .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                        .TextF(() => text.Format(
                            "noise controls — 2 nodes · {0} variables · {1} hybrids · 1 lookup",
                            VariableCount(),
                            HybridCount()));
                }

                toolbarMenu.Create(shell, popup);

                Node(shell, out var work)
                    .SizeRelativeV((1, 1))
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight);
                {
                    paramsMenu.Create(work, popup);
                    viewportMenu.Create(work);
                }

                statusMenu.Create(shell);
            }

            Node(layers, out var tooltipLayer)
                .SizeRelativeV((1, 1))
                .OrderValueV(2);
            {
                bl.Tooltip.Create(tooltipLayer);
            }
        }

        // Count the currently displayed scalar and enum controls.
        int VariableCount() =>
            CountKind(session.Field.Nodes.FractalParameters, false) + CountKind(session.Field.Nodes.SourceParameters, false);

        // Count the currently displayed hybrid inputs.
        int HybridCount() =>
            CountKind(session.Field.Nodes.FractalParameters, true) + CountKind(session.Field.Nodes.SourceParameters, true);

        static int CountKind(IReadOnlyList<AppNoiseParameter> parameters, bool hybrid)
        {
            var count = 0;
            foreach (var parameter in parameters)
            {
                if ((parameter.Kind == AppNoiseParameterKind.Hybrid) == hybrid)
                    count++;
            }

            return count;
        }
    }
}
