namespace AlvorKit.Engine;

/// <summary>Root-scoped keyboard and text-input reader.</summary>
[Root]
public sealed class RootKeyboard(WindowLoop window) : Keyboard(window);
