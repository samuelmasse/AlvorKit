namespace AlvorKit.Windowing;

/// <summary>Window resize data reported by a host window.</summary>
/// <param name="Size">The current drawable client size.</param>
public readonly record struct WindowResizeEvent(Vector2 Size);
