namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Builds the Noise Lab shell: menu bar, toolbar, parameter dock + viewport workspace, and status bar.</summary>
[App]
public class AppMenu(
    RootText text,
    AppStyle s,
    AppSession session,
    AppToolbarMenu toolbarMenu,
    AppParamsMenu paramsMenu,
    AppViewportMenu viewportMenu,
    AppStatusMenu statusMenu,
    AppDropdownMenu dropdownMenu,
    AppTooltipMenu tooltipMenu)
{
    public void Create(EntMut root)
    {
        Node(root, out var shell)
            .Mutate(s.Root);
        {
            Node(shell, out var menuBar)
                .Mutate(s.MenuBar)
                .PaddingV(s.Metrics.MenuBarPadding);
            {
                const float brandWidth = 176f;

                Node(menuBar, out var menuRow)
                    .SizeRelativeV((1, 1))
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight);

                Node(menuRow, out var brand)
                    .Mutate(s.Board)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeV((brandWidth, 0))
                    .Mutate(s.RightRule);
                {
                    const float markSize = 13f;
                    var brandInset = s.Metrics.BrandPadding.X;

                    Node(brand)
                        .IsFloatingV(true)
                        .AlignmentV(Alignment.Left | Alignment.Vertical)
                        .OffsetV((brandInset, 0))
                        .SizeRelativeV((0, 0))
                        .SizeV((markSize, markSize))
                        .ColorV(s.Palette.ActiveSurface)
                        .Mutate(mark => BlendStyle.Rule(mark, Alignment.Top | Alignment.Left, (1, 0), (0, s.Metrics.Hairline), s.Palette.Accent))
                        .Mutate(mark => BlendStyle.Rule(mark, Alignment.Bottom | Alignment.Left, (1, 0), (0, s.Metrics.Hairline), s.Palette.Accent))
                        .Mutate(mark => BlendStyle.Rule(mark, Alignment.Top | Alignment.Left, (0, 1), (s.Metrics.Hairline, 0), s.Palette.Accent))
                        .Mutate(mark => BlendStyle.Rule(mark, Alignment.Top | Alignment.Right, (0, 1), (s.Metrics.Hairline, 0), s.Palette.Accent));

                    Node(brand)
                        .Mutate(s.EmphasisCellLabel)
                        .IsFloatingV(true)
                        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
                        .TextPaddingV((brandInset + markSize + s.Metrics.LooseSpacing, 0, 0, 0))
                        .TextV("Noise Lab");
                }

                Node(menuRow);

                Node(menuRow)
                    .Mutate(s.MutedCellLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeTextRelativeV((1, 0))
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextF(() => text.Format(
                        "panel generated from FastNoise2 metadata — 2 nodes · {0} variables · {1} hybrids · 1 lookup",
                        VariableCount(),
                        HybridCount()));
            }

            toolbarMenu.Create(shell);

            Node(shell, out var work)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight);
            {
                paramsMenu.Create(work);
                viewportMenu.Create(work);
            }

            statusMenu.Create(shell);
        }

        dropdownMenu.Create(root);
        tooltipMenu.Create(root);

        int VariableCount() =>
            CountKind(session.Field.FractalParameters, false) + CountKind(session.Field.SourceParameters, false);

        int HybridCount() =>
            CountKind(session.Field.FractalParameters, true) + CountKind(session.Field.SourceParameters, true);

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
