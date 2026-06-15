namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when an unbind is attempted on a target that has nothing bound.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public sealed class GlNotBoundException(string function, string message) : GlException(function, message);
