namespace AlvorKit.Windowing;

/// <summary>Keyboard input data reported by a host window.</summary>
/// <param name="Key">The AlvorKit key identifier.</param>
/// <param name="IsRepeat">Whether the host reported an auto-repeat press.</param>
public readonly record struct WindowKeyEvent(WindowKey Key, bool IsRepeat);
