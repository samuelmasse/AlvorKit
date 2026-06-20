namespace AlvorKit.Windowing;

/// <summary>Mouse button input data reported by a host window.</summary>
/// <param name="Button">The AlvorKit mouse button identifier.</param>
public readonly record struct WindowMouseButtonEvent(WindowMouseButton Button);
