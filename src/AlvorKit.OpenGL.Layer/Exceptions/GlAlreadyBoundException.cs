namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when a nonzero object is bound over a target that already has one bound.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="message">The violation details to append after the GL function name.</param>
public sealed class GlAlreadyBoundException(string function, string message) : GlException(function, message);
