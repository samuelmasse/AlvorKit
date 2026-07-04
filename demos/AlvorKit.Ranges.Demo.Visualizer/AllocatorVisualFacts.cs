namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Shared derived facts and colors for allocator visualization.</summary>
internal static class AllocatorVisualFacts
{
    internal static Vec4 Free => (0.13f, 0.25f, 0.22f, 1f);
    internal static Vec4 TailFree => (0.09f, 0.14f, 0.14f, 1f);
    internal static Vec4 FreeEdge => (0.27f, 0.6f, 0.48f, 1f);
    internal static Vec4 Padding => (0.95f, 0.62f, 0.2f, 0.78f);
    internal static Vec4 Retained => (0.42f, 0.58f, 0.74f, 0.66f);
    internal static Vec4 RequestFill => (0.9f, 1f, 1f, 0.38f);
    internal static Vec4 RequestEdge => (1f, 0.96f, 0.55f, 0.95f);
    internal static Vec4 Highlight => (1f, 0.96f, 0.55f, 1f);
    internal static Vec4 AllocColor => (0.25f, 0.8f, 0.95f, 1f);
    internal static Vec4 FreeColor => (0.95f, 0.34f, 0.36f, 1f);
    internal static Vec4 PackColor => (0.78f, 0.56f, 1f, 1f);
    internal static Vec4 ResizeColor => (0.55f, 0.9f, 0.42f, 1f);

    /// <summary>Returns the command slot highlighted by the latest operation, if any.</summary>
    internal static int ActiveSlot(AllocatorScenarioRunner runner) =>
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free
            ? runner.LastCommand.Slot
            : -1;

    /// <summary>Returns whether the latest operation requested payload bytes for the supplied live slot.</summary>
    internal static bool IsLatestPayloadRequest(AllocatorScenarioRunner runner, int slot) =>
        runner.LastCommand.Slot == slot &&
        runner.LastCommand.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc;

    /// <summary>Finds the live or just-freed range touched by the latest operation.</summary>
    internal static bool TryTouchedRange(AllocatorScenarioRunner runner, out AllocatorRangeVisual range)
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

    /// <summary>Returns a stable color for a handle slot.</summary>
    internal static Vec4 Palette(int slot) => (slot % 8) switch
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

    /// <summary>Returns the command color used in the timeline.</summary>
    internal static Vec4 CommandColor(AllocatorCommandKind kind) => kind switch
    {
        AllocatorCommandKind.Alloc => AllocColor,
        AllocatorCommandKind.Realloc => ResizeColor,
        AllocatorCommandKind.Free => FreeColor,
        AllocatorCommandKind.Pack => PackColor,
        _ => (0.9f, 0.93f, 0.95f, 1f),
    };

    /// <summary>Dims a color while preserving alpha.</summary>
    internal static Vec4 Dim(Vec4 color, float factor) => (color.X * factor, color.Y * factor, color.Z * factor, color.W);

    /// <summary>Returns a safe ratio for drawing fill meters.</summary>
    internal static float Fill(long value, long total) => total <= 0 ? 0f : Math.Clamp(value / (float)total, 0f, 1f);
}
