namespace AlvorKit.Engine;

/// <summary>Root-scoped mouse reader for buttons, wheel, position, and motion.</summary>
[Root]
public sealed class RootMouse(WindowLoop window) : Mouse(window);
