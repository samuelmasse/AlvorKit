namespace AlvorKit.Windowing;

/// <summary>Window movement data reported by a host window.</summary>
/// <param name="Position">The current window position in screen coordinates.</param>
public readonly record struct WindowPositionEvent(Vec2i Position);
