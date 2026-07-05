namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Builds the scrubbable command timeline lane with playhead, hover cell, and progress shade.</summary>
[App]
public class AppTimelineMenu(
    RootUiMouse uiMouse,
    RootMouse mouse,
    AppStyle s,
    AppSession session,
    AppTimelineTexture texture)
{
    public void Create(EntMut root)
    {
        const int pendingRevision = -1;

        var lastRevision = pendingRevision;
        var scrubbing = false;
        var mainWasDown = false;
        Node(root, out var content)
            .SizeRelativeV((1, 1))
            .ColorV(s.PanelInsetColor)
            .IsSelectableV(true)
            .IsSilentFocusableV(true)
            .CursorF(() => CursorShape.ResizeHorizontal)
            .TooltipF(() => texture.Tooltip(content))
            .TooltipColorF(() => texture.TooltipColor(content))
            .OnPressF(() =>
            {
                scrubbing = true;
                ScrubTimeline(content);
            })
            .OnUpdateF(() =>
            {
                var mainDown = mouse.IsMainDown();
                if (!mainDown)
                    scrubbing = false;
                else if (!mainWasDown && TimelinePointerInside(content, session.Runner.Scenario.Commands.Length))
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

            Node(lane)
                .SizeRelativeV((1, 1))
                .TextureF(texture.Texture);

            Node(lane)
                .SizeRelativeV((0, 1))
                .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, session.Runner.StepIndex, 0))
                .SizeF(() => TimelineRemainingSize(lane, commands.Length, session.Runner.StepIndex))
                .ColorV((0f, 0f, 0f, 0.64f))
                .IsDisabledF(() => session.Runner.StepIndex >= commands.Length);

            const float markerWidth = 3f;
            const float markerCapPadding = 2f;
            const float markerCapHeight = 3f;

            Node(lane)
                .SizeRelativeV((0, 1))
                .SizeV((markerWidth, 0))
                .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, session.Runner.StepIndex, markerWidth))
                .ColorV(s.HighlightColor);

            Node(lane)
                .SizeRelativeV((0, 0))
                .SizeF(() => (
                    markerWidth + markerCapPadding + markerCapPadding,
                    markerCapHeight))
                .OffsetF(() => TimelineMarkerOffset(lane, commands.Length, session.Runner.StepIndex, markerWidth) - (markerCapPadding, 0))
                .ColorV(s.HighlightColor);

            Node(lane)
                .SizeRelativeV((0, 1))
                .SizeF(() => TimelineCellSize(lane, commands.Length))
                .OffsetF(() => TimelineCellOffset(lane, commands.Length, texture.HoverIndex(lane)))
                .ColorV(s.TimelineHoverColor)
                .IsDisabledF(() => !TimelinePointerInside(lane, commands.Length));

            Outline(lane, () => TimelinePointerInside(lane, commands.Length)
                ? s.AccentColor
                : s.TimelineIdleOutlineColor);
        }

        void ScrubTimeline(EntMut lane)
        {
            var count = session.Runner.Scenario.Commands.Length;
            if (count == 0 || lane.SizeR.X <= 0)
                return;

            var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
            var cell = Math.Clamp((int)Math.Floor(localX / lane.SizeR.X * (double)count), 0, count - 1);
            session.JumpToStep(cell + 1);
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

        Vec2 TimelineRemainingSize(EntMut lane, int count, int step) =>
            count <= 0
                ? default
                : (Math.Max(0, lane.SizeR.X - Math.Clamp(step, 0, count) / (float)count * lane.SizeR.X), lane.SizeR.Y);

        Vec2 TimelineMarkerOffset(EntMut lane, int count, int step, float markerWidth)
        {
            const float markerCenterAnchor = 0.5f;

            if (count <= 0)
                return default;

            return (Math.Clamp(step, 0, count) / (float)count * lane.SizeR.X - markerWidth * markerCenterAnchor, 0);
        }

        bool TimelinePointerInside(EntMut lane, int count) =>
            count > 0 &&
            lane.SizeR.X > 0 &&
            uiMouse.Position.X >= lane.PositionR.X &&
            uiMouse.Position.X <= lane.PositionR.X + lane.SizeR.X &&
            uiMouse.Position.Y >= lane.PositionR.Y &&
            uiMouse.Position.Y <= lane.PositionR.Y + lane.SizeR.Y;

        void Outline(EntMut parent, Func<Vec4> color)
        {
            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, s.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, s.RuleWidth))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((0, 1))
                .SizeV((s.RuleWidth, 0))
                .ColorF(color);

            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((s.RuleWidth, 0))
                .ColorF(color);
        }
    }
}
