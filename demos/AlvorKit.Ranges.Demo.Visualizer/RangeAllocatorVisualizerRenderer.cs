namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Draws allocator snapshots as a slow-motion memory strip and operation timeline.</summary>
[Root]
internal sealed class RangeAllocatorVisualizerRenderer(
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale,
    RootCanvas canvas)
{
    private static readonly Vec4 Background = (0.025f, 0.03f, 0.038f, 1f);
    private static readonly Vec4 Panel = (0.07f, 0.08f, 0.095f, 0.92f);
    private static readonly Vec4 PanelSoft = (0.105f, 0.115f, 0.13f, 0.92f);
    private static readonly Vec4 Text = (0.9f, 0.93f, 0.95f, 1f);
    private static readonly Vec4 MutedText = (0.58f, 0.64f, 0.68f, 1f);
    private static readonly Vec4 Free = (0.13f, 0.25f, 0.22f, 1f);
    private static readonly Vec4 TailFree = (0.09f, 0.14f, 0.14f, 1f);
    private static readonly Vec4 FreeEdge = (0.27f, 0.6f, 0.48f, 1f);
    private static readonly Vec4 Padding = (0.95f, 0.62f, 0.2f, 0.78f);
    private static readonly Vec4 Trail = (0.98f, 0.86f, 0.35f, 0.72f);
    private static readonly Vec4 RequestFill = (0.9f, 1f, 1f, 0.38f);
    private static readonly Vec4 RequestEdge = (1f, 0.96f, 0.55f, 0.95f);
    private static readonly Vec4 Highlight = (1f, 0.96f, 0.55f, 1f);
    private static readonly Vec4 AllocColor = (0.25f, 0.8f, 0.95f, 1f);
    private static readonly Vec4 FreeColor = (0.95f, 0.34f, 0.36f, 1f);
    private static readonly Vec4 PackColor = (0.78f, 0.56f, 1f, 1f);
    private static readonly Vec4 ResizeColor = (0.55f, 0.9f, 0.42f, 1f);

    /// <summary>Gets the clear color used by the demo.</summary>
    internal Vec4 ClearColor => Background;

    /// <summary>Draws the full visualizer frame.</summary>
    internal void Draw(
        AllocatorScenario scenario,
        AllocatorScenarioRunner runner,
        float phase,
        float speed,
        bool playing,
        bool showLabels,
        bool showPadding,
        bool showTrails)
    {
        var batch = sprites.Batch;
        var ui = scale.Scale;
        var canvasSize = canvas.Size;
        var titleFont = roboto[Math.Max(18, scale[24])];
        var bodyFont = roboto[Math.Max(13, scale[16])];
        var smallFont = roboto[Math.Max(11, scale[13])];
        var margin = S(24f, ui);
        var headerHeight = S(104f, ui);
        var footerHeight = S(112f, ui);
        var sideWidth = Math.Min(S(560f, ui), Math.Max(S(340f, ui), canvasSize.X * 0.3f));
        var memoryPosition = new Vec2(sideWidth + margin * 1.5f, headerHeight);
        var memorySize = new Vec2(
            Math.Max(S(320f, ui), canvasSize.X - sideWidth - margin * 2.5f),
            Math.Max(S(170f, ui), canvasSize.Y - headerHeight - footerHeight - margin));
        var timelinePosition = new Vec2(memoryPosition.X, canvasSize.Y - S(88f, ui));
        var timelineSize = new Vec2(memorySize.X, S(52f, ui));

        batch.Draw((0, 0), canvasSize, Background);
        DrawHeader(batch, titleFont, bodyFont, text, scenario, runner, speed, playing, canvasSize, ui);
        DrawMetricPanel(
            batch,
            bodyFont,
            smallFont,
            text,
            runner,
            scenario,
            (margin, headerHeight - S(8f, ui)),
            (sideWidth, canvasSize.Y - headerHeight),
            ui);
        DrawMemoryStrip(batch, bodyFont, smallFont, text, runner, memoryPosition, memorySize, phase, showLabels, showPadding, showTrails, ui);
        DrawTimeline(batch, smallFont, text, scenario, runner, timelinePosition, timelineSize, ui);
    }

    /// <summary>Draws the demo header.</summary>
    private static void DrawHeader(
        SpriteBatchWriter batch,
        FontSize titleFont,
        FontSize bodyFont,
        RootText text,
        AllocatorScenario scenario,
        AllocatorScenarioRunner runner,
        float speed,
        bool playing,
        Vec2 canvasSize,
        float ui)
    {
        batch.Write(titleFont, "RangeAllocator visualizer", (S(24f, ui), S(28f, ui)), Text);
        batch.Write(bodyFont, scenario.Name, (S(24f, ui), S(64f, ui)), Palette(3));
        batch.Write(bodyFont, scenario.Description, (S(360f, ui), S(64f, ui)), MutedText);
        batch.Write(
            bodyFont,
            text.Format(
                "{0}  step {1}/{2}  speed {3:F1}x  ui {4:F2}x",
                playing ? "playing" : "paused",
                runner.StepIndex,
                scenario.Commands.Length,
                speed,
                ui),
            (canvasSize.X - S(520f, ui), S(34f, ui)),
            Text);
    }

    /// <summary>Draws the left-side allocator metric panel.</summary>
    private static void DrawMetricPanel(
        SpriteBatchWriter batch,
        FontSize bodyFont,
        FontSize smallFont,
        RootText text,
        AllocatorScenarioRunner runner,
        AllocatorScenario scenario,
        Vec2 position,
        Vec2 size,
        float ui)
    {
        var snapshot = runner.Current;
        batch.Draw(position, size, Panel);
        var cursor = position + (S(18f, ui), S(18f, ui));
        var lineHeight = S(24f, ui);
        batch.Write(bodyFont, "allocator state", cursor, Text);
        cursor += (0, S(34f, ui));
        WriteMetric(batch, smallFont, text, "scenario op", runner.LastCommand.Label, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "method", runner.LastMethodText, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "args", runner.LastArgumentsText, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "kind", runner.LastCommand.Kind.ToString(), cursor, ui);
        cursor += (0, lineHeight);
        if (TryTouchedRange(runner, out var touchedRange))
        {
            var hasRequest = TryRequestBytes(runner, touchedRange.Slot, out var requestBytes);
            WriteMetric(batch, smallFont, text, "block slot", touchedRange.Slot, cursor, ui);
            cursor += (0, lineHeight);
            if (hasRequest)
                WriteMetric(batch, smallFont, text, "request B", requestBytes, cursor, ui);
            else
                WriteMetric(batch, smallFont, text, "request B", "none", cursor, ui);

            cursor += (0, lineHeight);
            WriteMetric(batch, smallFont, text, "payload B", touchedRange.Size, cursor, ui);
            cursor += (0, lineHeight);
            if (hasRequest && touchedRange.Size > requestBytes)
            {
                WriteMetric(batch, smallFont, text, "retained extra B", touchedRange.Size - requestBytes, cursor, ui);
                cursor += (0, lineHeight);
            }

            WriteMetric(batch, smallFont, text, "reserved B", touchedRange.ReservedSize, cursor, ui);
            cursor += (0, lineHeight);
            WriteMetric(batch, smallFont, text, "padding B", touchedRange.ReservedSize - touchedRange.Size, cursor, ui);
            cursor += (0, lineHeight);
        }
        else
        {
            WriteMetric(batch, smallFont, text, "block bytes", "none", cursor, ui);
            cursor += (0, lineHeight);
        }

        WriteMetric(batch, smallFont, text, "backing size", snapshot.Size, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "used", snapshot.Used, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "live ranges", snapshot.LiveCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "free blocks", snapshot.FreeBlockCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "free sizes", snapshot.FreeSizeCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "pooled nodes", snapshot.PooledNodeCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "packs", snapshot.PackCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "resizes", snapshot.ResizeCount, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "op ticks", snapshot.OperationTicks, cursor, ui);
        cursor += (0, lineHeight);
        WriteMetric(batch, smallFont, text, "op managed B", snapshot.OperationManagedBytes, cursor, ui);

        var fillTop = position + (S(18f, ui), size.Y - S(92f, ui));
        batch.Draw(fillTop, (size.X - S(36f, ui), S(10f, ui)), PanelSoft);
        batch.Draw(fillTop, ((size.X - S(36f, ui)) * Fill(snapshot.Used, snapshot.Size), S(10f, ui)), ResizeColor);
        batch.Write(smallFont, text.Format("reserved {0:F1}%", Fill(snapshot.Used, snapshot.Size) * 100f), fillTop + (0, S(20f, ui)), MutedText);
        batch.Write(smallFont, text.Format("{0} commands scripted", scenario.Commands.Length), fillTop + (0, S(44f, ui)), MutedText);
    }

    /// <summary>Writes one metric line in the side panel.</summary>
    private static void WriteMetric<T>(SpriteBatchWriter batch, FontSize font, RootText text, string name, T value, Vec2 position, float ui)
    {
        batch.Write(font, name, position, MutedText);
        batch.Write(font, text.Format("{0}", value), position + (S(155f, ui), 0), Text);
    }

    /// <summary>Draws the main memory strip.</summary>
    private static void DrawMemoryStrip(
        SpriteBatchWriter batch,
        FontSize bodyFont,
        FontSize smallFont,
        RootText text,
        AllocatorScenarioRunner runner,
        Vec2 position,
        Vec2 size,
        float phase,
        bool showLabels,
        bool showPadding,
        bool showTrails,
        float ui)
    {
        var snapshot = runner.Current;
        batch.Draw(position, size, Panel);
        batch.Write(bodyFont, "backing store", position + (S(18f, ui), S(16f, ui)), Text);
        batch.Write(
            smallFont,
            text.Format("size {0}, used {1}, free spans {2}", snapshot.Size, snapshot.Used, snapshot.FreeSpans.Length),
            position + (S(18f, ui), S(48f, ui)),
            MutedText);
        batch.Write(smallFont, "last allocator call:", position + (S(18f, ui), S(72f, ui)), MutedText);
        batch.Write(smallFont, runner.LastCallText, position + (S(205f, ui), S(72f, ui)), Highlight);
        DrawLegend(batch, smallFont, position + (size.X - S(740f, ui), S(18f, ui)), ui);

        var stripPosition = position + (S(24f, ui), S(88f, ui));
        var stripWidth = size.X - S(48f, ui);
        var detailEnd = DetailEnd(snapshot, out var tailOmitted);
        var detailHeight = Math.Min(S(170f, ui), Math.Max(S(82f, ui), size.Y * 0.2f));
        var detailGap = S(54f, ui);
        var stripSize = new Vec2(stripWidth, Math.Max(S(48f, ui), size.Y - S(128f, ui) - detailHeight - detailGap));
        batch.Draw(stripPosition, stripSize, (0.045f, 0.05f, 0.058f, 1f));
        DrawFreeSpans(batch, snapshot, 0, snapshot.Size, stripPosition, stripSize, true, ui);
        DrawPackTrails(batch, snapshot, 0, snapshot.Size, stripPosition, stripSize, phase, showTrails, ui);
        DrawLiveRanges(batch, smallFont, text, runner, snapshot, 0, snapshot.Size, stripPosition, stripSize, phase, showLabels, showPadding, ui);
        DrawOutline(batch, stripPosition, stripSize, (0.22f, 0.25f, 0.28f, 1f), S(1f, ui));

        var detailPosition = stripPosition + (0, stripSize.Y + S(42f, ui));
        var detailSize = new Vec2(stripWidth, detailHeight);
        batch.Write(
            smallFont,
            DetailLabel(text, detailEnd, tailOmitted),
            detailPosition + (0, -S(28f, ui)),
            MutedText);
        batch.Draw(detailPosition, detailSize, (0.045f, 0.05f, 0.058f, 1f));
        DrawFreeSpans(batch, snapshot, 1, detailEnd, detailPosition, detailSize, false, ui);
        DrawPackTrails(batch, snapshot, 1, detailEnd, detailPosition, detailSize, phase, showTrails, ui);
        DrawLiveRanges(batch, smallFont, text, runner, snapshot, 1, detailEnd, detailPosition, detailSize, phase, showLabels, showPadding, ui);
        DrawOutline(batch, detailPosition, detailSize, (0.22f, 0.25f, 0.28f, 1f), S(1f, ui));
    }

    /// <summary>Draws a compact color legend for the memory views.</summary>
    private static void DrawLegend(SpriteBatchWriter batch, FontSize font, Vec2 position, float ui)
    {
        DrawLegendItem(batch, font, position, Free, "free block", ui);
        DrawLegendItem(batch, font, position + (S(140f, ui), 0), Palette(0), "live payload", ui);
        DrawLegendItem(batch, font, position + (S(305f, ui), 0), Padding, "padding", ui);
        DrawLegendItem(batch, font, position + (S(430f, ui), 0), RequestFill, "latest request", ui);
        DrawLegendItem(batch, font, position + (S(600f, ui), 0), Trail, "pack trail", ui);
    }

    /// <summary>Draws one legend swatch and label.</summary>
    private static void DrawLegendItem(SpriteBatchWriter batch, FontSize font, Vec2 position, Vec4 color, string label, float ui)
    {
        batch.Draw(position + (0, S(4f, ui)), (S(20f, ui), S(12f, ui)), color);
        batch.Write(font, label, position + (S(28f, ui), 0), MutedText);
    }

    /// <summary>Draws all derived free spans.</summary>
    private static void DrawFreeSpans(
        SpriteBatchWriter batch,
        AllocatorSnapshot snapshot,
        long viewStart,
        long viewEnd,
        Vec2 position,
        Vec2 size,
        bool muteTail,
        float ui)
    {
        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            if (!TryRangeRect(viewStart, viewEnd, span.Index, span.Size, position, size, ui, out var rectPosition, out var rectSize))
                continue;

            var color = muteTail && span.Index + span.Size == snapshot.Size && snapshot.Ranges.Length > 0 ? TailFree : Free;
            batch.Draw(rectPosition, rectSize, color);
            if (rectSize.X > S(5f, ui))
                batch.Draw(rectPosition, (Math.Max(S(1f, ui), rectSize.X), S(2f, ui)), FreeEdge);
        }
    }

    /// <summary>Draws movement trails created by pack operations.</summary>
    private static void DrawPackTrails(
        SpriteBatchWriter batch,
        AllocatorSnapshot snapshot,
        long viewStart,
        long viewEnd,
        Vec2 position,
        Vec2 size,
        float phase,
        bool showTrails,
        float ui)
    {
        if (!showTrails)
            return;

        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            if (!range.Moved)
                continue;

            if (!TryRangeRect(viewStart, viewEnd, range.LastIndex, range.ReservedSize, position, size, ui, out var oldPosition, out var oldSize))
                continue;
            if (!TryRangeRect(viewStart, viewEnd, range.Index, range.ReservedSize, position, size, ui, out var newPosition, out var newSize))
                continue;

            var oldCenter = oldPosition + (oldSize.X * 0.5f, oldSize.Y * 0.18f);
            var newCenter = newPosition + (newSize.X * 0.5f, newSize.Y * 0.18f);
            batch.Draw(oldPosition, oldSize, (0.98f, 0.86f, 0.35f, 0.18f * (1f - phase)));
            batch.DrawLine(oldCenter, newCenter, S(2f, ui), Trail);
        }
    }

    /// <summary>Draws live ranges and their payload/padding subregions.</summary>
    private static void DrawLiveRanges(
        SpriteBatchWriter batch,
        FontSize font,
        RootText text,
        AllocatorScenarioRunner runner,
        AllocatorSnapshot snapshot,
        long viewStart,
        long viewEnd,
        Vec2 position,
        Vec2 size,
        float phase,
        bool showLabels,
        bool showPadding,
        float ui)
    {
        var activeSlot = ActiveSlot(runner);
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            if (!TryRangeRect(viewStart, viewEnd, range.Index, range.ReservedSize, position, size, ui, out var rectPosition, out var rectSize))
                continue;

            var color = Palette(range.Slot);
            batch.Draw(rectPosition, rectSize, Dim(color, 0.42f));

            if (showPadding)
                DrawPadding(batch, viewStart, viewEnd, range, position, size, ui);

            if (!TryRangeRect(viewStart, viewEnd, range.PayloadIndex, range.Size, position, size, ui, out var payloadPosition, out var payloadSize))
                continue;

            var hasRequest = TryRequestBytes(runner, range.Slot, out var requestBytes);
            var requestReusesLargerPayload = activeSlot == range.Slot && hasRequest && requestBytes < range.Size;
            var pulse = activeSlot == range.Slot ? 0.7f + 0.3f * MathF.Sin(phase * MathF.PI) : 1f;
            if (requestReusesLargerPayload)
                pulse *= 0.58f;

            batch.Draw(payloadPosition, payloadSize, Dim(color, pulse));

            if (hasRequest && activeSlot == range.Slot)
                DrawRequestedPayload(batch, viewStart, viewEnd, range, requestBytes, position, size, ui);

            if (activeSlot == range.Slot)
                DrawActiveRangeHighlight(batch, rectPosition, rectSize, ui);

            if (showLabels)
                DrawRangeLabel(batch, font, text, runner, range, rectPosition, rectSize, ui);
        }
    }

    /// <summary>Draws leading and trailing reserved padding for a live range.</summary>
    private static void DrawPadding(
        SpriteBatchWriter batch,
        long viewStart,
        long viewEnd,
        AllocatorRangeVisual range,
        Vec2 position,
        Vec2 size,
        float ui)
    {
        if (range.LeadingPadding > 0)
        {
            if (TryRangeRect(viewStart, viewEnd, range.Index, range.LeadingPadding, position, size, ui, out var leadingPosition, out var leadingSize))
                batch.Draw(leadingPosition, leadingSize, Padding);
        }

        if (range.TrailingPadding > 0)
        {
            var trailingIndex = range.PayloadIndex + range.Size;
            if (TryRangeRect(viewStart, viewEnd, trailingIndex, range.TrailingPadding, position, size, ui, out var trailingPosition, out var trailingSize))
                batch.Draw(trailingPosition, trailingSize, Padding);
        }
    }

    /// <summary>Draws the operation timeline.</summary>
    private static void DrawTimeline(
        SpriteBatchWriter batch,
        FontSize font,
        RootText text,
        AllocatorScenario scenario,
        AllocatorScenarioRunner runner,
        Vec2 position,
        Vec2 size,
        float ui)
    {
        batch.Draw(position, size, Panel);
        batch.Write(font, text.Format("timeline {0}/{1}", runner.StepIndex, scenario.Commands.Length), position + (S(12f, ui), S(8f, ui)), MutedText);
        if (scenario.Commands.Length == 0)
            return;

        var lanePosition = position + (S(12f, ui), S(30f, ui));
        var laneSize = new Vec2(size.X - S(24f, ui), S(12f, ui));
        batch.Draw(lanePosition, laneSize, PanelSoft);
        var cellWidth = laneSize.X / scenario.Commands.Length;
        for (var i = 0; i < scenario.Commands.Length; i++)
        {
            var command = scenario.Commands[i];
            var color = CommandColor(command.Kind);
            if (i >= runner.StepIndex)
                color = Dim(color, 0.28f);

            var x = lanePosition.X + i * cellWidth;
            var width = Math.Max(S(1f, ui), cellWidth - S(1f, ui));
            batch.Draw((x, lanePosition.Y), (width, laneSize.Y), color);
        }

        var markerX = lanePosition.X + Math.Clamp(runner.StepIndex, 0, scenario.Commands.Length) * cellWidth;
        batch.Draw((markerX - S(1f, ui), lanePosition.Y - S(5f, ui)), (S(2f, ui), laneSize.Y + S(10f, ui)), Highlight);
    }

    /// <summary>Returns the current command slot when it has one.</summary>
    private static int ActiveSlot(AllocatorScenarioRunner runner) =>
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free
            ? runner.LastCommand.Slot
            : -1;

    /// <summary>Finds the live or just-freed range touched by the latest operation.</summary>
    private static bool TryTouchedRange(AllocatorScenarioRunner runner, out AllocatorRangeVisual range)
    {
        var command = runner.LastCommand;
        if (command.Kind is not (AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free))
        {
            range = default;
            return false;
        }

        var snapshot = command.Kind == AllocatorCommandKind.Free ? runner.Previous : runner.Current;
        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            if (snapshot.Ranges[i].Slot != command.Slot)
                continue;

            range = snapshot.Ranges[i];
            return true;
        }

        range = default;
        return false;
    }

    /// <summary>Returns the latest request size when the current operation targeted the supplied live slot.</summary>
    private static bool TryRequestBytes(AllocatorScenarioRunner runner, int slot, out long requestBytes)
    {
        var command = runner.LastCommand;
        if (command.Slot == slot && command.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc)
        {
            requestBytes = command.Size;
            return true;
        }

        requestBytes = 0;
        return false;
    }

    /// <summary>Draws the latest request as a bright sub-region inside the retained payload area.</summary>
    private static void DrawRequestedPayload(
        SpriteBatchWriter batch,
        long viewStart,
        long viewEnd,
        AllocatorRangeVisual range,
        long requestBytes,
        Vec2 position,
        Vec2 size,
        float ui)
    {
        var clampedRequestBytes = Math.Clamp(requestBytes, 0, range.Size);
        if (clampedRequestBytes <= 0)
            return;

        if (!TryRangeRect(viewStart, viewEnd, range.PayloadIndex, clampedRequestBytes, position, size, ui, out var requestPosition, out var requestSize))
            return;

        batch.Draw(requestPosition, requestSize, RequestFill);
        var markerWidth = S(2f, ui);
        var markerX = requestPosition.X + requestSize.X - markerWidth * 0.5f;
        batch.Draw((markerX, requestPosition.Y), (markerWidth, requestSize.Y), RequestEdge);
    }

    /// <summary>Draws a compact byte breakdown inside an allocation block when the text fits.</summary>
    private static void DrawRangeLabel(
        SpriteBatchWriter batch,
        FontSize font,
        RootText text,
        AllocatorScenarioRunner runner,
        AllocatorRangeVisual range,
        Vec2 position,
        Vec2 size,
        float ui)
    {
        var x = position.X + S(4f, ui);
        var y = position.Y + S(6f, ui);
        var maxWidth = size.X - S(8f, ui);
        var lineAdvance = font.Metrics.Height;
        var maxLines = (int)MathF.Floor((size.Y - S(8f, ui)) / lineAdvance);

        if (maxWidth <= S(22f, ui) || maxLines <= 0)
            return;

        var lineIndex = 0;
        var slotLabel = text.Format("#{0}", range.Slot);
        if (batch.Measure(font, slotLabel) <= maxWidth)
            batch.Write(font, slotLabel, (x, y + lineIndex * lineAdvance), Text);

        lineIndex++;
        if (lineIndex >= maxLines)
            return;

        if (TryRequestBytes(runner, range.Slot, out var requestBytes))
        {
            var requestLabel = text.Format("{0}B requested", requestBytes);
            if (batch.Measure(font, requestLabel) <= maxWidth)
                batch.Write(font, requestLabel, (x, y + lineIndex * lineAdvance), Text);

            lineIndex++;
            if (lineIndex >= maxLines)
                return;
        }

        var payloadLabel = text.Format("{0}B payload", range.Size);
        if (batch.Measure(font, payloadLabel) <= maxWidth)
            batch.Write(font, payloadLabel, (x, y + lineIndex * lineAdvance), Text);

        lineIndex++;
        if (lineIndex >= maxLines)
            return;

        var padding = range.ReservedSize - range.Size;
        var reservedLabel = padding == 0
            ? text.Format("{0}B reserved", range.ReservedSize)
            : text.Format("{0}B reserved, {1}B pad", range.ReservedSize, padding);
        if (batch.Measure(font, reservedLabel) <= maxWidth)
            batch.Write(font, reservedLabel, (x, y + lineIndex * lineAdvance), Text);
    }

    /// <summary>Returns the end byte of the active non-tail region when the final free block is large.</summary>
    private static long DetailEnd(AllocatorSnapshot snapshot, out bool tailOmitted)
    {
        tailOmitted = false;
        if (snapshot.Ranges.Length == 0 || snapshot.FreeSpans.Length == 0)
            return snapshot.Size;

        var lastFree = snapshot.FreeSpans[^1];
        if (lastFree.Index + lastFree.Size != snapshot.Size)
            return snapshot.Size;

        var activeEnd = Math.Max(2, lastFree.Index);
        tailOmitted = lastFree.Size > activeEnd;
        return tailOmitted ? activeEnd : snapshot.Size;
    }

    /// <summary>Formats the active zoom label, including whether the large tail free block is hidden.</summary>
    private static ReadOnlySpan<char> DetailLabel(RootText text, long detailEnd, bool tailOmitted) =>
        tailOmitted
            ? text.Format("active region zoom: bytes 1..{0}; large tail free block is omitted here", detailEnd)
            : text.Format("active region zoom: bytes 1..{0}; full usable range", detailEnd);

    /// <summary>Clips allocator byte coordinates to a visible byte window and converts them into a rectangle.</summary>
    private static bool TryRangeRect(
        long viewStart,
        long viewEnd,
        long index,
        long rangeSize,
        Vec2 position,
        Vec2 size,
        float ui,
        out Vec2 rectPosition,
        out Vec2 rectSize)
    {
        var clippedStart = Math.Max(index, viewStart);
        var clippedEnd = Math.Min(index + rangeSize, viewEnd);
        if (clippedEnd <= clippedStart)
        {
            rectPosition = default;
            rectSize = default;
            return false;
        }

        var scale = size.X / Math.Max(1f, viewEnd - viewStart);
        var x = position.X + (clippedStart - viewStart) * scale;
        var width = Math.Max(S(1.5f, ui), (clippedEnd - clippedStart) * scale);
        rectPosition = (x, position.Y);
        rectSize = (width, size.Y);
        return true;
    }

    /// <summary>Draws a rectangle outline using sprite rectangles.</summary>
    private static void DrawOutline(SpriteBatchWriter batch, Vec2 position, Vec2 size, Vec4 color, float thickness)
    {
        batch.Draw(position, (size.X, thickness), color);
        batch.Draw(position + (0, size.Y - thickness), (size.X, thickness), color);
        batch.Draw(position, (thickness, size.Y), color);
        batch.Draw(position + (size.X - thickness, 0), (thickness, size.Y), color);
    }

    /// <summary>Draws the active allocation as one connected four-sided frame plus a translucent body.</summary>
    private static void DrawActiveRangeHighlight(SpriteBatchWriter batch, Vec2 position, Vec2 size, float ui)
    {
        var pad = S(2f, ui);
        var thickness = Math.Min(S(3f, ui), Math.Max(S(1.5f, ui), Math.Min(size.X, size.Y) * 0.35f));
        var highlightPosition = position - (pad, pad);
        var highlightSize = size + (pad * 2f, pad * 2f);
        batch.Draw(highlightPosition, highlightSize, (1f, 0.96f, 0.55f, 0.16f));
        batch.Draw(highlightPosition, (highlightSize.X, thickness), Highlight);
        batch.Draw(highlightPosition + (0, highlightSize.Y - thickness), (highlightSize.X, thickness), Highlight);
        batch.Draw(highlightPosition, (thickness, highlightSize.Y), Highlight);
        batch.Draw(highlightPosition + (highlightSize.X - thickness, 0), (thickness, highlightSize.Y), Highlight);
    }

    /// <summary>Returns the command color used in the timeline.</summary>
    private static Vec4 CommandColor(AllocatorCommandKind kind) => kind switch
    {
        AllocatorCommandKind.Alloc => AllocColor,
        AllocatorCommandKind.Realloc => ResizeColor,
        AllocatorCommandKind.Free => FreeColor,
        AllocatorCommandKind.Pack => PackColor,
        _ => Text,
    };

    /// <summary>Returns a stable color for a handle slot.</summary>
    private static Vec4 Palette(int slot) => (slot % 8) switch
    {
        0 => (0.26f, 0.78f, 0.95f, 1f),
        1 => (0.95f, 0.46f, 0.52f, 1f),
        2 => (0.58f, 0.88f, 0.42f, 1f),
        3 => (0.98f, 0.72f, 0.3f, 1f),
        4 => (0.66f, 0.55f, 0.96f, 1f),
        5 => (0.35f, 0.88f, 0.72f, 1f),
        6 => (0.92f, 0.62f, 0.9f, 1f),
        _ => (0.68f, 0.78f, 0.92f, 1f),
    };

    /// <summary>Dims a color while preserving alpha.</summary>
    private static Vec4 Dim(Vec4 color, float factor) => (color.X * factor, color.Y * factor, color.Z * factor, color.W);

    /// <summary>Returns a safe ratio for drawing fill meters.</summary>
    private static float Fill(long value, long total) => total <= 0 ? 0f : Math.Clamp(value / (float)total, 0f, 1f);

    /// <summary>Scales a layout value by the root UI scale.</summary>
    private static float S(float value, float ui) => value * ui;
}
