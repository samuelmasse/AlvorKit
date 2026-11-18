namespace AlvorKit;

/// <summary>Root-owned strict OpenGL layer for engine and game rendering.</summary>
[Root]
[ExcludeFromCodeCoverage]
public class RootGl(Gl inner) : GlLayer(inner);
