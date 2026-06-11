namespace AlvorKit.RGFW;

/// <summary>Maps RGFW_eventType.</summary>
public enum RgfwEventType : byte
{
    None = 0,
    KeyPressed,
    KeyReleased,
    MouseButtonPressed,
    MouseButtonReleased,
    MouseScroll,
    MousePosChanged,
    WindowMoved,
    WindowResized,
    FocusIn,
    FocusOut,
    MouseEnter,
    MouseLeave,
    WindowRefresh,
    Quit,
    DataDrop,
    DataDrag,
    WindowMaximized,
    WindowMinimized,
    WindowRestored,
    ScaleUpdated
}
