namespace AlvorKit;

/// <summary>Builds the modal scenario picker with a two-column grid of selectable scenario options.</summary>
[App]
public class AppScenarioPickerMenu(
    RootText text,
    BlendUi bl,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const int columnCount = 2;
        const float optionHeight = 48f;
        const float modalSnapUnit = 2f;

        Node(root, out var layer)
            .Mutate(bl.S.ModalLayer)
            .OnPressF(session.CloseScenarioPicker)
            .IsDisabledF(() => !session.ScenarioPickerOpen);
        {
            Node(layer, out var modal)
                .Mutate(bl.S.ModalPanel)
                .SizeAlignmentSnapV(modalSnapUnit)
                .SizeF(() => PickerModalSize(layer));
            {
                Node(modal, out var title)
                    .Mutate(bl.S.PanelTitle)
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(bl.S.Metrics.LooseSpacing)
                    .PaddingV(bl.S.Metrics.PanelTitlePadding);
                {
                    Node(title)
                        .Mutate(bl.S.EmphasisText)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((0, 1))
                        .SizeTextRelativeV((1, 0))
                        .TextV("Select Scenario");

                    Node(title)
                        .ColorV(default);

                    Node(title)
                        .Mutate(bl.S.MutedText)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((0, 1))
                        .SizeTextRelativeV((1, 0))
                        .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                        .TextPaddingV((0, 0, bl.S.Metrics.RightGlyphPadding, 0))
                        .TextF(() => text.Format("{0} scenarios", session.ScenarioCount));
                }

                Node(modal, out var content)
                    .Mutate(bl.S.ModalContent)
                    .InnerSpacingV(bl.S.Metrics.LooseSpacing);
                {
                    Node(content)
                        .Mutate(bl.S.MutedLabel)
                        .TextV("choose an allocator script to inspect")
                        .SizeWeightTypeV(SizeWeightType.Self);

                    Node(content, out var columns)
                        .Mutate(bl.S.HorizontalList)
                        .InnerSpacingV(bl.S.Metrics.LooseSpacing)
                        .SizeWeightTypeV(SizeWeightType.Self)
                        .SizeRelativeV((1, 0))
                        .SizeV((0, PickerColumnHeight()));
                    {
                        var rows = PickerRowCount();
                        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                        {
                            Node(columns, out var column)
                                .Mutate(bl.S.VerticalList)
                                .InnerSpacingV(bl.S.Metrics.CompactSpacing)
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
            const float accentBarWidth = 3f;
            const float nameOffsetY = 8f;
            const float descriptionOffsetY = 26f;

            var textOffsetX = bl.S.Metrics.LooseSpacing + accentBarWidth;
            var scenario = session.ScenarioAt(scenarioIndex);
            Node(parent, out var option)
                .SizeRelativeV((1, 0))
                .SizeV((0, optionHeight))
                .ColorF(() => OptionFill(option, scenarioIndex == session.ScenarioIndex))
                .IsSelectableV(true)
                .IsFocusableV(true)
                .CursorF(() => CursorShape.Hand)
                .OnPressF(() => session.SelectScenario(scenarioIndex))
                .Mutate(node => OptionBorder(node, scenarioIndex));
            {
                Node(option)
                    .SizeRelativeV((0, 1))
                    .SizeV((accentBarWidth, 0))
                    .ColorV(bl.S.Palette.Accent)
                    .IsDisabledF(() => scenarioIndex != session.ScenarioIndex);

                Node(option)
                    .Mutate(bl.S.Label)
                    .TextV(scenario.Name)
                    .OffsetV((textOffsetX, nameOffsetY))
                    .AlignmentV(Alignment.Left | Alignment.Top);

                Node(option)
                    .Mutate(bl.S.MutedLabel)
                    .TextV(scenario.Description)
                    .OffsetV((textOffsetX, descriptionOffsetY))
                    .AlignmentV(Alignment.Left | Alignment.Top);
            }
        }

        Vec4 OptionFill(EntMut option, bool selected)
        {
            if (selected)
                return bl.S.Palette.ActiveSurface;

            if (option.IsPressedR)
                return bl.S.Palette.Selection;

            if (option.IsHoveredR)
                return bl.S.Palette.Hover;

            return bl.S.Palette.AppBackground;
        }

        void OptionBorder(EntMut option, int scenarioIndex)
        {
            OptionRule(option, Alignment.Top | Alignment.Left, (1, 0), (0, bl.S.Metrics.Hairline), scenarioIndex);
            OptionRule(option, Alignment.Bottom | Alignment.Left, (1, 0), (0, bl.S.Metrics.Hairline), scenarioIndex);
            OptionRule(option, Alignment.Top | Alignment.Left, (0, 1), (bl.S.Metrics.Hairline, 0), scenarioIndex);
            OptionRule(option, Alignment.Top | Alignment.Right, (0, 1), (bl.S.Metrics.Hairline, 0), scenarioIndex);
        }

        void OptionRule(EntMut option, Alignment alignment, Vec2 relativeSize, Vec2 size, int scenarioIndex)
        {
            Node(option)
                .IsFloatingV(true)
                .IsPostSizedV(true)
                .AlignmentV(alignment)
                .SizeRelativeV(relativeSize)
                .SizeV(size)
                .ColorF(() => OptionBorderColor(option, scenarioIndex == session.ScenarioIndex));
        }

        Vec4 OptionBorderColor(EntMut option, bool selected)
        {
            if (selected)
                return bl.S.Palette.Accent;

            if (option.IsFocusedR || option.IsHoveredR)
                return bl.S.Palette.StrongBorder;

            return bl.S.Palette.Border;
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
            const float subtitleHeight = 18f;

            var padding = bl.S.Metrics.ModalContentPadding;
            return bl.S.Metrics.PanelTitleHeight
                + padding.Y + padding.W
                + subtitleHeight
                + bl.S.Metrics.LooseSpacing
                + PickerColumnHeight();
        }

        float PickerColumnHeight()
        {
            var rows = PickerRowCount();
            return rows * optionHeight + Math.Max(0, rows - 1) * bl.S.Metrics.CompactSpacing;
        }

        int PickerRowCount() =>
            (session.ScenarioCount + columnCount - 1) / columnCount;

        Vec2 PickerColumnSize(EntMut columns) =>
            (MathF.Floor(Math.Max(0f, columns.SizeR.X - bl.S.Metrics.LooseSpacing) / columnCount), columns.SizeR.Y);

        float SnapFloor(float value) =>
            MathF.Floor(Math.Max(0f, value) / modalSnapUnit) * modalSnapUnit;
    }
}
