namespace AlvorKit.Engine;

/// <summary>Root-scoped input mutation facade for clipboard, cursor mode, and cursor tracking.</summary>
[Root]
public sealed class RootInput(WindowLoop window) : WindowInput(window);
