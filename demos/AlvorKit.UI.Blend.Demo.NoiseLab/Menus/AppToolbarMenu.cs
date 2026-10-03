namespace AlvorKit;

/// <summary>Builds the node/seed/generate tool strip; rebuilds its controls when the session UI revision changes.</summary>
[App]
public class AppToolbarMenu(
    BlendUi bl,
    AppSession session)
{
    /// <summary>Mounts the fractal selector and generation controls, rebinding them after selection changes.</summary>
    public void Create(EntMut root, BlendDropdownHandle popup)
    {
        const int pendingRevision = -1;

        Node(root, out var toolbar)
            .Mutate(bl.S.Toolbar)
            .PaddingV(bl.S.Metrics.ToolbarPadding);
        {
            var lastRevision = pendingRevision;
            Node(toolbar, out var controls)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSizingV(InnerSizing.HorizontalWeight)
                .InnerSpacingV(bl.S.Metrics.ToolbarSpacing)
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
                .Mutate(bl.S.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("node");

            bl.Fields.DropdownField(controls, popup, string.Empty, session.Field.Nodes.Fractals,
                () => session.Field.Nodes.FractalIndex, session.SelectFractal)
                .Mutate()
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((nodeFieldWidth, bl.S.Metrics.FieldHeight))
                .TooltipV("fractal node\nthe root FastNoise2 node the panel edits");

            Separator(controls);

            Node(controls)
                .Mutate(bl.S.MutedLabel)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("seed");

            bl.Fields.IntField(controls, new()
            {
                Label = string.Empty,
                Get = () => session.Seed,
                Set = session.SetSeed,
            })
                .Mutate()
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((seedFieldWidth, bl.S.Metrics.FieldHeight))
                .TooltipV("seed\ndrag scrubs, click types a value");

            Node(controls)
                .Mutate(bl.S.SquareButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("R")
                .TooltipV("randomize seed")
                .OnClickF(session.RandomizeSeed);

            Separator(controls);

            Node(controls)
                .Mutate(session.Auto ? bl.S.ActiveToolbarButton : bl.S.ToolbarButton)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV("Auto")
                .TooltipV("auto regenerate\nregenerates whenever a parameter changes")
                .OnClickF(session.ToggleAuto);

            Node(controls)
                .Mutate(bl.S.ToolbarButton)
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
                .SizeV((bl.S.Metrics.Hairline, 0))
                .MarginV((bl.S.Metrics.CompactSpacing, separatorInsetY, bl.S.Metrics.CompactSpacing, separatorInsetY))
                .ColorV(bl.S.Palette.Border);
        }
    }
}
