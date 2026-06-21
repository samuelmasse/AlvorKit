namespace AlvorKit.Windowing;

/// <summary>Mouse wheel input data reported by a host window.</summary>
/// <param name="Offset">The horizontal and vertical wheel offset.</param>
public readonly record struct WindowMouseWheelEvent(Vec2 Offset);
