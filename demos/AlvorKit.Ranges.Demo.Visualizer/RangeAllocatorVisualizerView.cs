namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the range allocator visualizer UI tree.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerView(
    RootText text,
    RangeAllocatorVisualizerStyle style,
    RangeAllocatorVisualizerMemoryView memoryView)
{
    /// <summary>Creates the visualizer UI under the supplied root node.</summary>
    internal void Create(EntMut root, RangeAllocatorVisualizerState state)
    {
        root.Mutate(style.Root);

        CreateHeader(root, state);
        CreateBody(root, state);
    }

    private void CreateHeader(EntMut root, RangeAllocatorVisualizerState state)
    {
        Node(root, out var header)
            .Mutate(style.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, 112f));

        Node(header)
            .Mutate(style.Title)
            .TextV("RangeAllocator visualizer")
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(header)
            .Mutate(style.ScenarioLink)
            .TextF(() => state.Runner.Scenario.Name)
            .OffsetV((0, 32f))
            .AlignmentV(Alignment.Left | Alignment.Top)
            .OnPressF(state.OpenScenarioPicker);

        Node(header)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => state.Runner.Scenario.Description)
            .OffsetV((0, 52f))
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(header)
            .Mutate(style.Label)
            .TextF(() => text.Format(
                "{0}  scenario {1}/{2}  step {3}/{4}  speed {5:0.##}x  ui {6:F2}x",
                state.Playing ? "playing" : "paused",
                state.ScenarioIndex + 1,
                state.ScenarioCount,
                state.Runner.StepIndex,
                state.Runner.Scenario.Commands.Length,
                state.Speed,
                state.UiScale))
            .OffsetV((-style.FloatingTextInset, 0))
            .AlignmentV(Alignment.Right | Alignment.Top);

        Node(header, out var toolbar)
            .Mutate(style.VerticalList)
            .InnerSpacingV(style.SpacingS)
            .AlignmentV(Alignment.Right | Alignment.Bottom);

        Node(toolbar, out var playbackRow)
            .Mutate(style.HorizontalList)
            .AlignmentV(Alignment.Right);
        Button(playbackRow, 84f, () => state.Playing ? "Pause" : "Play", state.TogglePlayback);
        Button(playbackRow, 72f, "Back", state.StepBackward);
        Button(playbackRow, 72f, "Step", state.StepForward);
        Button(playbackRow, 82f, "Reset", state.ResetScenario);
        Button(playbackRow, 86f, "Prev", state.PreviousScenario);
        Button(playbackRow, 78f, "Next", state.NextScenario);
        Button(playbackRow, 96f, "Pack", state.JumpToPack);

        Node(toolbar, out var optionRow)
            .Mutate(style.HorizontalList)
            .AlignmentV(Alignment.Right);
        ToolbarLabel(optionRow, () => text.Format("speed {0:0.##}x", state.Speed));
        Button(optionRow, 42f, "-", state.Slower);
        Button(optionRow, 42f, "+", state.Faster);
        ToolbarLabel(optionRow, () => text.Format("ui {0:F2}x", state.UiScale));
        Button(optionRow, 42f, "-", state.UiScaleDown);
        Button(optionRow, 42f, "+", state.UiScaleUp);
        Button(optionRow, 112f, () => state.ShowLabels ? "Labels on" : "Labels off", state.ToggleLabels);
        Button(optionRow, 122f, () => state.ShowPadding ? "Padding on" : "Padding off", state.TogglePadding);
    }

    /// <summary>Creates the root-level modal layer used to pick the active scenario.</summary>
    internal void CreateScenarioPicker(EntMut root, RangeAllocatorVisualizerState state)
    {
        root.Mutate(style.ModalLayer)
            .IsSelectableV(true)
            .IsSilentFocusableV(true)
            .OnPressF(state.CloseScenarioPicker)
            .IsDisabledF(() => !state.ScenarioPickerOpen);

        Node(root, out var modal)
            .Mutate(style.ModalPanel)
            .SizeF(() => PickerModalSize(root, state));

        Node(modal, out var content)
            .Mutate(style.ModalContent);

        Node(content)
            .Mutate(style.Heading)
            .TextV("select scenario")
            .SizeWeightTypeV(SizeWeightType.Self);

        Node(content)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextV("choose an allocator script to inspect")
            .SizeWeightTypeV(SizeWeightType.Self);

        Node(content, out var columns)
            .Mutate(style.HorizontalList)
            .InnerSpacingV(style.Spacing)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, PickerColumnHeight(state)));

        var rows = (state.ScenarioCount + 1) / 2;
        for (var columnIndex = 0; columnIndex < 2; columnIndex++)
        {
            Node(columns, out var column)
                .Mutate(style.VerticalList)
                .InnerSpacingV(style.SpacingS)
                .SizeInnerMaxRelativeV((0, 0))
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeF(() => PickerColumnSize(columns));

            for (var rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                var scenarioIndex = columnIndex * rows + rowIndex;
                if (scenarioIndex >= state.ScenarioCount)
                    continue;

                CreateScenarioOption(column, state, scenarioIndex);
            }
        }
    }

    private void CreateBody(EntMut root, RangeAllocatorVisualizerState state)
    {
        Node(root, out var body)
            .Mutate(style.HorizontalFill);

        Node(body, out var metrics)
            .Mutate(style.PanelList)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((0, 1))
            .SizeV((320f, 0));
        CreateMetricsPanel(metrics, state);

        Node(body, out var main)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .InnerSpacingV(style.Spacing)
            .SizeRelativeV((1, 1));
        CreateMemoryPanel(main, state);
        CreateTimelinePanel(main, state);
    }

    private void CreateMetricsPanel(EntMut metrics, RangeAllocatorVisualizerState state)
    {
        Node(metrics)
            .Mutate(style.Heading)
            .SizeWeightTypeV(SizeWeightType.Self)
            .TextV("allocator state");

        Metric(metrics, "scenario op", () => state.Runner.LastCommand.Label);
        Metric(metrics, "method", () => state.Runner.LastMethodText);
        Metric(metrics, "args", () => ArgumentSummary(state.Runner.LastCommand));
        Metric(metrics, "kind", () => text.Format("{0}", state.Runner.LastCommand.Kind));

        Spacer(metrics, 4f);
        Metric(metrics, "block slot", () => TouchedValue(state.Runner, range => range.Slot));
        Metric(metrics, "request B", () => RequestValue(state.Runner));
        Metric(metrics, "logical B", () => TouchedValue(state.Runner, range => range.Size));
        Metric(metrics, "capacity B", () => TouchedValue(state.Runner, range => range.CapacitySize));
        Metric(metrics, "retained extra B", () => TouchedValue(state.Runner, range => range.RetainedExtraSize));
        Metric(metrics, "reserved B", () => TouchedValue(state.Runner, range => range.ReservedSize));
        Metric(metrics, "padding B", () => TouchedValue(state.Runner, range => range.ReservedSize - range.CapacitySize));

        Spacer(metrics, 4f);
        Metric(metrics, "backing size", () => text.Format("{0}", state.Runner.Current.Size));
        Metric(metrics, "used", () => text.Format("{0}", state.Runner.Current.Used));
        Metric(metrics, "live ranges", () => text.Format("{0}", state.Runner.Current.LiveCount));
        Metric(metrics, "free blocks", () => text.Format("{0}", state.Runner.Current.FreeBlockCount));
        Metric(metrics, "free sizes", () => text.Format("{0}", state.Runner.Current.FreeSizeCount));
        Metric(metrics, "pooled nodes", () => text.Format("{0}", state.Runner.Current.PooledNodeCount));
        Metric(metrics, "packs", () => text.Format("{0}", state.Runner.Current.PackCount));
        Metric(metrics, "resizes", () => text.Format("{0}", state.Runner.Current.ResizeCount));
        Metric(metrics, "op ticks", () => text.Format("{0}", state.Runner.Current.OperationTicks));
        Metric(metrics, "op managed B", () => text.Format("{0}", state.Runner.Current.OperationManagedBytes));

    }

    private void CreateMemoryPanel(EntMut main, RangeAllocatorVisualizerState state)
    {
        Node(main, out var panel)
            .Mutate(style.PanelList)
            .InnerSizingV(InnerSizing.VerticalWeight)
            .SizeRelativeV((1, 1));

        Node(panel, out var header)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, 40f));
        Node(header)
            .Mutate(style.Heading)
            .TextV("backing store")
            .AlignmentV(Alignment.Left | Alignment.Top);
        Node(header)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => text.Format(
                "size {0}, used {1}, free spans {2}",
                state.Runner.Current.Size,
                state.Runner.Current.Used,
                state.Runner.Current.FreeSpans.Length))
            .OffsetV((0, 22f))
            .AlignmentV(Alignment.Left | Alignment.Top);
        Node(header)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => text.Format("last allocator call: {0}", state.Runner.LastCallText))
            .OffsetV((-style.FloatingTextInset, 0))
            .AlignmentV(Alignment.Right | Alignment.Top);

        Node(panel, out var legend)
            .Mutate(style.HorizontalList)
            .SizeWeightTypeV(SizeWeightType.Self);
        LegendItem(legend, AllocatorVisualFacts.Free, "free block");
        LegendItem(legend, AllocatorVisualFacts.Palette(0), "live payload");
        LegendItem(legend, AllocatorVisualFacts.Retained, "retained");
        LegendItem(legend, AllocatorVisualFacts.Padding, "padding");
        LegendItem(legend, AllocatorVisualFacts.RequestFill, "latest request");

        Node(panel, out var memory)
            .SizeRelativeV((1, 0))
            .ColorV(style.PanelInsetColor)
            .Mutate(node => memoryView.CreateMemoryCharts(node, state));
    }

    private void CreateTimelinePanel(EntMut main, RangeAllocatorVisualizerState state)
    {
        Node(main, out var panel)
            .Mutate(style.Panel)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, 78f));

        Node(panel)
            .Mutate(style.Heading)
            .TextF(() => text.Format("timeline {0}/{1}", state.Runner.StepIndex, state.Runner.Scenario.Commands.Length))
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(panel)
            .Mutate(style.MutedLabel)
            .FontSizeV(style.FontSizeBody)
            .TextF(() => state.Runner.LastCommand.Label)
            .OffsetV((-style.FloatingTextInset, 0))
            .AlignmentV(Alignment.Right | Alignment.Top);

        Node(panel, out var lane)
            .SizeRelativeV((1, 0))
            .SizeV((0, 18f))
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((0, -style.FloatingTextInset))
            .ColorV(style.PanelInsetColor)
            .Mutate(node => memoryView.CreateTimeline(node, state));
    }

    private void Metric(EntMut parent, string name, Func<ReadOnlySpan<char>> value)
    {
        Node(parent, out var row)
            .Mutate(style.MetricRow)
            .SizeWeightTypeV(SizeWeightType.Self);

        Node(row)
            .Mutate(style.MutedLabel)
            .TextV(name)
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(row)
            .Mutate(style.Label)
            .FontSizeV(style.FontSizeSmall)
            .TextF(value)
            .OffsetV((116f, 0))
            .AlignmentV(Alignment.Left | Alignment.Top);
    }

    private void LegendItem(EntMut parent, Vec4 color, string label)
    {
        Node(parent, out var item)
            .Mutate(style.HorizontalList)
            .InnerSpacingV(style.SpacingXS)
            .SizeWeightTypeV(SizeWeightType.Self);

        Node(item).Mutate(ent => style.Swatch(ent, color));
        Node(item)
            .Mutate(style.MutedLabel)
            .TextV(label);
    }

    private void ToolbarLabel(EntMut parent, Func<ReadOnlySpan<char>> value)
    {
        Node(parent)
            .Mutate(style.MutedLabel)
            .SizeV((78f, style.ButtonHeight))
            .SizeTextRelativeV((0, 0))
            .TextAlignmentV(Alignment.Center)
            .TextF(value)
            .AlignmentV(Alignment.Vertical);
    }

    private void CreateScenarioOption(EntMut parent, RangeAllocatorVisualizerState state, int scenarioIndex)
    {
        var scenario = state.ScenarioAt(scenarioIndex);
        Node(parent, out var option)
            .Mutate(node => style.PickerOption(node, () => scenarioIndex == state.ScenarioIndex))
            .OnPressF(() => state.SelectScenario(scenarioIndex));

        Node(option)
            .IsFloatingV(true)
            .SizeRelativeV((0, 1))
            .SizeV((3f, 0))
            .ColorV(style.WarmAccentColor)
            .IsDisabledF(() => scenarioIndex != state.ScenarioIndex);

        Node(option)
            .Mutate(style.Label)
            .FontSizeV(style.FontSizeBody)
            .TextColorF(() => scenarioIndex == state.ScenarioIndex ? style.WarmAccentColor : style.TextColor)
            .TextV(scenario.Name)
            .OffsetV((style.Spacing, 8f))
            .AlignmentV(Alignment.Left | Alignment.Top);

        Node(option)
            .Mutate(style.MutedLabel)
            .TextV(scenario.Description)
            .OffsetV((style.Spacing, 30f))
            .AlignmentV(Alignment.Left | Alignment.Top);
    }

    private Vec2 PickerModalSize(EntMut layer, RangeAllocatorVisualizerState state)
    {
        var inset = style.PickerScreenInset;
        var availableWidth = Math.Max(320f, layer.SizeR.X - inset * 2f);
        var availableHeight = Math.Max(260f, layer.SizeR.Y - inset * 2f);
        var width = Math.Min(availableWidth, Math.Max(760f, layer.SizeR.X * 0.86f));
        var height = Math.Min(availableHeight, PickerModalHeight(state));
        return (SnapEvenFloor(width), SnapEvenFloor(height));
    }

    private float PickerModalHeight(RangeAllocatorVisualizerState state) =>
        PickerColumnHeight(state)
        + style.ModalVerticalPadding
        + style.ModalHeaderHeight
        + style.SpacingS * 2f;

    private float PickerColumnHeight(RangeAllocatorVisualizerState state)
    {
        var rows = (state.ScenarioCount + 1) / 2;
        return rows * style.PickerOptionHeight + Math.Max(0, rows - 1) * style.SpacingS;
    }

    private Vec2 PickerColumnSize(EntMut columns) =>
        (MathF.Floor(Math.Max(0f, columns.SizeR.X - style.Spacing) * 0.5f), columns.SizeR.Y);

    private static float SnapEvenFloor(float value) =>
        MathF.Floor(Math.Max(0f, value) * 0.5f) * 2f;

    private void Button(EntMut parent, float width, string label, Action action) =>
        Button(parent, width, () => label, action);

    private void Button(EntMut parent, float width, Func<ReadOnlySpan<char>> label, Action action)
    {
        Node(parent)
            .Mutate(style.Button)
            .SizeV((width, style.ButtonHeight))
            .TextF(label)
            .OnPressF(action);
    }

    private static void Spacer(EntMut parent, float height)
    {
        Node(parent)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, height));
    }

    private ReadOnlySpan<char> TouchedValue<T>(AllocatorScenarioRunner runner, Func<AllocatorRangeVisual, T> value)
    {
        if (!AllocatorVisualFacts.TryTouchedRange(runner, out var range))
            return "none";

        return text.Format("{0}", value(range));
    }

    private ReadOnlySpan<char> RequestValue(AllocatorScenarioRunner runner) =>
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc
            ? text.Format("{0}", runner.LastCommand.Size)
            : "none";

    private ReadOnlySpan<char> ArgumentSummary(AllocatorCommand command) => command.Kind switch
    {
        AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc =>
            text.Format("slot {0}, align {1}, size {2}", command.Slot, command.Alignment, command.Size),
        AllocatorCommandKind.Free => text.Format("slot {0}", command.Slot),
        AllocatorCommandKind.Pack => "none",
        _ => "scenario not stepped",
    };
}
