namespace AlvorKit;

/// <summary>Builds the typed parameter dock; rebuilds its sections when the node graph changes.</summary>
[App]
public class AppParamsMenu(
    BlendUi bl,
    AppSession session,
    AppRamps ramps)
{
    /// <summary>Mounts a parameter dock that rebuilds its typed controls when the node selection changes.</summary>
    public void Create(EntMut root, BlendDropdownHandle popup)
    {
        const float dockWidth = 300f;
        const int pendingRevision = -1;

        Node(root, out var dock)
            .Mutate(bl.S.Dock)
            .SizeV((dockWidth, 0))
            .Mutate(bl.S.RightRule);
        {
            Node(dock, out var title)
                .Mutate(bl.S.PanelTitle)
                .PaddingV(bl.S.Metrics.PanelTitlePadding);
            {
                Node(title)
                    .Mutate(bl.S.EmphasisCellLabel)
                    .TextV("Node Parameters");

                Node(title)
                    .Mutate(bl.S.MutedCellLabel)
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextV("editable");
            }

            var lastRevision = pendingRevision;
            Node(dock, out var sections)
                .Mutate(bl.S.PanelFillList)
                .InnerSizingV(InnerSizing.None)
                .OnUpdateF(() =>
                {
                    if (lastRevision == session.UiRevision)
                        return;

                    lastRevision = session.UiRevision;
                    NodesClear(sections);
                    BuildSections(sections);
                });
        }

        // Bind sections to the selected nodes and the preview's display settings.
        void BuildSections(EntMut sections)
        {
            var field = session.Field;

            Section(sections, field.Nodes.Fractals[field.Nodes.FractalIndex].Text, "root", rows =>
            {
                bl.Fields.DropdownField(
                    rows, popup, "Source", field.Nodes.Sources, () => field.Nodes.SourceIndex, session.SelectSource)
                    .Mutate()
                    .TooltipV("Source\nthe generator node feeding the fractal");

                ParameterRows(rows, field.Nodes.FractalParameters);
            });

            Section(sections, field.Nodes.Sources[field.Nodes.SourceIndex].Text, "source", rows =>
                ParameterRows(rows, field.Nodes.SourceParameters));

            Section(sections, "Post", null, rows =>
            {
                bl.Fields.Checkbox(rows, "Normalize output", () => session.Normalize, () =>
                {
                    session.Normalize = !session.Normalize;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("normalize output\nmaps the generated min/max to the full ramp\ninstead of the fixed [-1, 1] range");

                bl.Fields.Checkbox(rows, "Invert", () => session.Invert, () =>
                {
                    session.Invert = !session.Invert;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("invert\nflips the ramp before mapping");

                bl.Fields.DropdownField(rows, popup, "Ramp", ramps.Items, () => session.RampIndex, index =>
                {
                    session.RampIndex = index;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("ramp\nmaps normalized samples to colors");
            });
        }

        // Choose a Blend editor for each authored parameter kind.
        void ParameterRows(EntMut rows, IReadOnlyList<AppNoiseParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                var p = parameter;

                // Regenerate only after applying the edit to the typed node.
                void Set(float value)
                {
                    p.Set(value);
                    session.MarkDirty();
                }

                var node = p.Kind switch
                {
                    AppNoiseParameterKind.Enum => bl.Fields.DropdownField(
                        rows,
                        popup,
                        p.Name,
                        p.EnumItems,
                        () => (int)p.Value,
                        index => Set(index)),
                    AppNoiseParameterKind.Int => bl.Fields.IntField(rows, new()
                    {
                        Label = p.Name,
                        Get = () => (int)p.Value,
                        Set = value => Set(value),
                        Min = p.HasRange ? (int)p.Min : int.MinValue,
                        Max = p.HasRange ? (int)p.Max : int.MaxValue,
                    }),
                    AppNoiseParameterKind.Float when p.HasRange => bl.Fields.SliderField(rows, new()
                    {
                        Label = p.Name,
                        Get = () => p.Value,
                        Set = Set,
                        Min = p.Min,
                        Max = p.Max,
                        Step = (p.Max - p.Min) / 200f,
                    }),
                    _ => bl.Fields.NumberField(rows, new()
                    {
                        Label = p.Name,
                        Get = () => p.Value,
                        Set = Set,
                        Step = DragStep(p.Value),
                    }),
                };

                node.Mutate()
                    .TooltipV(p.Tooltip);
            }
        }

        void Section(EntMut sections, string name, string? role, Action<EntMut> build)
        {
            Node(sections, out var section)
                .Mutate(bl.S.InsetPanelList)
                .PaddingV((bl.S.Metrics.LooseSpacing, 0, bl.S.Metrics.LooseSpacing, bl.S.Metrics.LooseSpacing))
                .InnerSpacingV(bl.S.Metrics.CompactSpacing);
            {
                Node(section, out var header)
                    .Mutate(bl.S.HorizontalRow)
                    .SizeV((0, bl.S.Metrics.FieldHeight));
                {
                    Node(header)
                        .Mutate(bl.S.EmphasisCellLabel)
                        .TextV(name);

                    if (role != null)
                    {
                        Node(header)
                            .Mutate(bl.S.MutedCellLabel)
                            .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                            .TextV(role);
                    }
                }

                build(section);
            }
        }

        static float DragStep(float value) => MathF.Max(0.01f, MathF.Abs(value) / 50f);
    }
}
