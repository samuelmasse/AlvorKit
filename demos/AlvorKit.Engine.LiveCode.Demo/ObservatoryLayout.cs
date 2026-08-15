namespace AlvorKit;

/// <summary>Shared mapping between normalized colony anchors and engine canvas coordinates.</summary>
public static class ObservatoryLayout
{
    /// <summary>Returns the scaled width reserved for the live scope graph.</summary>
    public static float SidebarWidth(float uiScale) => 340f * uiScale;

    /// <summary>Maps one normalized anchor into the visible constellation field.</summary>
    public static Vec2 Center(Vec2 anchor, Vec2 canvas, float uiScale)
    {
        var horizontalMargin = 70f * uiScale;
        var verticalMargin = 90f * uiScale;
        var fieldWidth = Math.Max(320f * uiScale, canvas.X - SidebarWidth(uiScale));
        return (
            horizontalMargin + anchor.X * Math.Max(1f, fieldWidth - horizontalMargin * 2f),
            verticalMargin + anchor.Y * Math.Max(1f, canvas.Y - verticalMargin * 2f));
    }

    /// <summary>Maps a mouse position back into a bounded normalized anchor.</summary>
    public static Vec2 Anchor(Vec2 position, Vec2 canvas, float uiScale)
    {
        var horizontalMargin = 70f * uiScale;
        var verticalMargin = 90f * uiScale;
        var fieldWidth = Math.Max(320f * uiScale, canvas.X - SidebarWidth(uiScale));
        return (
            Math.Clamp(
                (position.X - horizontalMargin) / Math.Max(1f, fieldWidth - horizontalMargin * 2f),
                0f,
                1f),
            Math.Clamp(
                (position.Y - verticalMargin) / Math.Max(1f, canvas.Y - verticalMargin * 2f),
                0f,
                1f));
    }

    /// <summary>Returns whether a position is inside the constellation field.</summary>
    public static bool IsInField(Vec2 position, Vec2 canvas, float uiScale) =>
        position.X >= 0f
        && position.Y >= 0f
        && position.X < canvas.X - SidebarWidth(uiScale)
        && position.Y < canvas.Y;
}
