namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppTimelineMenu(
    RootText text,
    RootUiMouse uiMouse,
    RootMouse mouse,
    AppStyle style,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        var lastRevision = pendingRevision;
        var scrubbing = false;
        var mainWasDown = false;
        Node(root, out var content)
            .SizeRelativeV((1, 1))
            .ColorV(style.PanelInsetColor)
            .OnUpdateF(() =>
            {
                var mainDown = mouse.IsMainDown();
                if (!mainDown)
                    scrubbing = false;
                else if (!mainWasDown && TimelinePointerOverCell(content, session.Runner.Scenario.Commands.Length))
                    scrubbing = true;

                if (scrubbing)
                    ScrubTimeline(content);

                mainWasDown = mainDown;
                if (lastRevision == session.VisualRevision)
                    return;

                lastRevision = session.VisualRevision;
                NodesClear(content);
                BuildTimeline(content);
            });

        void BuildTimeline(EntMut lane)
        {
            var commands = session.Runner.Scenario.Commands;
            if (commands.Length == 0)
                return;

            for (var i = 0; i < commands.Length; i++)
            {
                var index = i;
                var command = commands[index];
                Node(lane, out var cell)
                    .SizeRelativeV((0, 0))
                    .OffsetF(() => TimelineCellOffset(lane, commands.Length, index))
                    .SizeF(() => TimelineCellSize(lane, commands.Length))
                    .ColorF(() => TimelineCellColor(command.Kind, index))
                    .IsSelectableV(true)
                    .IsSilentFocusableV(true)
                    .CursorF(() => CursorShape.ResizeHorizontal)
                    .TooltipF(() => CommandTooltip(index, command))
                    .OnPressF(() =>
                    {
                        scrubbing = true;
                        ScrubTimeline(content);
                    });
                {
                    if (index < commands.Length - 1)
                        TimelineDivider(cell);
                }
            }

            const float markerWidth = 3f;
            const float markerCapPadding = 2f;
            const float markerCapHeight = 3f;

            Node(lane)
                .SizeRelativeV((0, 1))
                .SizeV((markerWidth, 0))
                .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, session.Runner.StepIndex, markerWidth))
                .ColorV(style.HighlightColor);

            Node(lane)
                .SizeRelativeV((0, 0))
                .SizeF(() => (
                    markerWidth + markerCapPadding + markerCapPadding,
                    markerCapHeight))
                .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, session.Runner.StepIndex, markerWidth) - (markerCapPadding, 0))
                .ColorV(style.HighlightColor);

            Node(lane)
                .SizeRelativeV((0, 1))
                .SizeF(() => TimelineCellSize(lane, commands.Length))
                .OffsetF(() => TimelineCellOffset(lane, commands.Length, TimelineHoverIndex(lane, commands.Length)))
                .ColorV(style.TimelineHoverColor)
                .IsDisabledF(() => !TimelinePointerOverCell(lane, commands.Length));

            Outline(lane, () => TimelinePointerOverCell(lane, commands.Length)
                ? style.AccentColor
                : style.TimelineIdleOutlineColor);
        }

        void ScrubTimeline(EntMut lane)
        {
            var count = session.Runner.Scenario.Commands.Length;
            if (count == 0 || lane.SizeR.X <= 0)
                return;

            var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
            var cell = Math.Clamp((int)MathF.Floor(localX / lane.SizeR.X * count), 0, count - 1);
            session.JumpToStep(cell + 1);
        }

        int TimelineHoverIndex(EntMut lane, int count)
        {
            if (count <= 0 || lane.SizeR.X <= 0)
                return 0;

            var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
            return Math.Clamp((int)MathF.Floor(localX / lane.SizeR.X * count), 0, count - 1);
        }

        Vec2 TimelineCellOffset(EntMut lane, int count, int index)
        {
            if (count <= 0)
                return default;

            var cellWidth = lane.SizeR.X / count;
            return (index * cellWidth, 0);
        }

        Vec2 TimelineCellSize(EntMut lane, int count)
        {
            if (count <= 0)
                return default;

            return (Math.Max(1f, lane.SizeR.X / count), lane.SizeR.Y);
        }

        void TimelineDivider(EntMut cell)
        {
            Node(cell)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((style.RuleWidth, 0))
                .ColorV(style.BackgroundColor);
        }

        Vec2 TimelineMarkerOffset(EntMut lane, int count, int step, float markerWidth)
        {
            const float markerCenterAnchor = 0.5f;

            if (count <= 0)
                return default;

            return (Math.Clamp(step, 0, count) / (float)count * lane.SizeR.X - markerWidth * markerCenterAnchor, 0);
        }

        Vec4 TimelineCellColor(AllocatorCommandKind kind, int index)
        {
            const float inactiveDimFactor = 0.28f;

            var color = style.CommandColor(kind);
            if (index >= session.Runner.StepIndex)
                color = style.Dim(color, inactiveDimFactor);

            return color;
        }

        bool TimelinePointerInside(EntMut lane) =>
            uiMouse.Position.X >= lane.PositionR.X &&
            uiMouse.Position.X <= lane.PositionR.X + lane.SizeR.X &&
            uiMouse.Position.Y >= lane.PositionR.Y &&
            uiMouse.Position.Y <= lane.PositionR.Y + lane.SizeR.Y;

        bool TimelinePointerOverCell(EntMut lane, int count)
            => count > 0 && lane.SizeR.X > 0 && TimelinePointerInside(lane);

        ReadOnlySpan<char> CommandTooltip(int index, AllocatorCommand command)
        {
            var eventIndex = index + 1;
            return command.Kind switch
            {
                AllocatorCommandKind.Alloc => text.Format(
                    "scripted event #{0}: alloc slot {1}, alignment {2}, size {3}B; {4}",
                    eventIndex,
                    command.Slot,
                    command.Alignment,
                    command.Size,
                    command.Label),
                AllocatorCommandKind.Realloc => text.Format(
                    "scripted event #{0}: realloc slot {1}, alignment {2}, size {3}B; {4}",
                    eventIndex,
                    command.Slot,
                    command.Alignment,
                    command.Size,
                    command.Label),
                AllocatorCommandKind.Free => text.Format(
                    "scripted event #{0}: free slot {1}; {2}",
                    eventIndex,
                    command.Slot,
                    command.Label),
                AllocatorCommandKind.Pack => text.Format(
                    "scripted event #{0}: pack live ranges; {1}",
                    eventIndex,
                    command.Label),
                _ => text.Format("scripted event #{0}: {1}", eventIndex, command.Label),
            };
        }

        void Outline(EntMut parent, Func<Vec4> color)
        {
            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, style.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, style.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((0, 1))
                .SizeV((style.RuleWidth, 0))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((style.RuleWidth, 0))
                .ColorF(color);
        }
    }
}
