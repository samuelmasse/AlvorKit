namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Builds the node/seed/generate tool strip; rebuilds its controls when the session UI revision changes.</summary>
[App]
public class AppToolbarMenu(
    AppStyle s,
    AppSession session,
    AppFields fields)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        Node(root, out var toolbar)
            .Mutate(s.Toolbar)
            .PaddingV(s.Metrics.ToolbarPadding);
        {
            var lastRevision = pendingRevision;
            Node(toolbar, out var controls)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(s.Metrics.ToolbarSpacing)
                .OnUpdateF(() =>
                {
                    if (lastRevision == session.UiRevision)
                        return;

                    lastRevision = session.UiRevision;
                    NodesClear(controls);
                    BuildControls(controls);
                });
        }

        void BuildControls(EntMut controls)
        {
            const float nodeFieldWidth = 170f;
            const float seedFieldWidth = 96f;

            Node(controls)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("node");

            fields.DropdownField(controls, string.Empty, session.Field.Fractals, () => session.Field.FractalIndex, session.SelectFractal)
                .Mutate()
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((nodeFieldWidth, s.Metrics.FieldHeight))
                .TooltipV("fractal node\nthe root FastNoise2 node the panel edits");

            Separator(controls);

            Node(controls)
                .Mutate(s.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("seed");

            fields.IntField(controls, new()
            {
                Label = string.Empty,
                Get = () => session.Seed,
                Set = session.SetSeed,
            })
                .Mutate()
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((seedFieldWidth, s.Metrics.FieldHeight))
                .TooltipV("seed\ndrag scrubs, click types a value");

            Node(controls)
                .Mutate(s.SquareButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("R")
                .TooltipV("randomize seed")
                .OnClickF(session.RandomizeSeed);

            Separator(controls);

            Node(controls)
                .Mutate(session.Auto ? s.ActiveToolbarButton : s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Auto")
                .TooltipV("auto regenerate\nregenerates whenever a parameter changes")
                .OnClickF(session.ToggleAuto);

            Node(controls)
                .Mutate(s.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Regenerate")
                .TooltipV("regenerate now\nruns one generation with the current parameters")
                .OnClickF(session.RegenerateNow);

            Node(controls);
        }

        void Separator(EntMut controls)
        {
            const float separatorInsetY = 2f;

            Node(controls)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((s.Metrics.Hairline, 0))
                .MarginV((s.Metrics.CompactSpacing, separatorInsetY, s.Metrics.CompactSpacing, separatorInsetY))
                .ColorV(s.Palette.Border);
        }
    }
}
