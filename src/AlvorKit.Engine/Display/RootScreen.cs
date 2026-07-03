namespace AlvorKit.Engine;

/// <summary>Root-scoped window screen, visibility, title, and close facade.</summary>
[Root]
public sealed class RootScreen(WindowLoop window) : WindowScreen(window);
