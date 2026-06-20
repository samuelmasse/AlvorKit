namespace AlvorKit.Windowing;

/// <summary>Mouse movement data reported by a host window.</summary>
/// <param name="Position">The current cursor position in window coordinates.</param>
public readonly record struct WindowMouseMoveEvent(Vector2 Position);
