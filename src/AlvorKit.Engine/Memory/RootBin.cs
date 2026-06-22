namespace AlvorKit.Engine;

/// <summary>Root-owned deferred OpenGL deletion bin.</summary>
[Root]
public sealed class RootBin(RootGl gl) : GlBin(gl, null);
