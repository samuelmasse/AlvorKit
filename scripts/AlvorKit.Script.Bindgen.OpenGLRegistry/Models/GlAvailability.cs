namespace AlvorKit.Script.Bindgen;

/// <summary>Availability attached to generated docs: desktop GL version/extension and optional ES version.</summary>
/// <param name="Gl">Desktop OpenGL version or extension that provides the symbol.</param>
/// <param name="GlEs">Optional OpenGL ES version that also provides the symbol.</param>
public sealed record GlAvailability(string Gl, string? GlEs);
