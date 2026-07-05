namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Builds the metadata-driven parameter dock; rebuilds its sections when the node graph changes.</summary>
[App]
public class AppParamsMenu(
    AppStyle s,
    AppSession session,
    AppRamps ramps,
    AppFields fields)
{
    public void Create(EntMut root)
    {
        const float dockWidth = 300f;
        const int pendingRevision = -1;

        Node(root, out var dock)
            .Mutate(s.Dock)
            .SizeV((dockWidth, 0))
            .Mutate(s.RightRule);
        {
            Node(dock, out var title)
                .Mutate(s.PanelTitle)
                .PaddingV(s.Metrics.PanelTitlePadding);
            {
                Node(title)
                    .Mutate(s.EmphasisCellLabel)
                    .TextV("Node Parameters");

                Node(title)
                    .Mutate(s.MutedCellLabel)
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                    .TextV("from metadata");
            }

            var lastRevision = pendingRevision;
            Node(dock, out var sections)
                .Mutate(s.PanelFillList)
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

        void BuildSections(EntMut sections)
        {
            var field = session.Field;

            Section(sections, field.Fractals[field.FractalIndex].Text, "root", rows =>
            {
                fields.DropdownField(rows, "Source", field.Sources, () => field.SourceIndex, session.SelectSource)
                    .Mutate()
                    .TooltipV("Source\nthe generator node feeding the fractal");

                ParameterRows(rows, field.FractalParameters);
            });

            Section(sections, field.Sources[field.SourceIndex].Text, "source", rows =>
                ParameterRows(rows, field.SourceParameters));

            Section(sections, "Post", null, rows =>
            {
                fields.Checkbox(rows, "Normalize output", () => session.Normalize, () =>
                {
                    session.Normalize = !session.Normalize;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("normalize output\nmaps the generated min/max to the full ramp\ninstead of the fixed [-1, 1] range");

                fields.Checkbox(rows, "Invert", () => session.Invert, () =>
                {
                    session.Invert = !session.Invert;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("invert\nflips the ramp before mapping");

                fields.DropdownField(rows, "Ramp", ramps.Items, () => session.RampIndex, index =>
                {
                    session.RampIndex = index;
                    session.MarkDirty();
                })
                    .Mutate()
                    .TooltipV("ramp\nmaps normalized samples to colors");
            });
        }

        void ParameterRows(EntMut rows, IReadOnlyList<AppNoiseParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                var p = parameter;

                void Set(float value)
                {
                    session.Field.Apply(p, value);
                    session.MarkDirty();
                }

                var node = p.Kind switch
                {
                    AppNoiseParameterKind.Enum => fields.DropdownField(
                        rows,
                        p.Name,
                        p.EnumItems,
                        () => (int)p.Value,
                        index => Set(index)),
                    AppNoiseParameterKind.Int => fields.IntField(rows, new()
                    {
                        Label = p.Name,
                        Get = () => (int)p.Value,
                        Set = value => Set(value),
                        Min = p.HasRange ? (int)p.Min : int.MinValue,
                        Max = p.HasRange ? (int)p.Max : int.MaxValue,
                    }),
                    AppNoiseParameterKind.Float when p.HasRange => fields.SliderField(rows, new()
                    {
                        Label = p.Name,
                        Get = () => p.Value,
                        Set = Set,
                        Min = p.Min,
                        Max = p.Max,
                        Step = (p.Max - p.Min) / 200f,
                    }),
                    _ => fields.NumberField(rows, new()
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
                .Mutate(s.InsetPanelList)
                .PaddingV((s.Metrics.LooseSpacing, 0, s.Metrics.LooseSpacing, s.Metrics.LooseSpacing))
                .InnerSpacingV(s.Metrics.CompactSpacing);
            {
                Node(section, out var header)
                    .Mutate(s.HorizontalRow)
                    .SizeV((0, s.Metrics.FieldHeight));
                {
                    Node(header)
                        .Mutate(s.EmphasisCellLabel)
                        .TextV(name);

                    if (role != null)
                    {
                        Node(header)
                            .Mutate(s.MutedCellLabel)
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
