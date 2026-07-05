namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppScenarioPickerMenu(
    AppStyle s,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const int columnCount = 2;
        const float optionHeight = 48f;
        const float modalSnapUnit = 2f;

        Node(root, out var layer)
            .Mutate(s.ModalLayer)
            .IsSelectableV(true)
            .IsSilentFocusableV(true)
            .OnPressF(session.CloseScenarioPicker)
            .IsDisabledF(() => !session.ScenarioPickerOpen);
        {
            Node(layer, out var modal)
                .Mutate(s.ModalPanel)
                .SizeAlignmentSnapV(modalSnapUnit)
                .SizeF(() => PickerModalSize(layer));
            {
                Node(modal, out var content)
                    .Mutate(s.ModalContent);
                {
                    Node(content)
                        .Mutate(s.Heading)
                        .TextV("select scenario")
                        .SizeWeightTypeV(SizeWeightType.Self);

                    Node(content)
                        .Mutate(s.MutedLabel)
                        .FontSizeV(s.FontSizeBody)
                        .TextV("choose an allocator script to inspect")
                        .SizeWeightTypeV(SizeWeightType.Self);

                    Node(content, out var columns)
                        .Mutate(s.HorizontalList)
                        .InnerSpacingV(s.Spacing)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((1, 0))
                        .SizeV((0, PickerColumnHeight()));
                    {
                        var rows = PickerRowCount();
                        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                        {
                            Node(columns, out var column)
                                .Mutate(s.VerticalList)
                                .InnerSpacingV(s.SpacingS)
                                .SizeInnerMaxRelativeV((0, 0))
                                .SizeWeightTypeV(SizeWeightType.Self)
                                .SizeF(() => PickerColumnSize(columns));
                            {
                                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                                {
                                    var scenarioIndex = columnIndex * rows + rowIndex;
                                    if (scenarioIndex >= session.ScenarioCount)
                                        continue;

                                    ScenarioOption(column, scenarioIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        void ScenarioOption(EntMut parent, int scenarioIndex)
        {
            const float selectedWidth = 3f;
            const float titleOffsetY = 8f;
            const float descriptionOffsetY = 30f;

            var scenario = session.ScenarioAt(scenarioIndex);
            Node(parent, out var option)
                .Mutate(node => s.PickerOption(node, () => scenarioIndex == session.ScenarioIndex))
                .SizeV((0, optionHeight))
                .OnPressF(() => session.SelectScenario(scenarioIndex));
            {
                Node(option)
                    .IsFloatingV(true)
                    .SizeRelativeV((0, 1))
                    .SizeV((selectedWidth, 0))
                    .ColorV(s.WarmAccentColor)
                    .IsDisabledF(() => scenarioIndex != session.ScenarioIndex);

                Node(option)
                    .Mutate(s.Label)
                    .FontSizeV(s.FontSizeBody)
                    .TextColorF(() => scenarioIndex == session.ScenarioIndex ? s.WarmAccentColor : s.TextColor)
                    .TextV(scenario.Name)
                    .OffsetV((s.Spacing, titleOffsetY))
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(option)
                    .Mutate(s.MutedLabel)
                    .TextV(scenario.Description)
                    .OffsetV((s.Spacing, descriptionOffsetY))
                    .AlignmentV(Alignment.Left | Alignment.Top);
            }
        }

        Vec2 PickerModalSize(EntMut layer)
        {
            const float screenInset = 36f;
            const float minimumAvailableWidth = 320f;
            const float minimumWidth = 760f;
            const float minimumHeight = 260f;
            const float widthRatio = 0.86f;

            var availableWidth = Math.Max(minimumAvailableWidth, layer.SizeR.X - screenInset - screenInset);
            var availableHeight = Math.Max(minimumHeight, layer.SizeR.Y - screenInset - screenInset);
            var width = Math.Min(availableWidth, Math.Max(minimumWidth, layer.SizeR.X * widthRatio));
            var height = Math.Min(availableHeight, PickerModalHeight());
            return (SnapFloor(width), SnapFloor(height));
        }

        float PickerModalHeight()
        {
            const float headerHeight = 36f;

            var verticalPadding = (s.PanelPadding + s.Spacing) + s.PanelPadding;
            return PickerColumnHeight()
                + verticalPadding
                + headerHeight
                + s.SpacingS
                + s.SpacingS;
        }

        float PickerColumnHeight()
        {
            var rows = PickerRowCount();
            return rows * optionHeight + Math.Max(0, rows - 1) * s.SpacingS;
        }

        int PickerRowCount() =>
            (session.ScenarioCount + columnCount - 1) / columnCount;

        Vec2 PickerColumnSize(EntMut columns) =>
            (MathF.Floor(Math.Max(0f, columns.SizeR.X - s.Spacing) / columnCount), columns.SizeR.Y);

        float SnapFloor(float value) =>
            MathF.Floor(Math.Max(0f, value) / modalSnapUnit) * modalSnapUnit;
    }
}
