namespace AlvorKit.Engine;

/// <summary>Root-scoped view of the current drawable canvas.</summary>
[Root]
public class RootCanvas(WindowLoop window) : WindowCanvas(window);
