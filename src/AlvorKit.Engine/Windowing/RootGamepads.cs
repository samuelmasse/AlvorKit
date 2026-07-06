namespace AlvorKit.Engine;

/// <summary>Root-scoped polled gamepad reader.</summary>
[Root]
public class RootGamepads(WindowLoop window) : Gamepads(window);
