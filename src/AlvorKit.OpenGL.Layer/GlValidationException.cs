namespace AlvorKit.OpenGL.Layer;

/// <summary>Thrown when the validation layer detects a misuse of the OpenGL API.</summary>
public sealed class GlValidationException(string message) : Exception(message);
